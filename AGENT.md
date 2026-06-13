# AGENT.md - Project Guidelines

## Project Overview
This is a C# Windows Forms application that provides SSH VPN functionality by establishing a SOCKS5 proxy and configuring Windows system proxy settings.

## Coding Standards
- **UI/UX**: Use flat design principles. Maintain support for both Light and Dark modes.
- **Localization**: All UI strings should be manageable through resource files to support English and Farsi.
- **Registry**: Use `Registry.CurrentUser.OpenSubKey("ssh_vpn")` for storing settings. Do not store sensitive data in plain text if possible (though currently, it uses plain registry keys).
- **Asynchronous Operations**: Use `ThreadPool` or `Task` for network operations to keep the UI responsive.

## UI Components
- **Main Form**: `Form1.cs`
- **Settings**: `SettingsForm.cs`
- **Modernization**: Focus on smooth transitions, consistent padding, and modern color palettes.

## CI/CD
- The project uses GitHub Actions for building and packaging. See `.github/workflows/main.yml`.

## Contribution
Always update the Roadmap in `README.md` when completing major features.
