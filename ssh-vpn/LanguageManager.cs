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
        };

        public static string GetString(string key)
        {
            if (translations.ContainsKey(key))
                return translations[key][CurrentLanguage];
            return key;
        }
    }
}
