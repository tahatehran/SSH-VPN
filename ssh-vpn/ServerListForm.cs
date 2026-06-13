using System;
using System.Windows.Forms;

namespace ssh_vpn
{
    public partial class ServerListForm : Form
    {
        public ServerListForm()
        {
            InitializeComponent();
            LoadServers();
        }

        private void LoadServers()
        {
            ServerManager.LoadServers();
            lstServers.Items.Clear();
            foreach (var server in ServerManager.Servers)
            {
                lstServers.Items.Add(server.Name + " (" + server.IPAddress + ":" + server.Port + ")");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var form = new ServerEditForm();
            if (form.ShowDialog() == DialogResult.OK)
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

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstServers.SelectedIndex < 0) return;

            var server = ServerManager.GetServer(lstServers.SelectedIndex);
            var form = new ServerEditForm(server);
            if (form.ShowDialog() == DialogResult.OK)
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstServers.SelectedIndex < 0) return;

            if (MessageBox.Show("Delete selected server?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ServerManager.RemoveServer(lstServers.SelectedIndex);
                LoadServers();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (lstServers.SelectedIndex < 0) return;

            var server = ServerManager.GetServer(lstServers.SelectedIndex);
            // Set the selected server as current and connect
            SetCurrentServer(server);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SetCurrentServer(ServerInfo server)
        {
            // Save to registry as current server
            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("ssh_vpn"))
            {
                key.SetValue("ip", server.IPAddress);
                key.SetValue("port", server.Port.ToString());
                key.SetValue("username", server.Username);
                key.SetValue("password", server.Password);
            }
        }
    }
}