using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ssh_vpn
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            ApplyLanguage();
            ApplyTheme();
        }

        private void ApplyLanguage()
        {
            this.Text = LanguageManager.GetString("Settings");
            lblIp.Text = LanguageManager.GetString("IPAddress");
            lblPort.Text = LanguageManager.GetString("ServerPort");
            lblUser.Text = LanguageManager.GetString("Username");
            lblPass.Text = LanguageManager.GetString("Password");
            lblLang.Text = LanguageManager.GetString("Language");
            lblTheme.Text = LanguageManager.GetString("Theme");
            btn_save.Text = LanguageManager.GetString("Save");

            int themeIdx = cmbTheme.SelectedIndex;
            cmbTheme.Items.Clear();
            cmbTheme.Items.Add(LanguageManager.GetString("Light"));
            cmbTheme.Items.Add(LanguageManager.GetString("Dark"));
            cmbTheme.SelectedIndex = themeIdx >= 0 && themeIdx < cmbTheme.Items.Count ? themeIdx : (int)ThemeManager.CurrentTheme;

            if (LanguageManager.CurrentLanguage == LanguageManager.Language.Farsi)
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
            }
            else
            {
                this.RightToLeft = RightToLeft.No;
                this.RightToLeftLayout = false;
            }
        }

        private void ApplyTheme()
        {
            try
            {
                var colors = ThemeManager.GetColors();
                ApplyTheme(this, colors);
            }
            catch
            {
                // Theme changes should never break the settings form.
            }
        }

        private void ApplyTheme(Control control, ThemeManager.ThemeColors colors)
        {
            control.BackColor = colors.Background;
            control.ForeColor = colors.Foreground;

            foreach (Control ctrl in control.Controls)
            {
                if (ctrl is TextBox || ctrl is NumericUpDown || ctrl is ComboBox)
                {
                    ctrl.BackColor = colors.ControlBack;
                    ctrl.ForeColor = colors.ControlFore;
                }
                else if (ctrl is Button)
                {
                    ctrl.BackColor = colors.ControlBack;
                    ctrl.ForeColor = colors.ControlFore;
                    ((Button)ctrl).FlatStyle = FlatStyle.Flat;
                    ((Button)ctrl).FlatAppearance.BorderColor = colors.Accent;
                }
                else if (ctrl is Label)
                {
                    ctrl.ForeColor = colors.Foreground;
                }

                if (ctrl.HasChildren)
                    ApplyTheme(ctrl, colors);
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                string keyName = "ssh_vpn";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyName))
                {
                    if (key == null)
                        throw new InvalidOperationException(LanguageManager.GetString("RegistryError"));

                    key.SetValue("ip", txt_ip.Text.Trim());
                    key.SetValue("port", (int)txt_port.Value);
                    key.SetValue("username", txt_username.Text.Trim());
                    key.SetValue("password", txt_password.Text);

                    key.SetValue("language", (int)LanguageManager.CurrentLanguage);
                    key.SetValue("theme", (int)ThemeManager.CurrentTheme);
                }

                MessageBox.Show(LanguageManager.GetString("SuccessSave"), LanguageManager.GetString("SuccessTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.GetString("RegistryError") + Environment.NewLine + ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            txt_port.Minimum = 1;
            txt_port.Maximum = 65535;
            txt_port.Value = GetRegistryInt("port", 22);
            txt_ip.Text = GetRegistryString("ip");
            txt_username.Text = GetRegistryString("username");
            txt_password.Text = GetRegistryString("password");

            cmbLang.SelectedIndex = Math.Min((int)LanguageManager.CurrentLanguage, cmbLang.Items.Count - 1);
            cmbTheme.SelectedIndex = Math.Min((int)ThemeManager.CurrentTheme, cmbTheme.Items.Count - 1);
            ApplyLanguage();
            ApplyTheme();
        }

        private void cmbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLang.SelectedIndex >= 0 && cmbLang.SelectedIndex <= (int)LanguageManager.Language.Farsi)
            {
                LanguageManager.CurrentLanguage = (LanguageManager.Language)cmbLang.SelectedIndex;
                ApplyLanguage();
            }
        }

        private void cmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTheme.SelectedIndex >= 0 && cmbTheme.SelectedIndex <= (int)ThemeManager.Theme.Dark)
            {
                ThemeManager.CurrentTheme = (ThemeManager.Theme)cmbTheme.SelectedIndex;
                ApplyTheme();
            }
        }

        private string GetRegistryString(string name)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("ssh_vpn"))
                {
                    if (key == null)
                        return "";

                    object value = key.GetValue(name);
                    return value == null ? "" : Convert.ToString(value);
                }
            }
            catch
            {
                return "";
            }
        }

        private int GetRegistryInt(string name, int defaultValue)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("ssh_vpn"))
                {
                    if (key == null)
                        return defaultValue;

                    object value = key.GetValue(name);
                    int parsed;
                    if (int.TryParse(Convert.ToString(value), out parsed))
                        return parsed;
                }
            }
            catch
            {
                // Return the default value when registry access fails.
            }

            return defaultValue;
        }
    }
}
