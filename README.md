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
2.  **Release Management**: Automatic generation of versioned x64 ZIP artifacts for production.
3.  **Video Utility**: A built-in `yt-dlp` powered downloader accessible via the Actions tab.

> [!TIP]
> To use the **Video Downloader**: Navigate to **Actions** -> **SSH VPN Pipeline** -> **Run workflow** -> Enter your URL.

## 🛠 Project Roadmap

- [x] **Phase 1**: Core SSH tunneling engine and basic Windows Forms UI.
- [x] **Phase 2**: UI/UX overhaul with custom themes and animations.
- [x] **Phase 3**: Multi-language support (EN/FA) and CI/CD pipeline setup.
- [x] **Phase 4**: Server list management, real-time ping detection, and bandwidth monitoring.

## 🛡️ Security & Development

- **Configuration**: Sensitive fields like SSH passwords are currently stored in the Windows Registry.
- **AES Encryption**: A security update to encrypt stored credentials is in development.
- **Guidelines**: Contributors should refer to [AGENT.md](./AGENT.md) for coding standards.

## 📥 Download & Installation

1.  Go to the [Releases](https://github.com/tahatehran/CSharp-SSH-VPN/releases) page.
2.  Download the latest `ssh-vpn-x64.zip`.
3.  Extract and run `ssh-vpn.exe`.
4.  Configure your SSH server details in the settings.
5.  Use the server list to manage multiple connections.

---

### 🤝 Support & Sponsorship
Developed and maintained by [tahatehran](https://github.com/tahatehran).
Supported by **Movti Group**.

For more information on internet freedom tools, check out the [Hope Project](https://github.com/iranxray/hope).
