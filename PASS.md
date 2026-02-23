# PASS.md - Configuration & Credentials

## Settings Storage
This application stores user settings and SSH credentials in the Windows Registry for persistence.

**Registry Path:**
`HKEY_CURRENT_USER\Software\ssh_vpn`

## Keys Stored:
- `ip`: The SSH server IP address.
- `port`: The SSH server port (default: 22).
- `username`: The SSH username.
- `password`: The SSH password.

## Security Note
Currently, credentials are stored in the registry without encryption. Users should be aware that anyone with access to the local machine's registry can retrieve these details.

**Planned Improvement**: Implement AES encryption for the `password` field in a future update.

## Proxy Configuration
The app sets the Windows Proxy to:
`socks5://127.0.0.1:9000`
