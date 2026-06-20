# 🛡️ SSH VPN (C#)

[![Build Status](https://github.com/tahatehran/CSharp-SSH-VPN/actions/workflows/main.yml/badge.svg)](https://github.com/tahatehran/CSharp-SSH-VPN/actions/workflows/main.yml)
[![Release](https://img.shields.io/github/v/release/tahatehran/CSharp-SSH-VPN?include_prereleases)](https://github.com/tahatehran/CSharp-SSH-VPN/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A professional-grade Windows application built with C# that leverages the SSH protocol to provide a secure VPN/Tunneling experience. By establishing an encrypted SSH tunnel and automatically configuring system-wide proxy settings, it ensures private and unrestricted internet access.

---

## 🚀 Key Features

- **🎨 Modern UI/UX**: Clean, flat design with a focus on usability and aesthetics.
- **🌓 Adaptive Theme**: Full support for both **Dark** and **Light** modes to match your system preference.
- **🌍 Bilingual Support**: Seamlessly switch between **English** and **Farsi (Persian)**.
- **⚡ High Performance**: Optimized SSH tunneling on port 9000 for low-latency connections.
- **🛠️ Zero Configuration**: Automatically handles Windows SOCKS5 proxy settings upon connection.
- **🤖 Integrated CI/CD**: Powered by GitHub Actions for automated builds, testing, and release management.
- **📋 Server List Management**: Save and manage multiple SSH server configurations.
- **📊 Real-time Ping Detection**: Monitor server latency with live ping updates.
- **📈 Bandwidth Monitoring**: Track download/upload traffic in real-time.

## 📸 Screenshots

| Connection Interface | Settings & Configuration |
| :---: | :---: |
| ![Main Interface](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/12b72ac7-92f6-47be-bdd5-abcabfc9458c) | ![Settings](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/21a80309-2760-46df-b94c-bab82cce845d) |

## ⚙️ Advanced Automation (Workflows)

This repository features a unified CI/CD pipeline (`.github/workflows/main.yml`) that handles:

1.  **Continuous Integration**: Automated MSBuild process for every push/PR targeting .NET Framework 4.8.
2.  **Release Management**: Automatic generation of versioned Release `ssh-vpn.exe` and `ssh-vpn.Setup.msi` artifacts.

## 🛠 Project Roadmap

- [x] **Phase 1**: Core SSH tunneling engine and basic Windows Forms UI.
- [x] **Phase 2**: UI/UX overhaul with custom themes and animations.
- [x] **Phase 3**: Multi-language support (EN/FA) and CI/CD pipeline setup.
- [x] **Phase 4**: Server list management, real-time ping detection, and bandwidth monitoring.
- [x] **Phase 5**: Connection reliability hardening with timeout-based SSH connects, safe disconnect cleanup, UI-thread marshaling, bandwidth monitor lifecycle management, and stronger registry/proxy error handling.
- [ ] **Phase 6**: Tauri rebuild - Modern Rust + React frontend with enhanced features

## 🛡️ Security & Development

- **Configuration**: Sensitive fields like SSH passwords are currently stored in the Windows Registry.
- **AES Encryption**: A security update to encrypt stored credentials is in development.
- **Guidelines**: Contributors should refer to [AGENT.md](./AGENT.md) for coding standards.

## 📥 Download & Installation

1.  Go to the [Releases](https://github.com/tahatehran/CSharp-SSH-VPN/releases) page.
2.  Download the latest `ssh-vpn.exe` Release executable, or install `ssh-vpn.Setup.msi`.
3.  Run `ssh-vpn.exe` or complete the MSI installation.
4.  Configure your SSH server details in the settings.
5.  Use the server list to manage multiple connections.

---

## 🚀 New: Tauri Rebuild (ssh-vpn-tauri)

A modern rebuild of the SSH VPN application using **Rust + React** with Tauri is available in the `ssh-vpn-tauri/` directory.

### Key Improvements:
- **Modern UI** - React + Tailwind CSS with smooth animations (Framer Motion)
- **Better Performance** - Native Rust backend with Tokio async runtime
- **Smaller Size** - ~10MB vs ~50MB for .NET Framework
- **Cross-Platform Ready** - Designed for Windows, with potential for macOS/Linux
- **Enhanced Features** - Multi-server failover, real-time charts, GeoIP location

See [ssh-vpn-tauri/SPEC.md](./ssh-vpn-tauri/SPEC.md) for technical details.

---

### 🤝 Support & Sponsorship
Developed and maintained by [tahatehran](https://github.com/tahatehran).
Supported by **Movti Group**.

For more information on internet freedom tools, check out the [Hope Project](https://github.com/iranxray/hope).
