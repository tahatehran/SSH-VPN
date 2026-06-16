using System;
using System.Drawing;
using System.Windows.Forms;

namespace ssh_vpn
{
    public partial class ServerListForm : Form
    {
        public ServerListForm()
        {
            InitializeComponent();
            UpdateLanguage();
            ApplyTheme();
            LoadServers();
        }

        private void UpdateLanguage()
        {
            this.Text = LanguageManager.GetString("ServerList");
            btnAdd.Text = LanguageManager.GetString("AddServer");
            btnEdit.Text = LanguageManager.GetString("EditServer");
            btnDelete.Text = LanguageManager.GetString("DeleteServer");
            btnConnect.Text = LanguageManager.GetString("Connect");
            btnClose.Text = LanguageManager.GetString("Close");

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
                this.BackColor = colors.Background;
                this.ForeColor = colors.Foreground;

                ApplyButtonTheme(btnAdd, colors);
                ApplyButtonTheme(btnEdit, colors);
                ApplyButtonTheme(btnDelete, colors);
                ApplyButtonTheme(btnConnect, colors);
                ApplyButtonTheme(btnClose, colors);

                lstServers.BackColor = colors.ControlBack;
                lstServers.ForeColor = colors.ControlFore;
            }
            catch
            {
                // Theme changes should never break the server list.
            }
        }

        private void ApplyButtonTheme(Button button, ThemeManager.ThemeColors colors)
        {
            button.BackColor = colors.ControlBack;
            button.ForeColor = colors.ControlFore;
            button.FlatAppearance.BorderColor = colors.Accent;
        }

        private void LoadServers()
        {
            try
            {
                ServerManager.LoadServers();
                lstServers.Items.Clear();
                foreach (var server in ServerManager.Servers)
                {
                    if (server == null)
                        continue;

                    lstServers.Items.Add(server.Name + " (" + server.IPAddress + ":" + server.Port + ")");
                }
            }
            catch (Exception ex)
            {
                lstServers.Items.Clear();
                MessageBox.Show(ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var form = new ServerEditForm();
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var server = new ServerInfo
                    {
                        Name = form.ServerName,
                        IPAddress = form.IPAddress,
                        Port = form.Port,
                        Username = form.Username,
                        Password = form.Password
                    };
                    ServerManager.AddServer(server);
                    LoadServers();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstServers.SelectedIndex < 0) return;

                var server = ServerManager.GetServer(lstServers.SelectedIndex);
                if (server == null) return;

                var form = new ServerEditForm(server);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    server.Name = form.ServerName;
                    server.IPAddress = form.IPAddress;
                    server.Port = form.Port;
                    server.Username = form.Username;
                    server.Password = form.Password;
                    ServerManager.UpdateServer(lstServers.SelectedIndex, server);
                    LoadServers();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstServers.SelectedIndex < 0) return;

                if (MessageBox.Show(LanguageManager.GetString("DeleteConfirm"), LanguageManager.GetString("Confirm"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ServerManager.RemoveServer(lstServers.SelectedIndex);
                    LoadServers();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (lstServers.SelectedIndex < 0) return;

            var server = ServerManager.GetServer(lstServers.SelectedIndex);
            if (server == null) return;

            if (!ServerManager.IsValidPort(server.Port))
            {
                MessageBox.Show(LanguageManager.GetString("InvalidPort"), LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                SetCurrentServer(server);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void SetCurrentServer(ServerInfo server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ssh_vpn"))
            {
                if (key == null)
                    throw new InvalidOperationException(LanguageManager.GetString("RegistryError"));

                key.SetValue("ip", server.IPAddress);
                key.SetValue("port", server.Port.ToString());
                key.SetValue("username", server.Username);
                key.SetValue("password", server.Password);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
