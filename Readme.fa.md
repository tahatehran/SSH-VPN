# 🛡️ SSH VPN - Tauri

<div dir="rtl">

[![Build Status](https://github.com/tahatehran/CSharp-SSH-VPN/actions/workflows/main.yml/badge.svg)](https://github.com/tahatehran/CSharp-SSH-VPN/actions/workflows/main.yml)
[![Release](https://img.shields.io/github/v/release/tahatehran/CSharp-SSH-VPN?include_prereleases)](https://github.com/tahatehran/CSharp-SSH-VPN/releases)
[![Tauri](https://img.shields.io/badge/Tauri-2.0-ffc107.svg)](https://tauri.app/)
[![Rust](https://img.shields.io/badge/Rust-stable-orange.svg)](https://www.rust-lang.org/)

یک نرم‌افزار حرفه‌ای ویندوز که از پروتکل SSH برای ایجاد تونل امن (VPN) استفاده می‌کند. با **Tauri (Rust + React)** توسعه یافته و تجربه‌ای مدرن و سریع ارائه می‌دهد.

---

## 🚀 ویژگی‌های کلیدی

### نسخه مدرن (Tauri/Rust) ✨
- **🎨 رابط کاربری مدرن** - React + Tailwind CSS با انیمیشن‌های Framer Motion
- **⚡ عملکرد بالا** - بک‌اند Rust با Tokio async runtime
- **📦 حجم کم** - ~10MB در مقابل ~50MB برای .NET Framework
- **🌓 تم تطبیقی** - حالت تاریک/روشن/سیستم
- **🌍 پشتیبانی RTL** - زبان فارسی و انگلیسی
- **📊 نمودارهای لحظه‌ای** - مانیتورینگ پهنای باند با Recharts
- **🌐 موقعیت سرور** - تشخیص موقعیت جغرافیایی سرور
- **🔄 اتصال خودکار** - اتصال مجدد با backoff نمایی
- **💾 ذخیره‌سازی JSON** - مدیریت مدرن تنظیمات

---

## 📸 تصاویر محیط برنامه

| نسخه مدرن (Tauri) | نسخه کلاسیک (C#) |
| :---: | :---: |
| ![Tauri UI](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/12b72ac7-92f6-47be-bdd5-abcabfc9458c) | ![C# UI](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/21a80309-2760-46df-b94c-bab82cce845d) |

---

## ⚙️ خط لوله CI/CD

GitHub Actions workflow (`.github/workflows/main.yml`):
- **ساخت Tauri**: Rust + React → فایل اجرایی ویندوز (.exe) و نصب‌کننده (.msi)
- **ساخت خودکار**: فقط نسخه تغییر یافته ساخته می‌شود

---

## 🛠 نقشه راه پروژه

- [x] **فاز ۱**: موتور اصلی تونل‌زنی SSH و رابط کاربری پایه.
- [x] **فاز ۲**: بازطراحی رابط کاربری با تم‌های سفارشی و انیمیشن.
- [x] **فاز ۳**: پشتیبانی از چند زبانه و راه‌اندازی خط لوله CI/CD.
- [x] **فاز ۴**: مدیریت لیست سرورها، تشخیص پینگ لحظه‌ای و مانیتورینگ پهنای باند.
- [x] **فاز ۵**: بهبود قابلیت اطمینان اتصال.
- [x] **فاز ۶**: بازسازی Tauri - فرانت‌اند مدرن Rust + React ✅

---

## 🛡️ امنیت و توسعه

- **ذخیره‌سازی**: رمزهای SSH در فایل‌های JSON ذخیره می‌شوند
- **راهنما**: برای استانداردهای کدنویسی به [AGENT.md](./AGENT.md) مراجعه کنید

---

## 📥 دانلود و نصب

### 🆕 نسخه مدرن (توصیه شده)
1. به صفحه [Releases](https://github.com/tahatehran/CSharp-SSH-VPN/releases) بروید
2. `SSH VPN Tauri.exe` یا نصب‌کننده `.msi` را دانلود کنید
3. اجرا کنید و از رابط مدرن لذت ببرید!

---

## 🔧 توسعه

```bash
cd ssh-vpn-tauri
npm install
npm run tauri dev    # توسعه
npm run tauri build  # ساخت نسخه نهایی
```

---

## 📁 ساختار پروژه

```
CSharp-SSH-VPN/
├── ssh-vpn-tauri/     # اپلیکیشن اصلی (Rust + React)
├── .github/           # workflow های CI/CD
├── AGENT.md           # راهنمای توسعه‌دهندگان
└── README.md          # مستندات انگلیسی
```

---

## ارتباط با ما

- **ایمیل:** [info@movtigroup.ir](mailto:info@movtigroup.ir)
- **وب‌سایت:** [movtigroup.ir](https://movtigroup.ir)
- **گیت‌هاب:** [https://github.com/movtigroup](https://github.com/movtigroup)

---

### 🤝 حمایت و پشتیبانی
توسعه و نگهداری توسط [tahatehran](https://github.com/tahatehran).
با حمایت **Movti Group**.

برای اطلاعات بیشتر در مورد ابزارهای آزادی اینترنت، از [پروژه امید](https://github.com/iranxray/hope) دیدن کنید.

</div>
