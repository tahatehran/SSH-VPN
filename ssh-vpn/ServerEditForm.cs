using System;
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
        }

        public ServerEditForm(ServerInfo server) : this()
        {
            ServerName = server.Name;
            IPAddress = server.IPAddress;
            Port = server.Port;
            Username = server.Username;
            Password = server.Password;
            LoadValues();
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
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtIP.Text) ||
                string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Please fill all fields", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ServerName = txtName.Text;
            IPAddress = txtIP.Text;
            int port;
            if (!int.TryParse(txtPort.Text, out port)) port = 22;
            Port = port;
            Username = txtUsername.Text;
            Password = txtPassword.Text;

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