#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use ssh_vpn_lib::{commands::AppState, storage::Storage, ssh_client::SshClient, bandwidth::BandwidthMonitor};
use std::sync::Arc;
use tokio::sync::Mutex;
use tracing::{error, info, Level};
use tracing_subscriber::FmtSubscriber;

fn main() {
    // Initialize logging
    let subscriber = FmtSubscriber::builder()
        .with_max_level(Level::INFO)
        .with_target(false)
        .with_thread_ids(false)
        .with_file(true)
        .with_line_number(true)
        .finish();

    if let Err(e) = tracing::subscriber::set_global_default(subscriber) {
        eprintln!("Failed to set up logging: {}", e);
    }

    info!("Starting SSH VPN Tauri application");

    // Initialize storage
    let storage = match Storage::new() {
        Ok(s) => s,
        Err(e) => {
            error!("Failed to initialize storage: {}", e);
            eprintln!("Failed to initialize storage: {}", e);
            std::process::exit(1);
        }
    };

    // Initialize SSH client and bandwidth monitor
    let ssh_client = Arc::new(Mutex::new(SshClient::new()));
    let bandwidth = Arc::new(BandwidthMonitor::new());

    let app_state = AppState { 
        storage, 
        ssh_client,
        bandwidth,
    };

    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_updater::Builder::new().build())
        .manage(app_state)
        .invoke_handler(tauri::generate_handler![
            ssh_vpn_lib::connect,
            ssh_vpn_lib::disconnect,
            ssh_vpn_lib::get_status,
            ssh_vpn_lib::get_bandwidth,
            ssh_vpn_lib::add_server,
            ssh_vpn_lib::get_servers,
            ssh_vpn_lib::update_server,
            ssh_vpn_lib::delete_server,
            ssh_vpn_lib::set_active_server,
            ssh_vpn_lib::get_settings,
            ssh_vpn_lib::save_settings,
            ssh_vpn_lib::test_latency,
            ssh_vpn_lib::get_server_location,
            ssh_vpn_lib::get_app_version,
            ssh_vpn_lib::set_system_proxy,
            ssh_vpn_lib::unset_system_proxy,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}