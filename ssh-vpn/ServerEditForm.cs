using System;
using System.Drawing;
using System.Windows.Forms;

namespace ssh_vpn
{
    public partial class ServerEditForm : Form
    {
        public string ServerName { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ServerEditForm()
        {
            InitializeComponent();
            Port = 22;
            UpdateLanguage();
            ApplyTheme();
        }

        public ServerEditForm(ServerInfo server) : this()
        {
            ServerName = server == null ? "" : server.Name;
            IPAddress = server == null ? "" : server.IPAddress;
            Port = server == null ? 22 : server.Port;
            Username = server == null ? "" : server.Username;
            Password = server == null ? "" : server.Password;
            LoadValues();
        }

        private void UpdateLanguage()
        {
            this.Text = LanguageManager.GetString("EditServer");
            lblName.Text = LanguageManager.GetString("ServerName");
            lblIP.Text = LanguageManager.GetString("IPAddress");
            lblPort.Text = LanguageManager.GetString("ServerPort");
            lblUsername.Text = LanguageManager.GetString("Username");
            lblPassword.Text = LanguageManager.GetString("Password");
            btnSave.Text = LanguageManager.GetString("Save");
            btnCancel.Text = LanguageManager.GetString("Close");
        }

        private void ApplyTheme()
        {
            try
            {
                var colors = ThemeManager.GetColors();
                this.BackColor = colors.Background;
                this.ForeColor = colors.Foreground;

                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is TextBox)
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
            catch
            {
                // Theme changes should never break the edit form.
            }
        }

        private void LoadValues()
        {
            txtName.Text = ServerName;
            txtIP.Text = IPAddress;
            txtPort.Text = Port.ToString();
            txtUsername.Text = Username;
            txtPassword.Text = Password;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string ip = txtIP.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill all fields", LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int port;
            if (!int.TryParse(txtPort.Text, out port) || !ServerManager.IsValidPort(port))
            {
                MessageBox.Show(LanguageManager.GetString("InvalidPort"), LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPort.Focus();
                return;
            }

            ServerName = name;
            IPAddress = ip;
            Port = port;
            Username = username;
            Password = password;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
