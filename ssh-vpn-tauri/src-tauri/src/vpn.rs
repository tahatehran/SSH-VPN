use crate::routing::RoutingManager;
use std::sync::Arc;
use tracing::{info, error};
use crate::error::{Result, SshVpnError};
use std::process::Command;

pub struct VpnManager {
    wintun: Option<Arc<wintun::Adapter>>,
    should_stop: Arc<std::sync::atomic::AtomicBool>,
    routing: RoutingManager,
}

impl VpnManager {
    pub fn new() -> Self {
        Self {
            wintun: None,
            should_stop: Arc::new(std::sync::atomic::AtomicBool::new(false)),
            routing: RoutingManager::new(),
        }
    }

    pub async fn start(&mut self, socks_port: u16, ssh_host: &str, dns_servers: &[String]) -> Result<()> {
        if self.wintun.is_some() {
            return Ok(());
        }

        info!("Starting Global VPN mode (Wintun)");
        self.should_stop.store(false, std::sync::atomic::Ordering::SeqCst);

        let wintun_lib = unsafe {
            wintun::load()
                .map_err(|e| SshVpnError::NetworkError(format!("Failed to load wintun.dll: {}", e)))?
        };

        // wintun::Adapter::create in v0.4 returns Result<Arc<Adapter>, Error>
                let adapter = wintun::Adapter::create(&wintun_lib, "SSH VPN Tunnel", "SSH VPN Tunnel", None)
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to create Wintun adapter: {}", e)))?;
        let adapter = Arc::new(adapter);

        info!("Configuring interface {} with IP 10.10.10.1", interface_name);

        // Set IP and Mask
        let output = Command::new("netsh")
            .args(["interface", "ip", "set", "address", &interface_name, "static", "10.10.10.1", "255.255.255.0", "none"])
            .output()
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to run netsh for IP: {}", e)))?;

        if !output.status.success() {
            let err = String::from_utf8_lossy(&output.stderr);
            tracing::warn!("Netsh IP set warning: {}", err);
        }

        let _ = Command::new("netsh")
            .args(["interface", "ipv4", "set", "subinterface", &interface_name, "mtu=1500", "store=active"])
            .output();

        Ok(())
    }

    async fn run_tun_to_socks(adapter: Arc<wintun::Adapter>, socks_port: u16, stop_flag: Arc<std::sync::atomic::AtomicBool>) -> Result<()> {
        let session = adapter.start_session(wintun::MAX_RING_CAPACITY)
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to start Wintun session: {}", e)))?;

        let session = Arc::new(session);
        info!("Wintun session active. Forwarding to SOCKS port {}", socks_port);

        info!("Entering TUN to SOCKS bridge loop");
        while !stop_flag.load(std::sync::atomic::Ordering::SeqCst) {
            match session.receive_blocking() {
                Ok(packet) => {
                    let data = packet.bytes();
                    // Basic logging for activity tracking
                    // In a full implementation, this is where smoltcp would process the IP packet
                    if data.len() > 9 {
                        let protocol = data[9];
                        if protocol == 17 {
                            // UDP detected - we could log source/dest if needed
                        } else if protocol == 6 {
                            // TCP detected
                        }
                    }
                }
                Err(e) => {
                    if stop_flag.load(std::sync::atomic::Ordering::SeqCst) {
                        break;
                    }
                    warn!("Wintun receive non-fatal error or timeout: {}", e);
                    // Avoid tight loop on repeated errors
                    std::thread::sleep(std::time::Duration::from_millis(10));
                }
            }
        }

        info!("TUN to SOCKS worker stopped");
        Ok(())
    }

    pub fn stop(&mut self) -> Result<()> {
        let _ = self.routing.restore_routing();
        info!("Stopping Global VPN mode");
        self.should_stop.store(true, std::sync::atomic::Ordering::SeqCst);

        if let Some(adapter) = self.wintun.take() {
            drop(adapter);
        }

        Ok(())
    }
}
