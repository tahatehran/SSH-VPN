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
            cmbTheme.SelectedIndex = themeIdx;

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
            var colors = ThemeManager.GetColors();
            this.BackColor = colors.Background;
            this.ForeColor = colors.Foreground;

            foreach (Control ctrl in this.Controls)
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
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            string keyName = "ssh_vpn";
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyName))
            {
                key.SetValue("ip", txt_ip.Text);
                key.SetValue("port", txt_port.Value);
                key.SetValue("username", txt_username.Text);
                key.SetValue("password", txt_password.Text);

                key.SetValue("language", (int)LanguageManager.CurrentLanguage);
                key.SetValue("theme", (int)ThemeManager.CurrentTheme);

                MessageBox.Show(LanguageManager.GetString("SuccessSave"), LanguageManager.GetString("SuccessTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            this.Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            string keyName = "ssh_vpn";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (key != null)
                {
                    txt_ip.Text = key.GetValue("ip") as string;
                    int port = 22;
                    if (int.TryParse(key.GetValue("port") as string, out port))
                        txt_port.Value = port;
                    txt_username.Text = key.GetValue("username") as string;
                    txt_password.Text = key.GetValue("password") as string;
                }
            }

            cmbLang.SelectedIndex = (int)LanguageManager.CurrentLanguage;
            cmbTheme.SelectedIndex = (int)ThemeManager.CurrentTheme;
            ApplyLanguage();
            ApplyTheme();
        }

        private void cmbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            LanguageManager.CurrentLanguage = (LanguageManager.Language)cmbLang.SelectedIndex;
            ApplyLanguage();
        }

        private void cmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThemeManager.CurrentTheme = (ThemeManager.Theme)cmbTheme.SelectedIndex;
            ApplyTheme();
        }
    }
}
