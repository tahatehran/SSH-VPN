use crate::error::{Result, SshVpnError};
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use ssh2::{Channel, Session};
use std::io::{Read, Write};
use std::net::{TcpStream, SocketAddr, ToSocketAddrs};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use std::time::Duration;
use tokio::sync::broadcast;
use tracing::{info, warn};

/// Shared state for SOCKS proxy handling across threads
#[derive(Clone)]
pub struct SocksProxyHandle {
    pub session: Arc<Mutex<Option<Session>>>,
    pub listener: Arc<Mutex<Option<std::net::TcpListener>>>,
    pub should_stop: Arc<AtomicBool>,
}

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
    socks_handle: SocksProxyHandle,  // Shared SOCKS proxy state
    status: ConnectionStatus,
    state_tx: broadcast::Sender<ConnectionState>,
    should_disconnect: Arc<AtomicBool>,
    reconnect_config: ReconnectConfig,
}

impl SshClient {
    pub fn new() -> Self {
        let (state_tx, _) = broadcast::channel(100);
        Self {
            socks_handle: SocksProxyHandle {
                session: Arc::new(Mutex::new(None)),
                listener: Arc::new(Mutex::new(None)),
                should_stop: Arc::new(AtomicBool::new(false)),
            },
            status: ConnectionStatus::default(),
            state_tx,
            should_disconnect: Arc::new(AtomicBool::new(false)),
            reconnect_config: ReconnectConfig::default(),
        }
    }

    /// Set the local SOCKS proxy port
    pub fn set_local_port(&mut self, port: u16) {
        self.status.local_port = port;
    }

    /// Connect to SSH server with auto-reconnect
    pub async fn connect(&mut self, server: &ServerInfo) -> Result<ConnectionStatus> {
        self.should_disconnect.store(false, Ordering::SeqCst);
        self.socks_handle.should_stop.store(false, Ordering::SeqCst);
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

        *self.socks_handle.session.lock().unwrap() = Some(session);
        *self.socks_handle.listener.lock().unwrap() = Some(socks_listener);
        self.status.connected_at = Some(chrono::Utc::now());
        self.status.server = Some(server.clone());

        info!("SOCKS5 proxy listening on {}", local_addr);
        
        // Start background SOCKS proxy loop
        let handle = self.get_socks_handle();
        std::thread::spawn(move || {
            SshClient::socks_proxy_loop(handle);
        });
        
        Ok(())
    }

    /// Handle a SOCKS5 connection - call this in a loop when listener is active
    pub fn handle_socks_connection(&self) -> Result<()> {
        let listener_guard = match self.socks_handle.listener.lock() {
            Ok(guard) => guard,
            Err(e) => return Err(SshVpnError::SocksProxyError(format!("Lock poisoned: {}", e))),
        };
        
        let listener = match listener_guard.as_ref() {
            Some(l) => l,
            None => return Ok(()),
        };
        
        let session = Arc::clone(&self.socks_handle.session);
        
        match listener.accept() {
            Ok((mut client_stream, client_addr)) => {
                info!("SOCKS connection from {}", client_addr);
                
                // Handle in a blocking task with session reference
                std::thread::spawn(move || {
                    if let Err(e) = Self::handle_socks5_connection_internal(&mut client_stream, session) {
                        warn!("SOCKS5 handling error: {}", e);
                    }
                });
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

    /// Get a clone of the SOCKS proxy handle for background task
    pub fn get_socks_handle(&self) -> SocksProxyHandle {
        SocksProxyHandle {
            session: Arc::clone(&self.socks_handle.session),
            listener: Arc::clone(&self.socks_handle.listener),
            should_stop: Arc::clone(&self.socks_handle.should_stop),
        }
    }

    /// Start background task to poll SOCKS connections
    pub fn socks_proxy_loop(socks_handle: SocksProxyHandle) {
        info!("SOCKS proxy loop started");
        while !socks_handle.should_stop.load(Ordering::SeqCst) {
            let listener_guard = match socks_handle.listener.lock() {
                Ok(guard) => guard,
                Err(e) => {
                    warn!("Listener lock poisoned: {}", e);
                    std::thread::sleep(std::time::Duration::from_millis(100));
                    continue;
                }
            };
            
            if let Some(ref listener) = *listener_guard {
                match listener.accept() {
                    Ok((mut client_stream, client_addr)) => {
                        info!("SOCKS connection from {}", client_addr);
                        let session = Arc::clone(&socks_handle.session);
                        drop(listener_guard); // Release lock before spawning thread
                        std::thread::spawn(move || {
                            if let Err(e) = Self::handle_socks5_connection_internal(&mut client_stream, session) {
                                warn!("SOCKS5 handling error: {}", e);
                            }
                        });
                    }
                    Err(ref e) if e.kind() == std::io::ErrorKind::WouldBlock => {
                        // No connection pending, sleep briefly
                        drop(listener_guard);
                        std::thread::sleep(std::time::Duration::from_millis(10));
                    }
                    Err(e) => {
                        warn!("SOCKS accept error: {}", e);
                        drop(listener_guard);
                        std::thread::sleep(std::time::Duration::from_millis(100));
                    }
                }
            } else {
                // No listener, sleep and wait
                drop(listener_guard);
                std::thread::sleep(std::time::Duration::from_millis(100));
            }
        }
        info!("SOCKS proxy loop stopped");
    }

    /// Internal SOCKS5 connection handler - with actual SSH forwarding
    fn handle_socks5_connection_internal(
        client_stream: &mut std::net::TcpStream,
        session: Arc<Mutex<Option<Session>>>,
    ) -> Result<()> {
        // SOCKS5 handshake
        let mut buf = [0u8; 2];
        client_stream.read_exact(&mut buf)?;
        
        if buf[0] != 0x05 {
            return Err(SshVpnError::SocksProxyError("Invalid SOCKS version".to_string()));
        }
        
        let num_auth_methods = buf[1] as usize;
        let mut auth_methods = vec![0u8; num_auth_methods];
        client_stream.read_exact(&mut auth_methods)?;
        
        // We only support NO_AUTH (0x00)
        client_stream.write_all(&[0x05, 0x00])?;
        
        // SOCKS5 request
        let mut request = [0u8; 4];
        client_stream.read_exact(&mut request)?;
        
        if request[0] != 0x05 {
            return Err(SshVpnError::SocksProxyError("Invalid SOCKS version in request".to_string()));
        }
        
        let cmd = request[1]; // CMD: 0x01 = CONNECT, 0x02 = BIND, 0x03 = UDP ASSOCIATE
        
        // Parse destination address
        let (dst_host, dst_port) = match request[3] {
            0x01 => { // IPv4
                let mut addr = [0u8; 4];
                client_stream.read_exact(&mut addr)?;
                let host = format!("{}.{}.{}.{}", addr[0], addr[1], addr[2], addr[3]);
                let mut port = [0u8; 2];
                client_stream.read_exact(&mut port)?;
                let port = u16::from_be_bytes(port);
                (host, port)
            }
            0x03 => { // Domain name
                let len = {
                    let mut b = [0u8; 1];
                    client_stream.read_exact(&mut b)?;
                    b[0] as usize
                };
                let mut domain = vec![0u8; len];
                client_stream.read_exact(&mut domain)?;
                let host = String::from_utf8(domain)
                    .map_err(|e| SshVpnError::SocksProxyError(format!("Invalid domain: {}", e)))?;
                let mut port = [0u8; 2];
                client_stream.read_exact(&mut port)?;
                let port = u16::from_be_bytes(port);
                (host, port)
            }
            0x04 => { // IPv6
                let mut addr = [0u8; 16];
                client_stream.read_exact(&mut addr)?;
                let host = format!("{:x?}", &addr);
                let mut port = [0u8; 2];
                client_stream.read_exact(&mut port)?;
                let port = u16::from_be_bytes(port);
                (host, port)
            }
            _ => return Err(SshVpnError::SocksProxyError("Unsupported address type".to_string())),
        };
        
        info!("SOCKS5 connect request: {}:{}", dst_host, dst_port);
        
        if cmd != 0x01 {
            // We only support CONNECT
            client_stream.write_all(&[0x05, 0x07, 0x00, 0x01])?; // Command not supported
            return Err(SshVpnError::SocksProxyError("Only CONNECT command supported".to_string()));
        }
        
        // Get session from Arc<Mutex>
        let session_guard = session.lock().unwrap();
        let ssh_session = match session_guard.as_ref() {
            Some(s) => s,
            None => {
                client_stream.write_all(&[0x05, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00])?;
                return Err(SshVpnError::SocksProxyError("SSH session not available".to_string()));
            }
        };
        
        // Open SSH channel for direct TCP
        let mut channel = ssh_session.channel_direct_tcpip(&dst_host, dst_port)
            .map_err(|e| SshVpnError::SocksProxyError(format!("Failed to open SSH channel: {}", e)))?;
        
        // Send success response
        client_stream.write_all(&[0x05, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00])?;
        
        info!("SOCKS5 connection to {}:{} forwarded via SSH", dst_host, dst_port);
        
        // Drop the lock before copying data
        drop(session_guard);
        
        // Copy data between client and SSH channel
        let mut buf = [0u8; 8192];
        loop {
            // Non-blocking read from client
            match client_stream.read(&mut buf) {
                Ok(0) => {
                    info!("Client closed connection");
                    break;
                }
                Ok(n) => {
                    channel.write_all(&buf[..n])?;
                }
                Err(ref e) if e.kind() == std::io::ErrorKind::WouldBlock => {
                    // No data available, try reading from channel
                }
                Err(e) => {
                    warn!("Error reading from client: {}", e);
                    break;
                }
            }
            
            // Non-blocking read from channel
            match channel.read(&mut buf) {
                Ok(0) => {
                    info!("SSH channel closed");
                    break;
                }
                Ok(n) => {
                    client_stream.write_all(&buf[..n])?;
                }
                Err(ref e) if e.kind() == std::io::ErrorKind::WouldBlock => {
                    // No data available
                }
                Err(e) => {
                    warn!("Error reading from SSH channel: {}", e);
                    break;
                }
            }
        }
        
        channel.close()?;
        info!("SOCKS5 tunnel closed for {}:{}", dst_host, dst_port);
        Ok(())
    }

    /// Disconnect from SSH server
    pub fn disconnect(&mut self) -> Result<()> {
        self.should_disconnect.store(true, Ordering::SeqCst);
        self.socks_handle.should_stop.store(true, Ordering::SeqCst);
        *self.socks_handle.listener.lock().unwrap() = None;
        *self.socks_handle.session.lock().unwrap() = None;
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