# SSH VPN (C#)

<div dir="rtl">

## توضیحات (Persian)
این یک برنامه حرفه‌ای برای استفاده از پروتکل SSH به عنوان VPN است. این برنامه با ایجاد یک تونل SSH روی پورت 9000 و تنظیم خودکار پروکسی ویندوز، دسترسی آزاد به اینترنت را فراهم می‌کند.

## ویژگی‌های کلیدی
- **رابط کاربری مدرن**: پشتیبانی از حالت تاریک و روشن (Dark/Light Mode).
- **دو زبانه**: پشتیبانی کامل از زبان‌های فارسی و انگلیسی.
- **اتصال سریع**: بهینه‌سازی شده برای تونل‌زنی SSH.
- **تنظیم خودکار پروکسی**: پیکربندی خودکار تنظیمات پروکسی ویندوز.
- **خودکارسازی (CI/CD)**: مجهز به GitHub Actions برای ساخت خودکار و ابزارهای جانبی.

</div>

---

## Description (English)
A professional C# Windows application to use the SSH protocol as a VPN. It establishes an SSH session on port 9000 and automatically configures the local Windows proxy settings for seamless browsing.

## 🚀 Key Features
- **Modern UI**: Sleek design with Dark/Light mode support.
- **Bilingual**: Full support for English and Farsi (Persian).
- **Fast Connection**: Optimized SSH tunneling for better performance.
- **Auto Proxy**: Automatically handles Windows system proxy configuration.
- **Integrated CI/CD**: Powered by GitHub Actions for automated building and maintenance.

## 📸 Screenshots
![Main Interface](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/12b72ac7-92f6-47be-bdd5-abcabfc9458c)
![Settings](https://github.com/tahatehran/CSharp-SSH-VPN/assets/67155909/21a80309-2760-46df-b94c-bab82cce845d)

## ⚙️ GitHub Actions (Workflows)
We have a unified and robust CI/CD pipeline in `.github/workflows/main.yml`:
1.  **Build Application**: Automatically builds the C# solution and generates Release artifacts.
2.  **Video Downloader**: A powerful utility to download videos via `yt-dlp` (Manual trigger via `workflow_dispatch`).
3.  **Automatic Cleanup**: Ensures the environment stays clean after each run.

**To run the Video Downloader:**
Go to the **Actions** tab -> Select **SSH VPN Pipeline** -> Click **Run workflow** -> Enter the Video URL.

## 🛠 Project Roadmap (Phases)
- **Phase 1**: Core functionality and basic UI (Completed)
- **Phase 2**: Modern UI/UX overhaul, Dark Mode, and Animations (Completed)
- **Phase 3**: Localization (EN/FA) and CI/CD Integration (Completed)
- **Phase 4**: Advanced features like Server List, Ping detection, and Speed monitoring (Planned)

## 🤝 Support & Sponsorship
Supported by **Movti Group**.
Maintained by [tahatehran](https://github.com/tahatehran)

## 📥 Download
| Version | Download |
|---------|----------|
| Latest Release | [Download Here](https://github.com/tahatehran/CSharp-SSH-VPN/releases) |

## 🛡 Security & Guidelines
- **Credentials**: Check [PASS.md](./PASS.md) for info on how settings are stored.
- **Development**: Refer to [AGENT.md](./AGENT.md) for coding standards and AI instructions.

## Learn more
For more information about filtering and bypass techniques, visit [Hope Project](https://github.com/iranxray/hope).
