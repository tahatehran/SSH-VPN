# AGENT.md - Project Guidelines

## Project Overview
This is a dual-application project providing SSH VPN functionality:

1. **Legacy Application** (C# Windows Forms): Original `.NET Framework 4.8` application in `ssh-vpn/` directory
2. **Modern Application** (Tauri/Rust + React): Rebuild in `ssh-vpn-tauri/` directory

## Project Structure
```
CSharp-SSH-VPN/
├── ssh-vpn/           # Legacy C# Windows Forms app
├── ssh-vpn-tauri/     # Modern Tauri (Rust + React) app
├── AGENT.md           # This file
└── README.md          # Project documentation
```

## Coding Standards

### Modern App (ssh-vpn-tauri)
- **Backend**: Rust with Tauri 2.x framework
- **Frontend**: React 18 + TypeScript + Vite + Tailwind CSS
- **State Management**: Zustand
- **i18n**: i18next with RTL support (English/Farsi)
- **Animations**: Framer Motion
- **Charts**: Recharts for bandwidth visualization
- **Storage**: JSON files in `~/.config/ssh-vpn-tauri/` (replaces Registry)

### Legacy App (ssh-vpn)
- **UI/UX**: Flat design with Light/Dark mode support
- **Localization**: Resource files for English and Farsi
- **Registry**: `Registry.CurrentUser.OpenSubKey("ssh_vpn")` for settings
- **Async**: `ThreadPool` or `Task` for network operations

## UI Components

### Modern App Components
- **Dashboard**: ConnectionCard, BandwidthChart, StatsPanel
- **ServerList**: ServerCard, ServerEditModal
- **Settings**: Full settings with toggles
- **Layout**: Header, Sidebar, StatusBar

### Legacy App Components
- **Main Form**: `Form1.cs`
- **Settings**: `SettingsForm.cs`

## CI/CD
GitHub Actions workflow (`.github/workflows/main.yml`) builds both:
- C# application (on changes to `ssh-vpn/`)
- Tauri application (on changes to `ssh-vpn-tauri/`)

## Key Technologies
| Component | Technology |
|-----------|------------|
| Backend | Rust + Tauri 2.x |
| Frontend | React 18 + TypeScript |
| Styling | Tailwind CSS |
| SSH | russh/ssh2 crate |
| Storage | JSON files |
| Charts | Recharts |

## Contribution
- Always update the Roadmap in `README.md` when completing major features.
- When adding features to the modern app, update `ssh-vpn-tauri/SPEC.md`
