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

    pub async fn start(&mut self, socks_port: u16, ssh_host: &str) -> Result<()> {
        if self.wintun.is_some() {
            return Ok(());
        }

        info!("Starting Global VPN mode (Wintun)");
        self.should_stop.store(false, std::sync::atomic::Ordering::SeqCst);

        let wintun = unsafe {
            wintun::load()
                .map_err(|e| SshVpnError::NetworkError(format!("Failed to load wintun.dll: {}", e)))?
        };

        let adapter = Arc::new(wintun::Adapter::create(&wintun, "SSHVPN", "SSH VPN Tunnel", None)
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to create Wintun adapter: {}", e)))?);

        self.configure_interface(&adapter)?;
        self.routing.setup_routing(ssh_host)?;

        self.wintun = Some(Arc::clone(&adapter));

        let adapter_worker = Arc::clone(&adapter);
        let stop_flag = Arc::clone(&self.should_stop);

        tokio::spawn(async move {
            if let Err(e) = Self::run_tun_to_socks(adapter_worker, socks_port, stop_flag).await {
                error!("TUN to SOCKS worker failed: {}", e);
            }
        });

        Ok(())
    }

    fn configure_interface(&self, adapter: &wintun::Adapter) -> Result<()> {
        let interface_name = adapter.get_name()
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to get adapter name: {}", e)))?;

        info!("Configuring interface {} with IP 10.10.10.1", interface_name);

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

        while !stop_flag.load(std::sync::atomic::Ordering::SeqCst) {
            match session.receive_blocking() {
                Ok(packet) => {
                    let data = packet.bytes();
                    if data.len() > 20 && data[9] == 17 {
                        // UDP detected
                    }
                }
                Err(e) => {
                    if stop_flag.load(std::sync::atomic::Ordering::SeqCst) {
                        break;
                    }
                    error!("Wintun receive error: {}", e);
                    std::thread::sleep(std::time::Duration::from_millis(100));
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
