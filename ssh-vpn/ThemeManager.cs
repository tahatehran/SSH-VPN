using System.Drawing;

namespace ssh_vpn
{
    public static class ThemeManager
    {
        public enum Theme { Light, Dark }
        public static Theme CurrentTheme = Theme.Light;

        public struct ThemeColors
        {
            public Color Background;
            public Color Foreground;
            public Color ControlBack;
            public Color ControlFore;
            public Color Accent;
            public Color Success;
            public Color Error;
        }

        public static ThemeColors GetColors()
        {
            if (CurrentTheme == Theme.Dark)
            {
                return new ThemeColors
                {
                    Background = Color.FromArgb(28, 28, 28),
                    Foreground = Color.FromArgb(240, 240, 240),
                    ControlBack = Color.FromArgb(45, 45, 48),
                    ControlFore = Color.FromArgb(240, 240, 240),
                    Accent = Color.FromArgb(0, 122, 204),
                    Success = Color.FromArgb(34, 139, 34),
                    Error = Color.FromArgb(178, 34, 34)
                };
            }
            return new ThemeColors
            {
                Background = Color.FromArgb(245, 245, 245),
                Foreground = Color.FromArgb(30, 30, 30),
                ControlBack = Color.White,
                ControlFore = Color.FromArgb(30, 30, 30),
                Accent = Color.FromArgb(65, 105, 225),
                Success = Color.FromArgb(46, 139, 87),
                Error = Color.FromArgb(220, 20, 60)
            };
        }
    }
}
