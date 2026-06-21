use std::sync::Arc;
use tokio::sync::{Mutex, broadcast};
use tokio::io::{AsyncReadExt, AsyncWriteExt};
use tokio::net::TcpListener;
use tracing::{info, warn, error};
use serde::{Deserialize, Serialize};
use chrono::{DateTime, Utc};
use std::sync::atomic::{AtomicBool, Ordering};
use crate::error::{Result, SshVpnError};
use async_ssh2_tokio::client::{Client, AuthMethod};
use crate::debug::{DebugManager, LogLevel};
use crate::bandwidth::BandwidthMonitor;

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(rename_all = "lowercase")]
pub enum ConnectionState {
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Error(String),
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ServerInfo {
    pub id: String,
    pub name: String,
    pub name_fa: Option<String>,
    pub host: String,
    pub port: u16,
    pub username: String,
    pub password: Option<String>,
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

pub struct SshClient {
    client: Arc<Mutex<Option<Client>>>,
    status: ConnectionStatus,
    state_tx: broadcast::Sender<ConnectionState>,
    should_stop: Arc<AtomicBool>,
    debug_manager: Option<Arc<DebugManager>>,
    bandwidth: Option<Arc<BandwidthMonitor>>,
}

impl SshClient {
    pub fn new() -> Self {
        let (state_tx, _) = broadcast::channel(100);
        Self {
            client: Arc::new(Mutex::new(None)),
            status: ConnectionStatus::default(),
            state_tx,
            should_stop: Arc::new(AtomicBool::new(false)),
            debug_manager: None,
            bandwidth: None,
        }
    }

    pub fn set_debug_manager(&mut self, debug_manager: Arc<DebugManager>) {
        self.debug_manager = Some(debug_manager);
    }

    pub fn set_bandwidth(&mut self, bandwidth: Arc<BandwidthMonitor>) {
        self.bandwidth = Some(bandwidth);
    }

    pub fn set_local_port(&mut self, port: u16) {
        self.status.local_port = port;
    }

    pub async fn connect(&mut self, server: &ServerInfo) -> Result<ConnectionStatus> {
        let dm = self.debug_manager.clone();
        if let Some(ref d) = dm {
            d.log(LogLevel::Info, "SSH", &format!("Initiating connection to {}:{}", server.host, server.port)).await;
        }

        self.update_state(ConnectionState::Connecting);
        self.should_stop.store(false, Ordering::SeqCst);

        let auth_method = if let Some(password) = &server.password {
            AuthMethod::with_password(password)
        } else {
            let msg = "No authentication method provided".to_string();
            if let Some(ref d) = dm { d.log(LogLevel::Error, "SSH", &msg).await; }
            return Err(SshVpnError::AuthFailed(msg));
        };

        match Client::connect((server.host.as_str(), server.port), &server.username, auth_method).await {
            Ok(client) => {
                if let Some(ref d) = dm { d.log(LogLevel::Info, "SSH", "Handshake and Auth successful").await; }
                *self.client.lock().await = Some(client);
                self.status.connected_at = Some(Utc::now());
                self.status.server = Some(server.clone());
                self.update_state(ConnectionState::Connected);

                let port = self.status.local_port;
                let client_arc = Arc::clone(&self.client);
                let stop_flag = Arc::clone(&self.should_stop);
                let dm_proxy = dm.clone();
                let bw_proxy = self.bandwidth.clone();
                
                tokio::spawn(async move {
                    if let Err(e) = Self::run_socks_proxy(port, client_arc, stop_flag, dm_proxy, bw_proxy).await {
                        error!("SOCKS proxy error: {}", e);
                    }
                });

                Ok(self.status.clone())
            }
            Err(e) => {
                let err_msg = format!("SSH Core Error: {}", e);
                if let Some(ref d) = dm { d.log(LogLevel::Error, "SSH", &err_msg).await; }
                self.update_state(ConnectionState::Error(err_msg.clone()));
                Err(SshVpnError::ConnectionFailed(err_msg))
            }
        }
    }

    async fn run_socks_proxy(port: u16, client: Arc<Mutex<Option<Client>>>, stop_flag: Arc<AtomicBool>, dm: Option<Arc<DebugManager>>, bw: Option<Arc<BandwidthMonitor>>) -> Result<()> {
        let listener = TcpListener::bind(format!("127.0.0.1:{}", port)).await?;
        if let Some(ref d) = dm { d.log(LogLevel::Info, "SOCKS", &format!("Internal proxy active on port {}", port)).await; }

        while !stop_flag.load(Ordering::SeqCst) {
            let (mut stream, addr) = match tokio::time::timeout(std::time::Duration::from_millis(500), listener.accept()).await {
                Ok(Ok(res)) => res,
                _ => continue,
            };

            let client_arc = Arc::clone(&client);
            let dm_conn = dm.clone();
            let bw_conn = bw.clone();

            tokio::spawn(async move {
                if let Err(e) = Self::handle_socks_connection(&mut stream, client_arc, dm_conn, bw_conn).await {
                    warn!("Connection handling failed for {}: {}", addr, e);
                }
            });
        }
        Ok(())
    }

    async fn handle_socks_connection(stream: &mut tokio::net::TcpStream, client_arc: Arc<Mutex<Option<Client>>>, dm: Option<Arc<DebugManager>>, bw: Option<Arc<BandwidthMonitor>>) -> Result<()> {
        let mut buf = [0u8; 2];
        stream.read_exact(&mut buf).await?;
        if buf[0] != 0x05 { return Ok(()); }
        let n_methods = buf[1] as usize;
        let mut methods = vec![0u8; n_methods];
        stream.read_exact(&mut methods).await?;
        stream.write_all(&[0x05, 0x00]).await?;

        let mut req = [0u8; 4];
        stream.read_exact(&mut req).await?;
        if req[1] != 0x01 { return Ok(()); }

        let (dst_host, dst_port) = match req[3] {
            0x01 => {
                let mut ip = [0u8; 4];
                stream.read_exact(&mut ip).await?;
                (format!("{}.{}.{}.{}", ip[0], ip[1], ip[2], ip[3]), u16::from_be_bytes([stream.read_u8().await?, stream.read_u8().await?]))
            }
            0x03 => {
                let len = stream.read_u8().await? as usize;
                let mut domain = vec![0u8; len];
                stream.read_exact(&mut domain).await?;
                let port = u16::from_be_bytes([stream.read_u8().await?, stream.read_u8().await?]);
                (String::from_utf8_lossy(&domain).to_string(), port)
            }
            _ => return Ok(()),
        };

        if let Some(ref d) = dm { d.log(LogLevel::Debug, "TUNNEL", &format!("Requesting channel to {}:{}", dst_host, dst_port)).await; }

        let guard = client_arc.lock().await;
        if let Some(client) = guard.as_ref() {
            match client.handle().channel_direct_tcpip(&dst_host, dst_port, None).await {
                Ok(mut channel) => {
                    stream.write_all(&[0x05, 0x00, 0x00, 0x01, 0, 0, 0, 0, 0, 0]).await?;

                    let (mut ri, mut wi) = tokio::io::split(stream);
                    let (mut rc, mut wc) = tokio::io::split(channel);

                    let client_to_ssh = tokio::io::copy(&mut ri, &mut wc);
                    let ssh_to_client = tokio::io::copy(&mut rc, &mut wi);

                    match tokio::join!(client_to_ssh, ssh_to_client) {
                        (Ok(sent), Ok(received)) => {
                            if let Some(ref b) = bw {
                                b.add_sent(sent);
                                b.add_received(received);
                            }
                        }
                        _ => {}
                    }
                }
                Err(e) => {
                    if let Some(ref d) = dm { d.log(LogLevel::Warning, "SSH", &format!("Channel failed for {}: {}", dst_host, e)).await; }
                    let _ = stream.write_all(&[0x05, 0x01, 0x00, 0x01, 0, 0, 0, 0, 0, 0]).await;
                }
            }
        }
        Ok(())
    }

    pub fn disconnect(&mut self) -> Result<()> {
        self.should_stop.store(true, Ordering::SeqCst);
        self.update_state(ConnectionState::Disconnected);
        Ok(())
    }

    pub fn get_status(&self) -> ConnectionStatus {
        self.status.clone()
    }

    fn update_state(&mut self, state: ConnectionState) {
        self.status.state = state.clone();
        let _ = self.state_tx.send(state);
    }
}
