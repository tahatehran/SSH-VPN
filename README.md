# 🛡️ SSH VPN

[![Build Status](https://github.com/tahatehran/CSharp-SSH-VPN/actions/workflows/main.yml/badge.svg)](https://github.com/tahatehran/CSharp-SSH-VPN/actions/workflows/main.yml)
[![Release](https://img.shields.io/github/v/release/tahatehran/CSharp-SSH-VPN?include_prereleases)](https://github.com/tahatehran/CSharp-SSH-VPN/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Tauri](https://img.shields.io/badge/Tauri-2.0-ffc107.svg)](https://tauri.app/)
[![Rust](https://img.shields.io/badge/Rust-stable-orange.svg)](https://www.rust-lang.org/)

A professional-grade Windows application that leverages the SSH protocol to provide a secure VPN/Tunneling experience. Built with **Tauri (Rust + React)** for a modern, fast, and secure experience.

---

## 🚀 Key Features

### Modern Version (Tauri/Rust) ✨
- **🎨 Modern UI** - React + Tailwind CSS with Framer Motion animations
- **⚡ High Performance** - Native Rust backend with Tokio async runtime
- **📦 Small Size** - ~10MB vs ~50MB for .NET Framework
- **🌓 Adaptive Theme** - Dark/Light/System mode support
- **🌍 RTL Support** - Full support for English and Farsi (Persian)
- **📊 Real-time Charts** - Live bandwidth monitoring with Recharts
- **🌐 GeoIP Location** - Server location detection
- **🔄 Auto-Reconnect** - Exponential backoff reconnection
- **💾 JSON Storage** - Modern configuration management

---

## 📸 Screenshots

| Modern (Tauri) | Classic (C#) |
| :---: | :---: |
| ![Tauri UI](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/12b72ac7-92f6-47be-bdd5-abcabfc9458c) | ![C# UI](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/21a80309-2760-46df-b94c-bab82cce845d) |

---

## ⚙️ CI/CD Pipeline

GitHub Actions workflows:
- **main.yml**: Build and release Tauri app on push to master
- **release.yml**: Auto-tag releases and generate changelogs

---

## 🛠 Project Roadmap

- [x] **Phase 1**: Core SSH tunneling engine and basic Windows Forms UI.
- [x] **Phase 2**: UI/UX overhaul with custom themes and animations.
- [x] **Phase 3**: Multi-language support (EN/FA) and CI/CD pipeline setup.
- [x] **Phase 4**: Server list management, real-time ping detection, and bandwidth monitoring.
- [x] **Phase 5**: Connection reliability hardening with timeout-based SSH connects.
- [x] **Phase 6**: Tauri rebuild - Modern Rust + React frontend ✅

---

## 🛡️ Security & Development

- **Configuration**: SSH passwords stored in JSON files
- **Guidelines**: See [AGENT.md](./AGENT.md) for coding standards

---

## 📥 Download & Installation

1. Go to [Releases](https://github.com/tahatehran/CSharp-SSH-VPN/releases)
2. Download `SSH VPN.exe` or `.msi` installer
3. Run and enjoy!

---

## 🔧 Development

```bash
cd ssh-vpn-tauri
npm install
npm run tauri dev    # Development
npm run tauri build  # Production build
```

---

## 📁 Project Structure

```
CSharp-SSH-VPN/
├── ssh-vpn-tauri/     # Modern Tauri (Rust + React) app
├── .github/           # CI/CD workflows
├── AGENT.md           # Developer guidelines
├── CONTRIBUTING.md    # Contribution guidelines
├── README.md          # This file
└── Readme.fa.md       # Persian documentation
```

---

### 🤝 Support & Sponsorship
Developed and maintained by [tahatehran](https://github.com/tahatehran).

For more information on internet freedom tools, check out the [Hope Project](https://github.com/iranxray/hope).
