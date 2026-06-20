use thiserror::Error;

#[derive(Error, Debug)]
pub enum SshVpnError {
    #[error("Connection failed: {0}")]
    ConnectionFailed(String),
    
    #[error("Authentication failed: {0}")]
    AuthFailed(String),
    
    #[error("SOCKS proxy error: {0}")]
    SocksProxyError(String),
    
    #[error("Network error: {0}")]
    NetworkError(String),
    
    #[error("Configuration error: {0}")]
    ConfigError(String),
    
    #[error("Storage error: {0}")]
    StorageError(String),
    
    #[error("Firewall error: {0}")]
    FirewallError(String),
    
    #[error("DNS error: {0}")]
    DnsError(String),
}

impl serde::Serialize for SshVpnError {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        serializer.serialize_str(&self.to_string())
    }
}

pub type Result<T> = std::result::Result<T, SshVpnError>;