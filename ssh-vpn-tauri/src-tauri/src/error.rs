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
    
    #[error("IO error: {0}")]
    IoError(#[from] std::io::Error),
    
    #[error("SSH error: {0}")]
    SshError(#[from] ssh2::Error),
}

pub type Result<T> = std::result::Result<T, SshVpnError>;