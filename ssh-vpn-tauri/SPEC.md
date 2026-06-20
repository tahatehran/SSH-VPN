# SSH VPN Tauri - Technical Specification

## Overview

This document describes the architecture and implementation details of the SSH VPN Tauri application - a rebuild of the existing C# Windows Forms application using Rust (Tauri) and React.

## Technology Stack

| Layer | Technology |
|-------|------------|
| Backend | Rust + Tauri 2.x |
| Frontend | React 18 + TypeScript |
| Styling | Tailwind CSS + CSS Variables |
| Animations | Framer Motion |
| State Management | Zustand |
| Charts | Recharts |
| i18n | i18next + react-i18next |
| SSH | ssh2 crate |
| Async Runtime | Tokio |
| Logging | tracing + tracing-subscriber |

## Architecture

### Backend (Rust/Tauri)

#### Core Modules

1. **ssh_client.rs** - SSH connection management
   - `SshClient` - Main client for managing SSH connections
   - `ServerInfo` - Server configuration structure
   - `ConnectionStatus` - Connection state tracking
   - `ConnectionState` - State enum (Disconnected, Connecting, Connected, Reconnecting, Error)
   - `ReconnectConfig` - Auto-reconnect configuration with exponential backoff

2. **storage.rs** - JSON file persistence
   - `Storage` - Manages JSON file storage in `~/.config/ssh-vpn-tauri/`
   - `AppSettings` - Application configuration
   - Files: `servers.json`, `settings.json`, `connections.log`

3. **bandwidth.rs** - Network monitoring
   - `BandwidthMonitor` - Tracks bytes sent/received
   - `BandwidthStats` - Statistics structure

4. **commands.rs** - Tauri IPC commands
   - `connect`, `disconnect`, `get_status`
   - `add_server`, `get_servers`, `update_server`, `delete_server`
   - `get_settings`, `save_settings`
   - `test_latency`, `get_server_location`

5. **error.rs** - Error handling
   - `SshVpnError` - Enum of error types
   - `Result<T>` - Result type alias

### Frontend (React/TypeScript)

#### Components

```
src/components/
├── Layout/
│   ├── Header.tsx      - App header with theme/language toggles
│   ├── Sidebar.tsx     - Navigation sidebar
│   └── StatusBar.tsx   - Connection status footer
├── Dashboard/
│   ├── Dashboard.tsx   - Main dashboard view
│   ├── ConnectionCard.tsx - Connect/disconnect UI
│   ├── BandwidthChart.tsx - Real-time bandwidth chart
│   └── StatsPanel.tsx  - Statistics display
├── ServerList/
│   ├── ServerList.tsx  - Server management view
│   ├── ServerCard.tsx  - Individual server card
│   └── ServerEditModal.tsx - Add/edit server form
├── Settings/
│   └── Settings.tsx    - Application settings
└── UI/
    ├── Button.tsx      - Reusable button component
    ├── Card.tsx        - Card container component
    ├── Toggle.tsx      - Toggle switch component
    └── Modal.tsx       - Modal dialog component
```

#### State Management (Zustand)

```typescript
interface AppState {
  // Connection
  connectionStatus: ConnectionStatus;
  isConnecting: boolean;
  
  // Servers
  servers: ServerInfo[];
  activeServerId: string | null;
  
  // Settings
  settings: AppSettings;
  
  // Bandwidth
  bandwidth: BandwidthStats[];
  
  // UI State
  theme: 'light' | 'dark' | 'system';
  language: 'en' | 'fa';
  activeView: 'dashboard' | 'servers' | 'settings';
  
  // Actions
  connect, disconnect, fetchStatus,
  fetchServers, addServer, updateServer, deleteServer,
  setActiveServer, fetchSettings, saveSettings,
  testLatency, setTheme, setLanguage, setActiveView
}
```

### Storage Structure

```
~/.config/ssh-vpn-tauri/
├── servers.json       # Server configurations
├── settings.json      # Application settings
├── connections.log    # Connection history
└── cache/
    └── geoip.db       # (Future) GeoIP database
```

### Tauri Commands

| Command | Parameters | Returns |
|---------|------------|---------|
| `connect` | `ServerConfig` | `ConnectionStatus` |
| `disconnect` | - | `()` |
| `get_status` | - | `ConnectionStatus` |
| `add_server` | `ServerInfo` | `String` (id) |
| `get_servers` | - | `Vec<ServerInfo>` |
| `update_server` | `ServerInfo` | `()` |
| `delete_server` | `String` (id) | `()` |
| `set_active_server` | `String` (id) | `()` |
| `get_settings` | - | `AppSettings` |
| `save_settings` | `AppSettings` | `()` |
| `test_latency` | `String` (host), `u16` (port) | `u32` (ms) |
| `get_server_location` | `String` (host) | `serde_json::Value` |

## Features

### Implemented

- [x] SSH connection with password/key authentication
- [x] SOCKS5 proxy on port 9000
- [x] Auto-reconnect with exponential backoff
- [x] Connection state management
- [x] Server list management (CRUD)
- [x] JSON file storage
- [x] Bandwidth monitoring
- [x] Theme support (Light/Dark/System)
- [x] RTL language support (English/Farsi)
- [x] Real-time bandwidth charts
- [x] Server latency testing
- [x] Server location display (via ip-api.com)

### Planned

- [ ] Kill Switch (Windows Firewall API)
- [ ] DNS Leak Protection
- [ ] Multi-Server Failover
- [ ] GeoIP database for offline location
- [ ] Unit tests
- [ ] Integration tests

## Build Requirements

- Rust 1.70+
- Node.js 18+
- npm 9+
- Windows 10/11 (for native features)

## Development

```bash
# Install dependencies
cd ssh-vpn-tauri
npm install

# Run in development mode
npm run tauri dev

# Build for production
npm run tauri build
```

## License

MIT License - See LICENSE file in repository root.