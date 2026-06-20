use crate::error::{Result, SshVpnError};
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use ssh2::Session;
use std::io::{Read, Write};
use std::net::TcpStream;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use tokio::sync::broadcast;
use tracing::{error, info, warn};

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
    listener: Option<tokio::net::TcpListener>,
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
        let tcp = TcpStream::connect_timeout(
            &format!("{}:{}", server.host, server.port).parse().unwrap(),
            std::time::Duration::from_secs(30),
        )
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

        // Create SOCKS5 proxy listener
        let listener = session
            .channel_forward_listen(9000)
            .map_err(|e| SshVpnError::SocksProxyError(e.to_string()))?;

        self.session = Some(session);
        self.listener = Some(listener);
        self.status.connected_at = Some(chrono::Utc::now());
        self.status.server = Some(server.clone());

        Ok(())
    }

    /// Disconnect from SSH server
    pub fn disconnect(&mut self) -> Result<()> {
        self.should_disconnect.store(true, Ordering::SeqCst);
        self.listener = None;
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