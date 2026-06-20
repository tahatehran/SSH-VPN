# Contributing to SSH VPN

Thank you for your interest in contributing to SSH VPN! This document provides guidelines and instructions for contributing.

---

## 🚀 Getting Started

### Prerequisites

- **Node.js** 18+ 
- **Rust** 1.70+
- **Git**
- Windows 10/11 (for building)

### Setup

```bash
# Clone the repository
git clone https://github.com/tahatehran/CSharp-SSH-VPN.git
cd CSharp-SSH-VPN

# Install frontend dependencies
cd ssh-vpn-tauri
npm install

# Run in development mode
npm run tauri dev
```

---

## 🛠️ Development

### Project Structure

```
CSharp-SSH-VPN/
├── ssh-vpn-tauri/          # Main application
│   ├── src/                 # React frontend
│   │   ├── components/      # UI components
│   │   ├── stores/          # Zustand stores
│   │   ├── locales/         # i18n translations
│   │   └── App.tsx          # Main app
│   └── src-tauri/           # Rust backend
│       ├── src/
│       │   ├── main.rs      # Entry point
│       │   ├── lib.rs       # Library exports
│       │   ├── commands.rs  # Tauri commands
│       │   ├── ssh_client.rs # SSH logic
│       │   └── storage.rs   # JSON storage
│       └── Cargo.toml
├── .github/                 # CI/CD workflows
├── AGENT.md                 # Developer guidelines
└── README.md                # Documentation
```

### Code Style

- **Rust**: Follow `rustfmt` and `clippy` recommendations
- **React/TypeScript**: Use ESLint and Prettier
- **Commits**: Use conventional commits format

### Branches

- `master` - Stable release branch
- `feature/*` - Feature development branches
- `fix/*` - Bug fix branches

---

## 🔄 Pull Request Process

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature`
3. **Make** your changes
4. **Test** thoroughly
5. **Commit** using conventional commits:
   - `feat:` New features
   - `fix:` Bug fixes
   - `docs:` Documentation
   - `chore:` Maintenance
6. **Push** to your fork
7. **Open** a Pull Request

### PR Requirements

- [ ] Code follows project style guidelines
- [ ] Tests pass (if applicable)
- [ ] Documentation updated
- [ ] No breaking changes (or documented)

---

## 🐛 Reporting Issues

- Use [GitHub Issues](https://github.com/tahatehran/CSharp-SSH-VPN/issues)
- Include OS, app version, and steps to reproduce
- For security issues, see Security Policy

---

## 📋 TODO

- [ ] Add unit tests
- [ ] Implement credential encryption
- [ ] Add Linux/macOS support
- [ ] Create mobile companion app

---

## 📞 Contact

- **Email**: info@movtigroup.ir
- **GitHub**: [@tahatehran](https://github.com/tahatehran)
- **Website**: [movtigroup.ir](https://movtigroup.ir)

---

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

# مشارکت در SSH VPN

از علاقه‌مندی شما به مشارکت در SSH VPN سپاسگزاریم!

### پیش‌نیازها

- Node.js 18+
- Rust 1.70+
- Git

### شروع

```bash
git clone https://github.com/tahatehran/CSharp-SSH-VPN.git
cd CSharp-SSH-VPN/ssh-vpn-tauri
npm install
npm run tauri dev
```

### ساختار پروژه

```
ssh-vpn-tauri/
├── src/                 # فرانت‌اند React
│   ├── components/      # کامپوننت‌های UI
│   ├── stores/          # استور‌های Zustand
│   └── locales/         # ترجمه‌ها
└── src-tauri/           # بک‌اند Rust
```

### قوانین

- از فرمت commit استفاده کنید
- کد را تست کنید
- مستندات را آپدیت کنید

### تماس با ما

- **ایمیل**: info@movtigroup.ir
- **گیت‌هاب**: [@tahatehran](https://github.com/tahatehran)
