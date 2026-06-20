# 🛡️ SSH VPN - Tauri Rebuild Plan (حرفه‌ای)

## 1. OBJECTIVE

بازسازی کامل اپلیکیشن SSH VPN از **C# Windows Forms** به **Tauri (Rust + React)** با:

- ✅ **Auto-Reconnect** - اتصال خودکار پس از قطع اینترنت
- ✅ **Multi-Server + Failover** - پشتیبانی از چند سرور با جابجایی خودکار
- ✅ **Kill Switch** - قطع اینترنت هنگام قطع VPN
- ✅ **DNS Leak Protection** - حفاظت از نشت DNS
- ✅ **Real-time Charts** - نمودار دانلود/آپلود با Recharts
- ✅ **Server Location** - نمایش کشور/شهر سرور
- ✅ **UI مدرن** - React + CSS با انیمیشن‌های روان
- ✅ **RTL Support** - زبان فارسی با پشتیبانی RTL
- ✅ **Theme Switch** - تم روشن/تاریک

---

## 2. CONTEXT SUMMARY

### پروژه فعلی (C#):
| فایل | عملکرد |
|------|--------|
| `Form1.cs` | اتصال SSH، مدیریت SOCKS5 proxy |
| `ServerManager.cs` | CRUD سرورها (ذخیره در Registry) |
| `BandwidthMonitor.cs` | مانیتورینگ پهنای باند |
| `ThemeManager.cs` | مدیریت تم Light/Dark |
| `LanguageManager.cs` | ترجمه EN/FA |

### فناوری‌های جدید:
| لایه | فناوری |
|------|--------|
| Backend | Rust + `ssh2` crate + `tokio` async runtime |
| Frontend | React 18 + TypeScript + Vite |
| Styling | Tailwind CSS + Custom CSS + Framer Motion |
| Charts | Recharts |
| State | Zustand |
| IPC | Tauri Commands |
| i18n | i18next + react-i18next |

---

## 3. APPROACH OVERVIEW

### استراتژی کلی:

1. **راه‌اندازی پروژه Tauri** با React + TypeScript
2. **پیاده‌سازی Core SSH** در Rust با:
   - Auto-reconnect با exponential backoff
   - Connection keep-alive
   - Multi-server failover
3. **پیاده‌سازی Kill Switch** با Windows Firewall API
4. **پیاده‌سازی DNS Leak Protection** با تنظیمات DNS سفارشی
5. **ساخت UI مدرن** با React + Tailwind + Framer Motion
6. **نمودارهای Real-time** با Recharts
7. **نمایش لوکیشن سرور** با GeoIP

---

## 4. IMPLEMENTATION STEPS

### مرحله ۱: راه‌اندازی پروژه Tauri ⚙️

**Goal:** ایجاد ساختار پروژه و نصب وابستگی‌ها

```bash
# بررسی نصب ابزارها
rustc --version    # >= 1.70
cargo --version
node --version     # >= 18
npm --version

# ایجاد پروژه Tauri
npm create tauri-app@latest ssh-vpn-tauri -- --template react-ts

# وابستگی‌های Rust (Cargo.toml)
cargo add ssh2 tokio --features tokio/full
cargo add serde --features derive
cargo add serde_json
cargo add chrono --features serde
cargo add uuid --features v4,serde
cargo add winreg     # Windows Registry
cargo add dirs       # مسیرهای سیستم
cargo add tokio::sync::broadcast  # Event channel
```

**Dependencies Frontend:**
```bash
npm install zustand framer-motion i18next react-i18next recharts
npm install -D tailwindcss postcss autoprefixer @types/node
```

---

### مرحله ۲: Core SSH Logic در Rust 🔧

**Goal:** پیاده‌سازی اتصال SSH حرفه‌ای

**Files:** `src-tauri/src/ssh_client.rs`, `src-tauri/src/lib.rs`

**قابلیت‌های کلیدی:**

```rust
// 1. Auto-Reconnect با Exponential Backoff
struct ReconnectConfig {
    initial_delay_ms: u64,    // 1000ms
    max_delay_ms: u64,        // 30000ms
    max_retries: u32,         // 10
    multiplier: f64,          // 2.0
}

// 2. Connection Keep-Alive
// ارسال پیام هر 30 ثانیه برای جلوگیری از قطع connection

// 3. Connection Events
enum ConnectionState {
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Error(String),
}
```

**Tauri Commands:**
```rust
#[tauri::command]
async fn connect(config: ServerConfig) -> Result<ConnectionStatus, String>

#[tauri::command]
async fn disconnect() -> Result<(), String>

#[tauri::command]
fn get_status() -> ConnectionStatus

#[tauri::command]
async fn add_server(server: ServerInfo) -> Result<String, String>  // returns ID

#[tauri::command]
fn get_servers() -> Vec<ServerInfo>

#[tauri::command]
fn delete_server(id: String) -> Result<(), String>

#[tauri::command]
fn set_active_server(id: String) -> Result<(), String>

#[tauri::command]
fn test_latency(host: String, port: u16) -> Result<u32, String>
```

---

### مرحله ۳: Kill Switch Implementation 🛡️

**Goal:** قطع اینترنت هنگام قطع VPN

**Implementation Strategy:**
```rust
// استفاده از Windows Firewall API
// 1. Block all outbound except SSH and tunnel
// 2. وقتی VPN قطع شد، همه ترافیک block میشه

// Windows Firewall Rules:
// - Allow SSH (port 22) outbound
// - Allow SOCKS proxy (port 9000) outbound  
// - Block all other outbound (در حالت Kill Switch فعال)
```

**Rust Implementation:**
```rust
pub struct KillSwitch {
    enabled: bool,
    original_rules: Vec<FirewallRule>,
}

impl KillSwitch {
    pub fn enable(&mut self) -> Result<(), Error>
    pub fn disable(&mut self) -> Result<(), Error>
    pub fn is_active(&self) -> bool
}
```

---

### مرحله ۴: DNS Leak Protection 🔒

**Goal:** جلوگیری از نشت DNS

**Implementation:**
```rust
// 1. DNS سفارشی تنظیم کن (مثل 1.1.1.1 یا 8.8.8.8)
// 2. Only allow DNS through tunnel
// 3. Block local DNS queries when VPN is active

pub struct DnsProtection {
    original_dns: Vec<String>,
    custom_dns: Vec<String>,
}

impl DnsProtection {
    pub fn enable(&self) -> Result<(), Error>
    pub fn restore(&self) -> Result<(), Error>
}
```

---

### مرحله ۵: Multi-Server Failover 🔄

**Goal:** جابجایی خودکار بین سرورها

**Logic:**
```rust
struct FailoverConfig {
    enabled: bool,
    check_interval_sec: u64,    // 30
    ping_timeout_ms: u64,       // 5000
    max_ping_ms: u32,           // 200 - اگر پینگ بیشتر شد switch کن
}

async fn check_server_health(server: &ServerInfo) -> ServerHealth {
    // 1. Ping test
    // 2. TCP connection test
    // 3. Return health score
}

async fn failover_manager() {
    // Monitor all servers
    // Find best available server
    // Auto-switch if current fails or degrades
}
```

---

### مرحله ۶: Server Storage (JSON) 💾

**Goal:** ذخیره سرورها به جای Registry

**Storage Location:** `%APPDATA%/ssh-vpn-tauri/`

**Files:**
```
~/.config/ssh-vpn-tauri/
├── servers.json       # لیست سرورها
├── settings.json      # تنظیمات
├── connections.log    # لاگ اتصالات
└── cache/
    └── geoip.db       # دیتابیس GeoIP
```

**ServerInfo Structure:**
```rust
#[derive(Serialize, Deserialize, Clone)]
pub struct ServerInfo {
    pub id: String,           // UUID
    pub name: String,
    pub name_fa: Option<String>,  // نام فارسی
    pub host: String,
    pub port: u16,
    pub username: String,
    pub password: Option<String>,
    pub private_key_path: Option<String>,
    pub country: Option<String>,
    pub city: Option<String>,
    pub priority: u32,        // اولویت برای failover
    pub is_active: bool,
    pub created_at: String,
    pub last_used: Option<String>,
}
```

---

### مرحله ۷: UI Components با React 🎨

**Files:** `src/components/*`, `src/styles/*`

**Component Hierarchy:**
```
App
├── Layout
│   ├── Header (Logo, Theme Toggle, Language Toggle, Minimize/Close)
│   ├── Sidebar (Navigation)
│   └── Content
│       ├── Dashboard
│       │   ├── ConnectionCard (دکمه Connect، وضعیت، پینگ)
│       │   ├── ServerSelector (Dropdown سرورها)
│       │   ├── BandwidthChart (نمودار Real-time)
│       │   └── StatsPanel (آمار کلی)
│       ├── ServerList
│       │   ├── ServerCard (نمایش سرور + location)
│       │   └── AddEditServerModal
│       └── Settings
│           ├── GeneralSettings
│           ├── ConnectionSettings (Auto-reconnect, Kill Switch, DNS)
│           └── AppearanceSettings
└── StatusBar (Connection status, Uptime, Data usage)
```

**CSS Variables (Theme System):**
```css
:root {
  /* Light Theme */
  --bg-primary: #f8fafc;
  --bg-secondary: #ffffff;
  --bg-tertiary: #f1f5f9;
  --text-primary: #1e293b;
  --text-secondary: #64748b;
  --accent: #3b82f6;
  --accent-hover: #2563eb;
  --success: #22c55e;
  --warning: #f59e0b;
  --error: #ef4444;
  --border: #e2e8f0;
  --shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1);
  
  /* Spacing */
  --radius-sm: 6px;
  --radius-md: 12px;
  --radius-lg: 16px;
  
  /* Transitions */
  --transition-fast: 150ms ease;
  --transition-normal: 300ms ease;
}

[data-theme="dark"] {
  --bg-primary: #0f172a;
  --bg-secondary: #1e293b;
  --bg-tertiary: #334155;
  --text-primary: #f1f5f9;
  --text-secondary: #94a3b8;
  --accent: #60a5fa;
  --accent-hover: #3b82f6;
  --success: #4ade80;
  --warning: #fbbf24;
  --error: #f87171;
  --border: #334155;
}
```

**Animations (Framer Motion):**
```tsx
// Connect Button
<motion.button
  animate={{ scale: isConnecting ? [1, 1.05, 1] : 1 }}
  transition={{ repeat: Infinity, duration: 1.5 }}
>
  {isConnected ? "Disconnect" : "Connect"}
</motion.button>

// Status Indicator
<motion.div
  initial={{ opacity: 0 }}
  animate={{ opacity: 1 }}
  className={statusColor}
/>

// Page Transitions
<AnimatePresence mode="wait">
  <motion.div
    initial={{ opacity: 0, x: 20 }}
    animate={{ opacity: 1, x: 0 }}
    exit={{ opacity: 0, x: -20 }}
  />
</AnimatePresence>
```

---

### مرحله ۸: Bandwidth Charts با Recharts 📊

**Goal:** نمودار Real-time مصرف

```tsx
<ResponsiveContainer width="100%" height={200}>
  <AreaChart data={bandwidthData}>
    <defs>
      <linearGradient id="downloadGradient" x1="0" y1="0" x2="0" y2="1">
        <stop offset="5%" stopColor="#22c55e" stopOpacity={0.3}/>
        <stop offset="95%" stopColor="#22c55e" stopOpacity={0}/>
      </linearGradient>
      <linearGradient id="uploadGradient" x1="0" y1="0" x2="0" y2="1">
        <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3}/>
        <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
      </linearGradient>
    </defs>
    <XAxis dataKey="time" />
    <YAxis />
    <Area
      type="monotone"
      dataKey="download"
      stroke="#22c55e"
      fill="url(#downloadGradient)"
    />
    <Area
      type="monotone"
      dataKey="upload"
      stroke="#3b82f6"
      fill="url(#uploadGradient)"
    />
  </AreaChart>
</ResponsiveContainer>
```

---

### مرحله ۹: Server Location Display 🌍

**Goal:** نمایش کشور/شهر سرور

**Approach:**
```rust
// استفاده از MaxMind GeoLite2 Country/City database
// یا سرویس‌های رایگان مثل ip-api.com

pub async fn get_server_location(host: &str) -> Option<GeoLocation> {
    // Free API: http://ip-api.com/json/{host}
    // Returns: country, city, ISP, timezone
}
```

**UI:**
```tsx
<ServerCard>
  <FlagIcon country={server.country} />
  <span>{server.name}</span>
  <span>{server.country} / {server.city}</span>
  <PingIndicator value={server.latency} />
</ServerCard>
```

---

### مرحله ۱۰: i18n (English + Farsi) 🌍

**File:** `src/i18n/index.ts`

```typescript
// en.json
{
  "app": {
    "name": "SSH VPN",
    "connect": "Connect",
    "disconnect": "Disconnect",
    "connecting": "Connecting...",
    "connected": "Connected",
    "notConnected": "Not Connected"
  },
  "dashboard": {
    "servers": "Servers",
    "bandwidth": "Bandwidth",
    "download": "Download",
    "upload": "Upload",
    "ping": "Ping",
    "uptime": "Uptime"
  },
  "settings": {
    "title": "Settings",
    "autoReconnect": "Auto Reconnect",
    "killSwitch": "Kill Switch",
    "dnsProtection": "DNS Leak Protection",
    "theme": "Theme",
    "language": "Language"
  }
}

// fa.json (similar structure with RTL support)
{
  "app": {
    "name": "VPN SSH",
    "connect": "اتصال",
    "disconnect": "قطع اتصال"
  }
}
```

**RTL Support:**
```css
[dir="rtl"] {
  direction: rtl;
  text-align: right;
}
```

---

## 5. TESTING AND VALIDATION

### Unit Tests (Rust):
```rust
#[cfg(test)]
mod tests {
    #[test]
    fn test_reconnect_backoff() { }
    #[test]
    fn test_server_serialization() { }
    #[test]
    fn test_kill_switch_rules() { }
    #[test]
    fn test_dns_protection() { }
}
```

### Integration Tests:
```typescript
// Playwright
describe('SSH VPN Tauri', () => {
  it('connects to SSH server', async () => { });
  it('auto-reconnects after disconnect', async () => { });
  it('switches theme correctly', async () => { });
  it('changes language to Farsi', async () => { });
  it('displays bandwidth chart', async () => { });
  it('kill switch blocks traffic', async () => { });
});
```

### Success Criteria:
| معیار | هدف |
|-------|-----|
| سایز فایل نهایی | < 15 MB |
| زمان cold start | < 2 ثانیه |
| Memory usage | < 100 MB |
| Auto-reconnect | Success within 3 attempts |
| Kill Switch | 100% traffic block when active |
| UI Animation | 60 FPS |

---

## 6. FINAL PROJECT STRUCTURE

```
ssh-vpn-tauri/
├── src-tauri/
│   ├── Cargo.toml
│   ├── tauri.conf.json
│   ├── build.rs
│   ├── icons/
│   │   ├── icon.ico
│   │   └── icon.png
│   └── src/
│       ├── main.rs              # Entry point
│       ├── lib.rs               # Library exports
│       ├── ssh_client.rs        # Core SSH + SOCKS5
│       ├── reconnect.rs         # Auto-reconnect logic
│       ├── failover.rs          # Multi-server failover
│       ├── kill_switch.rs       # Windows Firewall
│       ├── dns_protection.rs    # DNS leak protection
│       ├── bandwidth.rs          # Bandwidth monitoring
│       ├── geoip.rs             # Server location
│       ├── storage.rs           # JSON file storage
│       ├── commands.rs          # Tauri IPC commands
│       └── utils/
│           ├── mod.rs
│           ├── logging.rs
│           └── error.rs
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── index.html
│   ├── components/
│   │   ├── Dashboard/
│   │   │   ├── ConnectionCard.tsx
│   │   │   ├── ServerSelector.tsx
│   │   │   ├── BandwidthChart.tsx
│   │   │   └── StatsPanel.tsx
│   │   ├── ServerList/
│   │   │   ├── ServerCard.tsx
│   │   │   ├── ServerList.tsx
│   │   │   └── AddEditModal.tsx
│   │   ├── Settings/
│   │   │   ├── Settings.tsx
│   │   │   └── SettingsSection.tsx
│   │   ├── Layout/
│   │   │   ├── Header.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── StatusBar.tsx
│   │   └── UI/
│   │       ├── Button.tsx
│   │       ├── Card.tsx
│   │       ├── Toggle.tsx
│   │       └── Modal.tsx
│   ├── hooks/
│   │   ├── useConnection.ts
│   │   ├── useServers.ts
│   │   ├── useSettings.ts
│   │   └── useBandwidth.ts
│   ├── store/
│   │   └── appStore.ts          # Zustand
│   ├── i18n/
│   │   ├── index.ts
│   │   ├── en.json
│   │   └── fa.json
│   ├── styles/
│   │   ├── index.css
│   │   ├── variables.css
│   │   └── animations.css
│   ├── types/
│   │   └── index.ts
│   └── utils/
│       ├── format.ts
│       └── constants.ts
├── package.json
├── tsconfig.json
├── vite.config.ts
├── tailwind.config.js
├── postcss.config.js
└── SPEC.md
```
