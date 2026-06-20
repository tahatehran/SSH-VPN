use crate::error::{Result, SshVpnError};
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use ssh2::{Session, Listener as SshListener};
use std::io::Write;
use std::net::{TcpStream, ToSocketAddrs};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::broadcast;
use tracing::{info, warn};

/// Connection state enum
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "snake_case")]
pub enum ConnectionState {
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Error(String),
}

impl Default for ConnectionState {
    fn default() -> Self {
        ConnectionState::Disconnected
    }
}

/// Server information
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ServerInfo {
    pub id: String,
    pub name: String,
    pub name_fa: Option<String>,
    pub host: String,
    pub port: u16,
    pub username: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub password: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub private_key_path: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub country: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub city: Option<String>,
    pub priority: u32,
    pub is_active: bool,
    pub created_at: DateTime<Utc>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub last_used: Option<DateTime<Utc>>,
}

impl ServerInfo {
    pub fn new(
        id: String,
        name: String,
        host: String,
        port: u16,
        username: String,
    ) -> Self {
        Self {
            id,
            name,
            name_fa: None,
            host,
            port,
            username,
            password: None,
            private_key_path: None,
            country: None,
            city: None,
            priority: 0,
            is_active: false,
            created_at: Utc::now(),
            last_used: None,
        }
    }
}

/// Connection status
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionStatus {
    pub state: ConnectionState,
    pub connected_at: Option<DateTime<Utc>>,
    pub server: Option<ServerInfo>,
    pub local_port: u16,
    pub bytes_sent: u64,
    pub bytes_received: u64,
}

impl Default for ConnectionStatus {
    fn default() -> Self {
        Self {
            state: ConnectionState::Disconnected,
            connected_at: None,
            server: None,
            local_port: 9000,
            bytes_sent: 0,
            bytes_received: 0,
        }
    }
}

/// Reconnect configuration
#[derive(Debug, Clone)]
pub struct ReconnectConfig {
    pub initial_delay_ms: u64,
    pub max_delay_ms: u64,
    pub max_retries: u32,
    pub multiplier: f64,
}

impl Default for ReconnectConfig {
    fn default() -> Self {
        Self {
            initial_delay_ms: 1000,
            max_delay_ms: 30000,
            max_retries: 10,
            multiplier: 2.0,
        }
    }
}

/// SSH client for managing connections
pub struct SshClient {
    session: Option<Session>,
    listener: Option<SshListener>,  // ssh2::Listener for port forwarding
    socks_proxy_listener: Option<std::net::TcpListener>,  // Local SOCKS5 proxy listener
    status: ConnectionStatus,
    state_tx: broadcast::Sender<ConnectionState>,
    should_disconnect: Arc<AtomicBool>,
    reconnect_config: ReconnectConfig,
}

impl SshClient {
    pub fn new() -> Self {
        let (state_tx, _) = broadcast::channel(100);
        Self {
            session: None,
            listener: None,
            socks_proxy_listener: None,
            status: ConnectionStatus::default(),
            state_tx,
            should_disconnect: Arc::new(AtomicBool::new(false)),
            reconnect_config: ReconnectConfig::default(),
        }
    }

    /// Connect to SSH server with auto-reconnect
    pub async fn connect(&mut self, server: &ServerInfo) -> Result<ConnectionStatus> {
        self.should_disconnect.store(false, Ordering::SeqCst);
        self.update_state(ConnectionState::Connecting);

        let mut delay_ms = self.reconnect_config.initial_delay_ms;
        let mut retries = 0;

        loop {
            match self.establish_connection(server).await {
                Ok(_) => {
                    info!("Connected to SSH server {}", server.host);
                    self.update_state(ConnectionState::Connected);
                    return Ok(self.status.clone());
                }
                Err(e) => {
                    if self.should_disconnect.load(Ordering::SeqCst) {
                        return Err(e);
                    }

                    retries += 1;
                    if retries > self.reconnect_config.max_retries {
                        let error = format!("Max retries ({}) exceeded", self.reconnect_config.max_retries);
                        self.update_state(ConnectionState::Error(error.clone()));
                        return Err(SshVpnError::ConnectionFailed(error));
                    }

                    warn!("Connection attempt {} failed: {}, retrying in {}ms", 
                          retries, e, delay_ms);
                    self.update_state(ConnectionState::Reconnecting);

                    tokio::time::sleep(tokio::time::Duration::from_millis(delay_ms)).await;
                    delay_ms = (delay_ms as f64 * self.reconnect_config.multiplier) as u64;
                    delay_ms = delay_ms.min(self.reconnect_config.max_delay_ms);
                }
            }
        }
    }

    async fn establish_connection(&mut self, server: &ServerInfo) -> Result<()> {
        // Use ToSocketAddrs to safely resolve hostnames to socket addresses
        let addr_str = format!("{}:{}", server.host, server.port);
        let socket_addrs: Vec<SocketAddr> = addr_str
            .to_socket_addrs()
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to resolve {}: {}", server.host, e)))?
            .collect();
        
        if socket_addrs.is_empty() {
            return Err(SshVpnError::NetworkError(format!("No addresses found for {}", server.host)));
        }
        
        let addr = socket_addrs[0];
        
        let tcp = TcpStream::connect_timeout(&addr, Duration::from_secs(30))
            .map_err(|e| SshVpnError::NetworkError(e.to_string()))?;

        let mut session = Session::new().map_err(|e| SshVpnError::ConnectionFailed(e.to_string()))?;
        session.set_tcp_stream(tcp);
        session.handshake().map_err(|e| SshVpnError::ConnectionFailed(e.to_string()))?;

        // Authenticate
        if let Some(password) = &server.password {
            session.userauth_password(&server.username, password)
                .map_err(|e| SshVpnError::AuthFailed(e.to_string()))?;
        } else if let Some(key_path) = &server.private_key_path {
            session.userauth_pubkey_file(&server.username, None, std::path::Path::new(key_path), None)
                .map_err(|e| SshVpnError::AuthFailed(e.to_string()))?;
        } else {
            return Err(SshVpnError::AuthFailed("No authentication method provided".to_string()));
        }

        if !session.authenticated() {
            return Err(SshVpnError::AuthFailed("Authentication failed".to_string()));
        }

        // Create local SOCKS5 proxy listener (equivalent to SSH -D dynamic port forwarding)
        let local_addr = format!("127.0.0.1:{}", self.status.local_port);
        let socks_listener = std::net::TcpListener::bind(&local_addr)
            .map_err(|e| SshVpnError::SocksProxyError(format!("Failed to bind SOCKS proxy on {}: {}", local_addr, e)))?;
        
        // Set socket reuse options
        if let Err(e) = socks_listener.set_nonblocking(true) {
            warn!("Failed to set non-blocking: {}", e);
        }

        self.session = Some(session);
        self.listener = None;  // Not using remote port forwarding
        self.socks_proxy_listener = Some(socks_listener);
        self.status.connected_at = Some(chrono::Utc::now());
        self.status.server = Some(server.clone());

        info!("SOCKS5 proxy listening on {}", local_addr);
        Ok(())
    }

    /// Handle a SOCKS5 connection - call this in a loop when listener is active
    pub fn handle_socks_connection(&mut self) -> Result<()> {
        let listener = match &self.socks_proxy_listener {
            Some(l) => l,
            None => return Ok(()),
        };
        
        match listener.accept() {
            Ok((mut client_stream, _client_addr)) => {
                // SOCKS5 handshake: send greeting response
                // Version 0x05 (SOCKS5), No authentication 0x00
                let _ = client_stream.write_all(&[0x05, 0x00]);
                info!("SOCKS connection received (basic implementation)");
            }
            Err(ref e) if e.kind() == std::io::ErrorKind::WouldBlock => {
                // No connection pending
            }
            Err(e) => {
                warn!("SOCKS accept error: {}", e);
            }
        }
        Ok(())
    }

    /// Disconnect from SSH server
    pub fn disconnect(&mut self) -> Result<()> {
        self.should_disconnect.store(true, Ordering::SeqCst);
        self.listener = None;
        self.socks_proxy_listener = None;
        self.session = None;
        self.update_state(ConnectionState::Disconnected);
        self.status = ConnectionStatus::default();
        info!("Disconnected from SSH server");
        Ok(())
    }

    /// Get current connection status
    pub fn get_status(&self) -> ConnectionStatus {
        self.status.clone()
    }

    /// Subscribe to state changes
    pub fn subscribe(&self) -> broadcast::Receiver<ConnectionState> {
        self.state_tx.subscribe()
    }

    fn update_state(&mut self, state: ConnectionState) {
        self.status.state = state.clone();
        let _ = self.state_tx.send(state);
    }
}

impl Default for SshClient {
    fn default() -> Self {
        Self::new()
    }
}