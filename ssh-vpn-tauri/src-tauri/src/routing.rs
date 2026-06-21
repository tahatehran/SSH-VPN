use std::process::Command;
use tracing::info;
use crate::error::{Result, SshVpnError};

pub struct RoutingManager {
    original_gateway: Option<String>,
    ssh_server_ip: Option<String>,
    interface_name: Option<String>,
}

impl RoutingManager {
    pub fn new() -> Self {
        Self {
            original_gateway: None,
            ssh_server_ip: None,
            interface_name: None,
        }
    }

    pub fn setup_routing(&mut self, ssh_host: &str, dns_servers: &[String], interface_name: &str) -> Result<()> {
        info!("Setting up routing for {}", ssh_host);

        // 1. Resolve SSH host to IP if it's a hostname
        let ssh_ip = self.resolve_host(ssh_host)?;
        self.ssh_server_ip = Some(ssh_ip.clone());

        // 2. Find default gateway
        let gateway = self.find_default_gateway()?;
        self.original_gateway = Some(gateway.clone());

        info!("Original gateway: {}, SSH IP: {}", gateway, ssh_ip);

        // 3. Add route to SSH server via original gateway
        self.run_route_cmd(&["add", &ssh_ip, "mask", "255.255.255.255", &gateway, "metric", "1"])?;

        // 4. Add global route via TUN adapter
        self.run_route_cmd(&["add", "0.0.0.0", "mask", "0.0.0.0", "10.10.10.1", "metric", "5"])?;

        // 5. Setup DNS

        // 5. Setup DNS
        if !dns_servers.is_empty() {
            // Set the first DNS server (clears existing)
            let _ = Command::new("netsh")
                .args(["interface", "ip", "set", "dns", interface_name, "static", &dns_servers[0]])
                .output();

            // Add subsequent DNS servers
            for (i, dns) in dns_servers.iter().enumerate().skip(1) {
                let index = (i + 1).to_string();
                let _ = Command::new("netsh")
                    .args(["interface", "ip", "add", "dns", interface_name, dns, &format!("index={}", index)])
                    .output();
            }
        }


        info!("Routing established");
        Ok(())
    }

    pub fn restore_routing(&mut self) -> Result<()> {
        info!("Restoring original routing");

                if let Some(ref name) = self.interface_name {
            let _ = self.run_route_cmd(&["delete", "0.0.0.0", "mask", "0.0.0.0", "10.10.10.1"]);
        }

        if let Some(ssh_ip) = &self.ssh_server_ip {
            let _ = self.run_route_cmd(&["delete", ssh_ip]);
        }

        self.ssh_server_ip = None;
        self.original_gateway = None;

        info!("Routing restored");
        Ok(())
    }

    fn resolve_host(&self, host: &str) -> Result<String> {
        use std::net::ToSocketAddrs;
        let addr = format!("{}:22", host);
        if let Ok(mut addrs) = addr.to_socket_addrs() {
            if let Some(addr) = addrs.next() {
                return Ok(addr.ip().to_string());
            }
        }
        Err(SshVpnError::NetworkError(format!("Failed to resolve host: {}", host)))
    }

    fn find_default_gateway(&self) -> Result<String> {
        let output = Command::new("powershell")
            .args(["-Command", "Get-NetRoute -DestinationPrefix '0.0.0.0/0' | Sort-Object RouteMetric | Select-Object -First 1 -ExpandProperty NextHop"])
            .output()
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to run powershell: {}", e)))?;

        if output.status.success() {
            let gateway = String::from_utf8_lossy(&output.stdout).trim().to_string();
            if !gateway.is_empty() {
                return Ok(gateway);
            }
        }

        Err(SshVpnError::NetworkError("Could not find default gateway".to_string()))
    }

    fn run_route_cmd(&self, args: &[&str]) -> Result<()> {
        let output = Command::new("route")
            .args(args)
            .output()
            .map_err(|e| SshVpnError::NetworkError(format!("Failed to run route command: {}", e)))?;

        if !output.status.success() {
            let err = String::from_utf8_lossy(&output.stderr);
            tracing::warn!("Route command warning ({}): {}", args.join(" "), err);
        }
        Ok(())
    }
}
