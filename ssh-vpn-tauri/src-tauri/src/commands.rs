use crate::vpn::VpnManager;
use crate::ssh_client::{ConnectionStatus, ServerInfo};
use crate::storage::{AppSettings, Storage};
use chrono::Utc;
use serde::{Deserialize, Serialize};
use std::sync::Arc;
use tauri::State;
use tokio::sync::Mutex;
use tracing::{error, info};
use uuid::Uuid;

use crate::ssh_client::SshClient;
use crate::bandwidth::BandwidthMonitor;

pub struct AppState {
    pub storage: Storage,
    pub ssh_client: Arc<Mutex<SshClient>>,
    pub bandwidth: Arc<BandwidthMonitor>,
    pub vpn_manager: Arc<Mutex<VpnManager>>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ServerConfig {
    pub host: String,
    pub port: u16,
    pub username: String,
    pub password: Option<String>,
    pub private_key_path: Option<String>,
}

impl From<ServerConfig> for ServerInfo {
    fn from(config: ServerConfig) -> Self {
        ServerInfo {
            id: Uuid::new_v4().to_string(),
            name: config.host.clone(),
            name_fa: None,
            host: config.host,
            port: config.port,
            username: config.username,
            password: config.password,
            private_key_path: config.private_key_path,
            country: None,
            city: None,
            priority: 0,
            is_active: false,
            created_at: Utc::now(),
            last_used: None,
        }
    }
}

/// Connect to SSH server
#[tauri::command]
pub async fn connect(
    state: State<'_, AppState>,
    config: ServerConfig,
) -> Result<ConnectionStatus, String> {
    info!("Connecting to {}:{}", config.host, config.port);
    
    let server: ServerInfo = config.into();
    state.storage.log_connection("CONNECTING", &server.name)
        .map_err(|e| e.to_string())?;
    
    // Load settings to get socks_port
    let settings = state.storage.load_settings()
        .map_err(|e| e.to_string())?;
    
    // Use the SSH client from AppState
    let mut client = state.ssh_client.lock().await;
    client.set_local_port(settings.socks_port);
    client.connect(&server).await.map_err(|e| e.to_string())
}

/// Disconnect from SSH server
#[tauri::command]
pub async fn disconnect(state: State<'_, AppState>) -> Result<(), String> {
    info!("Disconnecting");
    
    let mut client = state.ssh_client.lock().await;
    client.disconnect().map_err(|e| e.to_string())?;
    
    state.storage.log_connection("DISCONNECTED", "N/A")
        .map_err(|e| e.to_string())?;
    
    Ok(())
}

/// Get connection status
#[tauri::command]
pub async fn get_status(state: State<'_, AppState>) -> Result<ConnectionStatus, String> {
    let client = state.ssh_client.lock().await;
    Ok(client.get_status())
}

/// Get bandwidth stats
#[tauri::command]
pub fn get_bandwidth(state: State<'_, AppState>) -> crate::bandwidth::BandwidthStats {
    state.bandwidth.get_stats()
}

/// Add a new server
#[tauri::command]
pub fn add_server(
    state: State<'_, AppState>,
    server: ServerInfo,
) -> Result<String, String> {
    let id = Uuid::new_v4().to_string();
    let mut new_server = server;
    new_server.id = id.clone();
    new_server.created_at = Utc::now();
    
    state.storage.add_server(new_server)
        .map_err(|e| e.to_string())?;
    
    info!("Added server: {}", id);
    Ok(id)
}

/// Get all servers
#[tauri::command]
pub fn get_servers(state: State<'_, AppState>) -> Result<Vec<ServerInfo>, String> {
    state.storage.load_servers()
        .map_err(|e| e.to_string())
}

/// Update a server
#[tauri::command]
pub fn update_server(
    state: State<'_, AppState>,
    server: ServerInfo,
) -> Result<(), String> {
    let server_id = server.id.clone();
    state.storage.update_server(server)
        .map_err(|e| e.to_string())?;
    
    info!("Updated server: {}", server_id);
    Ok(())
}

/// Delete a server
#[tauri::command]
pub fn delete_server(
    state: State<'_, AppState>,
    id: String,
) -> Result<(), String> {
    state.storage.delete_server(&id)
        .map_err(|e| e.to_string())?;
    
    info!("Deleted server: {}", id);
    Ok(())
}

/// Set active server
#[tauri::command]
pub fn set_active_server(
    state: State<'_, AppState>,
    id: String,
) -> Result<(), String> {
    let mut servers = state.storage.load_servers()
        .map_err(|e| e.to_string())?;
    
    for server in servers.iter_mut() {
        server.is_active = server.id == id;
        if server.is_active {
            server.last_used = Some(Utc::now());
        }
    }
    
    state.storage.save_servers(&servers)
        .map_err(|e| e.to_string())?;
    
    info!("Set active server: {}", id);
    Ok(())
}

/// Get application settings
#[tauri::command]
pub fn get_settings(state: State<'_, AppState>) -> Result<AppSettings, String> {
    state.storage.load_settings()
        .map_err(|e| e.to_string())
}

/// Save application settings
#[tauri::command]
pub fn save_settings(
    state: State<'_, AppState>,
    settings: AppSettings,
) -> Result<(), String> {
    state.storage.save_settings(&settings)
        .map_err(|e| e.to_string())?;
    
    info!("Settings saved");
    Ok(())
}

/// Test server latency (ping)
#[tauri::command]
pub async fn test_latency(host: String, port: u16) -> Result<u32, String> {
    let start = std::time::Instant::now();
    
    let addr = format!("{}:{}", host, port);
    match tokio::net::TcpStream::connect(&addr).await {
        Ok(_) => {
            let elapsed = start.elapsed().as_millis() as u32;
            info!("Ping to {}:{} = {}ms", host, port, elapsed);
            Ok(elapsed)
        }
        Err(e) => {
            error!("Ping failed: {}", e);
            Err(format!("Connection failed: {}", e))
        }
    }
}

/// Get server location using ip-api.com
#[tauri::command]
pub async fn get_server_location(host: String) -> Result<serde_json::Value, String> {
    let url = format!("http://ip-api.com/json/{}", host);
    
    let response = reqwest::get(&url)
        .await
        .map_err(|e| e.to_string())?
        .json::<serde_json::Value>()
        .await
        .map_err(|e| e.to_string())?;
    
    Ok(response)
}

/// Get the application version
#[tauri::command]
pub fn get_app_version() -> String {
    env!("CARGO_PKG_VERSION").to_string()
}

/// Get public IP address
#[tauri::command]
pub async fn get_public_ip() -> Result<String, String> {
    let client = reqwest::Client::builder()
        .timeout(std::time::Duration::from_secs(10))
        .build()
        .map_err(|e| e.to_string())?;
    
    // Try multiple IP detection services
    let urls = [
        "https://api.ipify.org?format=json",
        "https://ifconfig.me/json",
        "https://api.myip.com",
    ];
    
    for url in &urls {
        if let Ok(response) = client.get(*url).send().await {
            if let Ok(text) = response.text().await {
                // Try to parse as JSON - check both "ip" and "ip_addr" keys
                if let Ok(json) = serde_json::from_str::<serde_json::Value>(&text) {
                    if let Some(ip) = json.get("ip")
                        .or_else(|| json.get("ip_addr"))
                        .and_then(|v| v.as_str()) 
                    {
                        return Ok(ip.to_string());
                    }
                }
                // Try plain text format - use proper IPAddr parser for IPv4/IPv6
                let trimmed = text.trim();
                if trimmed.parse::<std::net::IpAddr>().is_ok() {
                    return Ok(trimmed.to_string());
                }
            }
        }
    }
    
    Err("Failed to detect IP. Check your connection.".to_string())
}

/// Set system proxy (Windows)
#[cfg(windows)]
#[tauri::command]
pub fn set_system_proxy(port: u16) -> Result<(), String> {
    use std::process::Command;
    
    // Try using netsh first (affects more apps)
    let proxy_addr = format!("socks=127.0.0.1:{}", port);
    let output = Command::new("netsh")
        .args(["winhttp", "set", "proxy", &proxy_addr])
        .output();
    
    if let Ok(output) = output {
        if output.status.success() {
            info!("System proxy set via netsh to 127.0.0.1:{}", port);
            return Ok(());
        }
    }
    
    // Fallback to registry
    use winreg::enums::*;
    use winreg::RegKey;

    let hkcu = RegKey::predef(HKEY_CURRENT_USER);
    
    let (key, _) = hkcu.create_subkey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings")
        .map_err(|e| e.to_string())?;
    
    key.set_value("ProxyEnable", &1u32).map_err(|e| e.to_string())?;
    key.set_value("ProxyServer", &proxy_addr).map_err(|e| e.to_string())?;
    key.set_value("ProxyOverride", &"<local>").map_err(|e| e.to_string())?;
    
    info!("System proxy set via registry to 127.0.0.1:{}", port);
    Ok(())
}

/// Unset system proxy (Windows)
#[cfg(windows)]
#[tauri::command]
pub fn unset_system_proxy() -> Result<(), String> {
    use std::process::Command;
    
    // Try netsh first
    let _ = Command::new("netsh")
        .args(["winhttp", "reset", "proxy"])
        .output();
    
    // Also clear registry
    use winreg::enums::*;
    use winreg::RegKey;

    let hkcu = RegKey::predef(HKEY_CURRENT_USER);
    
    if let Ok(key) = hkcu.open_subkey_with_flags("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", KEY_WRITE) {
        let _ = key.set_value("ProxyEnable", &0u32);
        info!("System proxy disabled");
    }
    
    Ok(())
}

/// Start Global VPN
#[tauri::command]
pub async fn start_vpn(state: State<'_, AppState>) -> Result<(), String> {
    let settings = state.storage.load_settings().map_err(|e| e.to_string())?;
    let mut vpn = state.vpn_manager.lock().await;
    // Get the active server to get its host
    let servers = state.storage.load_servers().map_err(|e| e.to_string())?;
    let active_server = servers.iter().find(|s| s.is_active)
        .ok_or_else(|| "No active server selected".to_string())?;

    vpn.start(settings.socks_port, &active_server.host).await.map_err(|e| e.to_string())
}

/// Stop Global VPN
#[tauri::command]
pub async fn stop_vpn(state: State<'_, AppState>) -> Result<(), String> {
    let mut vpn = state.vpn_manager.lock().await;
    vpn.stop().map_err(|e| e.to_string())
}
