use crate::error::{Result, SshVpnError};
use crate::ssh_client::ServerInfo;
use serde::{Deserialize, Serialize};
use std::fs;
use std::io::Write;
use std::path::PathBuf;
use tracing::info;

const APP_DIR: &str = "ssh-vpn-tauri";

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AppSettings {
    pub language: String,
    pub theme: String,
    pub auto_reconnect: bool,
    pub kill_switch: bool,
    pub dns_protection: bool,
    pub custom_dns: Vec<String>,
    pub check_interval_sec: u64,
    pub max_ping_ms: u32,
}

impl Default for AppSettings {
    fn default() -> Self {
        Self {
            language: "en".to_string(),
            theme: "system".to_string(),
            auto_reconnect: true,
            kill_switch: false,
            dns_protection: false,
            custom_dns: vec!["1.1.1.1".to_string(), "8.8.8.8".to_string()],
            check_interval_sec: 30,
            max_ping_ms: 200,
        }
    }
}

/// Storage manager for JSON file persistence
pub struct Storage {
    base_path: PathBuf,
}

impl Storage {
    pub fn new() -> Result<Self> {
        let base_path = dirs::config_dir()
            .ok_or_else(|| SshVpnError::StorageError("Cannot find config directory".to_string()))?
            .join(APP_DIR);

        fs::create_dir_all(&base_path)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        info!("Storage initialized at {:?}", base_path);
        Ok(Self { base_path })
    }

    fn servers_path(&self) -> PathBuf {
        self.base_path.join("servers.json")
    }

    fn settings_path(&self) -> PathBuf {
        self.base_path.join("settings.json")
    }

    fn connections_log_path(&self) -> PathBuf {
        self.base_path.join("connections.log")
    }

    /// Load all servers from storage
    pub fn load_servers(&self) -> Result<Vec<ServerInfo>> {
        let path = self.servers_path();
        if !path.exists() {
            return Ok(Vec::new());
        }

        let content = fs::read_to_string(&path)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        serde_json::from_str(&content)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))
    }

    /// Save all servers to storage
    pub fn save_servers(&self, servers: &[ServerInfo]) -> Result<()> {
        let content = serde_json::to_string_pretty(servers)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        fs::write(self.servers_path(), content)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        info!("Saved {} servers", servers.len());
        Ok(())
    }

    /// Add a new server
    pub fn add_server(&self, server: ServerInfo) -> Result<String> {
        let mut servers = self.load_servers()?;
        servers.push(server.clone());
        self.save_servers(&servers)?;
        Ok(server.id)
    }

    /// Update an existing server
    pub fn update_server(&self, server: ServerInfo) -> Result<()> {
        let mut servers = self.load_servers()?;
        if let Some(existing) = servers.iter_mut().find(|s| s.id == server.id) {
            *existing = server;
            self.save_servers(&servers)?;
            Ok(())
        } else {
            Err(SshVpnError::StorageError("Server not found".to_string()))
        }
    }

    /// Delete a server by ID
    pub fn delete_server(&self, id: &str) -> Result<()> {
        let mut servers = self.load_servers()?;
        let original_len = servers.len();
        servers.retain(|s| s.id != id);

        if servers.len() == original_len {
            return Err(SshVpnError::StorageError("Server not found".to_string()));
        }

        self.save_servers(&servers)?;
        Ok(())
    }

    /// Load application settings
    pub fn load_settings(&self) -> Result<AppSettings> {
        let path = self.settings_path();
        if !path.exists() {
            return Ok(AppSettings::default());
        }

        let content = fs::read_to_string(&path)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        serde_json::from_str(&content)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))
    }

    /// Save application settings
    pub fn save_settings(&self, settings: &AppSettings) -> Result<()> {
        let content = serde_json::to_string_pretty(settings)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        fs::write(self.settings_path(), content)
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        info!("Settings saved");
        Ok(())
    }

    /// Log a connection event
    pub fn log_connection(&self, event: &str, server_name: &str) -> Result<()> {
        let timestamp = chrono::Utc::now().to_rfc3339();
        let log_line = format!("[{}] {} - {}\n", timestamp, event, server_name);
        
        fs::OpenOptions::new()
            .create(true)
            .append(true)
            .open(self.connections_log_path())
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?
            .write_all(log_line.as_bytes())
            .map_err(|e| SshVpnError::StorageError(e.to_string()))?;

        Ok(())
    }
}

impl Default for Storage {
    fn default() -> Self {
        Self::new().expect("Failed to initialize storage")
    }
}