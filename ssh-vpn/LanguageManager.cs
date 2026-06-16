using System.Collections.Generic;

namespace ssh_vpn
{
    public static class LanguageManager
    {
        public enum Language { English, Farsi }
        public static Language CurrentLanguage = Language.English;

        private static Dictionary<string, Dictionary<Language, string>> translations = new Dictionary<string, Dictionary<Language, string>>
        {
            ["Connect"] = new Dictionary<Language, string> { [Language.English] = "Connect", [Language.Farsi] = "اتصال" },
            ["Disconnect"] = new Dictionary<Language, string> { [Language.English] = "Disconnect", [Language.Farsi] = "قطع اتصال" },
            ["Connecting"] = new Dictionary<Language, string> { [Language.English] = "Connecting...", [Language.Farsi] = "در حال اتصال..." },
            ["Disconnecting"] = new Dictionary<Language, string> { [Language.English] = "Disconnecting...", [Language.Farsi] = "در حال قطع اتصال..." },
            ["NotConnected"] = new Dictionary<Language, string> { [Language.English] = "Not Connected", [Language.Farsi] = "متصل نیست" },
            ["Connected"] = new Dictionary<Language, string> { [Language.English] = "Connected", [Language.Farsi] = "متصل شد" },
            ["Settings"] = new Dictionary<Language, string> { [Language.English] = "Settings", [Language.Farsi] = "تنظیمات" },
            ["OpenSettings"] = new Dictionary<Language, string> { [Language.English] = "Open Settings", [Language.Farsi] = "باز کردن تنظیمات" },
            ["Save"] = new Dictionary<Language, string> { [Language.English] = "Save", [Language.Farsi] = "ذخیره" },
            ["IPAddress"] = new Dictionary<Language, string> { [Language.English] = "IP Address :", [Language.Farsi] = "آدرس آی‌پی :" },
            ["ServerPort"] = new Dictionary<Language, string> { [Language.English] = "Server port :", [Language.Farsi] = "پورت سرور :" },
            ["Username"] = new Dictionary<Language, string> { [Language.English] = "Username :", [Language.Farsi] = "نام کاربری :" },
            ["Password"] = new Dictionary<Language, string> { [Language.English] = "Password :", [Language.Farsi] = "رمز عبور :" },
            ["SuccessSave"] = new Dictionary<Language, string> { [Language.English] = "Successfully saved.", [Language.Farsi] = "با موفقیت ذخیره شد." },
            ["ErrorSettings"] = new Dictionary<Language, string> { [Language.English] = "Error : You should set SSH server settings...", [Language.Farsi] = "خطا: شما باید تنظیمات سرور SSH را وارد کنید..." },
            ["ExitConfirm"] = new Dictionary<Language, string> { [Language.English] = "Do you really wish to exit? the connection will be stopped.", [Language.Farsi] = "آیا واقعاً می‌خواهید خارج شوید؟ اتصال قطع خواهد شد." },
            ["ExitTitle"] = new Dictionary<Language, string> { [Language.English] = "Exit Program?", [Language.Farsi] = "خروج از برنامه؟" },
            ["SuccessTitle"] = new Dictionary<Language, string> { [Language.English] = "Success", [Language.Farsi] = "موفقیت" },
            ["ErrorTitle"] = new Dictionary<Language, string> { [Language.English] = "Error!", [Language.Farsi] = "خطا!" },
            ["ConnectionErrorTitle"] = new Dictionary<Language, string> { [Language.English] = "Connection Error", [Language.Farsi] = "خطای اتصال" },
            ["DisconnectErrorTitle"] = new Dictionary<Language, string> { [Language.English] = "Disconnect Error", [Language.Farsi] = "خطای قطع اتصال" },
            ["ConnectionTimeout"] = new Dictionary<Language, string> { [Language.English] = "Connection timed out. Check the server address, port, and network connection.", [Language.Farsi] = "اتصال زمان‌بر شد. آدرس سرور، پورت و اتصال شبکه را بررسی کنید." },
            ["AuthenticationError"] = new Dictionary<Language, string> { [Language.English] = "Authentication failed. Check the SSH username and password.", [Language.Farsi] = "احراز هویت ناموفق بود. نام کاربری و رمز عبور SSH را بررسی کنید." },
            ["ProxyError"] = new Dictionary<Language, string> { [Language.English] = "Windows proxy settings could not be configured.", [Language.Farsi] = "تنظیمات پراکسی ویندوز قابل اعمال نیست." },
            ["RegistryError"] = new Dictionary<Language, string> { [Language.English] = "Application settings could not be read or saved.", [Language.Farsi] = "تنظیمات برنامه قابل خواندن یا ذخیره نیست." },
            ["InvalidPort"] = new Dictionary<Language, string> { [Language.English] = "Port must be between 1 and 65535.", [Language.Farsi] = "پورت باید بین ۱ تا ۶۵۵۳۵ باشد." },
            ["Language"] = new Dictionary<Language, string> { [Language.English] = "Language :", [Language.Farsi] = "زبان :" },
            ["Theme"] = new Dictionary<Language, string> { [Language.English] = "Theme :", [Language.Farsi] = "پوسته :" },
            ["Light"] = new Dictionary<Language, string> { [Language.English] = "Light", [Language.Farsi] = "روشن" },
            ["Dark"] = new Dictionary<Language, string> { [Language.English] = "Dark", [Language.Farsi] = "تاریک" },
            ["ServerList"] = new Dictionary<Language, string> { [Language.English] = "Server List", [Language.Farsi] = "فهرست سرورها" },
            ["AddServer"] = new Dictionary<Language, string> { [Language.English] = "Add Server", [Language.Farsi] = "افزودن سرور" },
            ["EditServer"] = new Dictionary<Language, string> { [Language.English] = "Edit Server", [Language.Farsi] = "ویرایش سرور" },
            ["DeleteServer"] = new Dictionary<Language, string> { [Language.English] = "Delete Server", [Language.Farsi] = "حذف سرور" },
            ["ServerName"] = new Dictionary<Language, string> { [Language.English] = "Server Name :", [Language.Farsi] = "نام سرور :" },
            ["SelectServer"] = new Dictionary<Language, string> { [Language.English] = "Select a server from the list", [Language.Farsi] = "یک سرور از فهرست انتخاب کنید" },
            ["Bandwidth"] = new Dictionary<Language, string> { [Language.English] = "Bandwidth", [Language.Farsi] = "پهنای باند" },
            ["Ping"] = new Dictionary<Language, string> { [Language.English] = "Ping", [Language.Farsi] = "پینگ" },
            ["Confirm"] = new Dictionary<Language, string> { [Language.English] = "Confirm", [Language.Farsi] = "تأیید" },
            ["Close"] = new Dictionary<Language, string> { [Language.English] = "Close", [Language.Farsi] = "بستن" },
            ["DeleteConfirm"] = new Dictionary<Language, string> { [Language.English] = "Delete selected server?", [Language.Farsi] = "سرور انتخابی حذف شود؟" },
        };

        public static string GetString(string key)
        {
            Dictionary<Language, string> values;
            string value;
            if (translations.TryGetValue(key, out values) && values.TryGetValue(CurrentLanguage, out value))
                return value;
            return key;
        }
    }
}
