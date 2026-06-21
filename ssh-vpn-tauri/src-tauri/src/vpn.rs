use crate::routing::RoutingManager;
use crate::debug::{DebugManager, LogLevel};
use std::sync::Arc;
use tracing::{info, warn, error};
use crate::error::{Result, SshVpnError};
use std::process::Command;

pub struct VpnManager {
    wintun: Option<Arc<wintun::Adapter>>,
    should_stop: Arc<std::sync::atomic::AtomicBool>,
    routing: RoutingManager,
    debug_manager: Option<Arc<DebugManager>>,
}

impl VpnManager {
    pub fn new() -> Self {
        Self {
            wintun: None,
            should_stop: Arc::new(std::sync::atomic::AtomicBool::new(false)),
            routing: RoutingManager::new(),
            debug_manager: None,
        }
    }

    pub fn set_debug_manager(&mut self, debug_manager: Arc<DebugManager>) {
        self.debug_manager = Some(debug_manager);
    }

    pub async fn start(&mut self, socks_port: u16, ssh_host: &str, dns_servers: &[String]) -> Result<()> {
        if self.wintun.is_some() {
            return Ok(());
        }

        if let Some(dm) = &self.debug_manager {
            dm.log(LogLevel::Info, "VPN", "Initializing Global VPN (Wintun)").await;
        }

        info!("Starting Global VPN mode (Wintun)");
        self.should_stop.store(false, std::sync::atomic::Ordering::SeqCst);

        let wintun_lib = unsafe {
            if let Ok(exe_path) = std::env::current_exe() {
                if let Some(parent) = exe_path.parent() {
                    let dll_path = parent.join("wintun.dll");
                    if dll_path.exists() {
                        info!("Loading wintun.dll from {:?}", dll_path);
                        wintun::load_from_path(dll_path)
                    } else {
                        wintun::load()
                    }
                } else {
                    wintun::load()
                }
            } else {
                wintun::load()
            }
        }.map_err(|e| SshVpnError::NetworkError(format!("Failed to load wintun.dll: {}", e)))?;

        let adapter = wintun::Adapter::create(&wintun_lib, "SSH VPN Tunnel", "SSH VPN Tunnel", None)
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to create Wintun adapter: {}", e)))?;

        let interface_name = adapter.get_name()
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to get adapter name: {}", e)))?;

        info!("Configuring interface {} with IP 10.10.10.1", interface_name);

        let output = Command::new("netsh")
            .args(["interface", "ip", "set", "address", &interface_name, "static", "10.10.10.1", "255.255.255.0", "none"])
            .output()
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to run netsh for IP: {}", e)))?;

        if !output.status.success() {
            let err = String::from_utf8_lossy(&output.stderr);
            warn!("Netsh IP set warning: {}", err);
            if let Some(dm) = &self.debug_manager {
                dm.log(LogLevel::Warning, "VPN", &format!("Netsh warning: {}", err)).await;
            }
        }

        let _ = Command::new("netsh")
            .args(["interface", "ipv4", "set", "subinterface", &interface_name, "mtu=1500", "store=active"])
            .output();

        self.routing.setup_routing(ssh_host, dns_servers, &interface_name)?;

        let _ = Command::new("ipconfig").args(["/flushdns"]).output();

        self.wintun = Some(Arc::clone(&adapter));

        let adapter_worker = Arc::clone(&adapter);
        let stop_flag = Arc::clone(&self.should_stop);
        let dm_worker = self.debug_manager.clone();

        tokio::spawn(async move {
            if let Err(e) = Self::run_tun_bridge(adapter_worker, socks_port, stop_flag, dm_worker).await {
                error!("TUN bridge failed: {}", e);
            }
        });

        if let Some(dm) = &self.debug_manager {
            dm.log(LogLevel::Info, "VPN", "Global VPN started successfully").await;
        }

        Ok(())
    }

    async fn run_tun_bridge(adapter: Arc<wintun::Adapter>, socks_port: u16, stop_flag: Arc<std::sync::atomic::AtomicBool>, dm: Option<Arc<DebugManager>>) -> Result<()> {
        let session = adapter.start_session(wintun::MAX_RING_CAPACITY)
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to start Wintun session: {}", e)))?;

        let session = Arc::new(session);
        info!("TUN bridge active. Target SOCKS port: {}", socks_port);

        if let Some(ref d) = dm {
            d.log(LogLevel::Info, "VPN", "Packet bridge loop entered").await;
        }

        while !stop_flag.load(std::sync::atomic::Ordering::SeqCst) {
            match session.receive_blocking() {
                Ok(packet) => {
                    let data = packet.bytes();
                    if data.len() > 9 {
                        let _protocol = data[9];
                    }
                }
                Err(e) => {
                    if stop_flag.load(std::sync::atomic::Ordering::SeqCst) {
                        break;
                    }
                    warn!("Wintun receive error: {}", e);
                    tokio::time::sleep(std::time::Duration::from_millis(10)).await;
                }
            }
        }

        info!("TUN bridge worker stopped");
        Ok(())
    }

    pub fn stop(&mut self) -> Result<()> {
        if let Some(dm) = &self.debug_manager {
            let dm = dm.clone();
            tokio::spawn(async move {
                dm.log(LogLevel::Info, "VPN", "Stopping Global VPN").await;
            });
        }

        let _ = self.routing.restore_routing();
        info!("Stopping Global VPN mode");
        self.should_stop.store(true, std::sync::atomic::Ordering::SeqCst);

        if let Some(adapter) = self.wintun.take() {
            drop(adapter);
        }

        Ok(())
    }
}
