# AGENT.md - Project Guidelines

## Project Overview
This is a modern SSH VPN application built with **Tauri (Rust + React)**. It provides secure VPN/Tunneling functionality through SSH protocol with a SOCKS5 proxy.

## Project Structure
```
CSharp-SSH-VPN/
├── ssh-vpn-tauri/     # Main application (Rust + React)
├── .github/           # CI/CD workflows
├── AGENT.md           # This file
└── README.md          # Project documentation
```

## Coding Standards

### Backend (Rust/Tauri)
- **Framework**: Tauri 2.x
- **Async Runtime**: Tokio
- **SSH**: ssh2 crate
- **Error Handling**: Custom error types with `thiserror`
- **Logging**: tracing + tracing-subscriber

### Frontend (React/TypeScript)
- **Framework**: React 18 + TypeScript + Vite
- **Styling**: Tailwind CSS with CSS variables for theming
- **State Management**: Zustand
- **i18n**: i18next with RTL support (English/Farsi)
- **Animations**: Framer Motion
- **Charts**: Recharts for bandwidth visualization
- **Storage**: JSON files in `~/.config/ssh-vpn-tauri/`

## UI Components
- **Dashboard**: ConnectionCard, BandwidthChart, StatsPanel
- **ServerList**: ServerCard, ServerEditModal
- **Settings**: Full settings with toggles
- **Layout**: Header, Sidebar, StatusBar

## CI/CD
GitHub Actions workflow (`.github/workflows/main.yml`) builds the Tauri application.

## Key Technologies
| Component | Technology |
|-----------|------------|
| Backend | Rust + Tauri 2.x |
| Frontend | React 18 + TypeScript |
| Styling | Tailwind CSS |
| SSH | ssh2 crate |
| Storage | JSON files |
| Charts | Recharts |

## Contribution
- Always update the Roadmap in `README.md` when completing major features.
- Update `ssh-vpn-tauri/SPEC.md` when adding significant features.
