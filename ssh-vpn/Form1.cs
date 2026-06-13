using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Renci.SshNet;


namespace ssh_vpn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
            LoadSettings();
        }

        SshClient sshClient = new SshClient("0.0.0.0", 22, "0000", "0000");
        ForwardedPortDynamic portForwarded = new ForwardedPortDynamic(9000);
        private BandwidthMonitor bandwidthMonitor;

        private void LoadSettings()
        {
            string keyName = "ssh_vpn";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (key != null)
                {
                    object langValue = key.GetValue("language");
                    if (langValue != null)
                        LanguageManager.CurrentLanguage = (LanguageManager.Language)Convert.ToInt32(langValue);

                    object themeValue = key.GetValue("theme");
                    if (themeValue != null)
                        ThemeManager.CurrentTheme = (ThemeManager.Theme)Convert.ToInt32(themeValue);
                }
            }
            UpdateLanguage();
            ApplyTheme();
        }

        public void UpdateLanguage()
        {
            this.Text = "SSH VPN";
            btnOpenSettings.Text = LanguageManager.GetString("OpenSettings");
            lblSponsor.Text = LanguageManager.CurrentLanguage == LanguageManager.Language.Farsi ? "حمایت شده توسط Movti Group" : "Supported by Movti Group";

            if (sshClient.IsConnected)
            {
                btnToggle.Text = LanguageManager.GetString("Disconnect");
                lblStatus.Text = LanguageManager.GetString("Connected") + "      " + GetTimeString();
            }
            else
            {
                btnToggle.Text = LanguageManager.GetString("Connect");
                if (btnToggle.Text == "Connect" || btnToggle.Text == "اتصال")
                    lblStatus.Text = LanguageManager.GetString("NotConnected");
            }

            if (LanguageManager.CurrentLanguage == LanguageManager.Language.Farsi)
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
                lblPing.TextAlign = ContentAlignment.TopLeft;
            }
            else
            {
                this.RightToLeft = RightToLeft.No;
                this.RightToLeftLayout = false;
                lblPing.TextAlign = ContentAlignment.TopRight;
            }
        }

        private void ApplyTheme()
        {
            var colors = ThemeManager.GetColors();
            this.BackColor = colors.Background;
            this.ForeColor = colors.Foreground;

            btnToggle.BackColor = colors.ControlBack;
            btnToggle.ForeColor = colors.ControlFore;
            btnToggle.FlatAppearance.BorderColor = colors.Accent;

            btnOpenSettings.BackColor = colors.ControlBack;
            btnOpenSettings.ForeColor = colors.ControlFore;
            btnOpenSettings.FlatAppearance.BorderColor = colors.Accent;

            lblSponsor.ForeColor = colors.Foreground;
            lblPing.ForeColor = colors.Foreground;

            if (!sshClient.IsConnected)
            {
                lblStatus.BackColor = colors.Error;
            }
            else
            {
                lblStatus.BackColor = colors.Success;
            }
            lblStatus.ForeColor = Color.White;
        }

        private string GetTimeString()
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int remainingSeconds = seconds % 60;
            return hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + remainingSeconds.ToString("D2");
        }


        void Connect()
        {
            btnToggle.Text = LanguageManager.GetString("Connecting");
            var colors = ThemeManager.GetColors();

            string password = registery_get_data("password");
            string username = registery_get_data("username");
            string ip = registery_get_data("ip");
            int port;

            if (!int.TryParse(registery_get_data("port"), out port)) port = 22;

            if (password == "" || username == "" || ip == "")
            {
                MessageBox.Show(LanguageManager.GetString("ErrorSettings"), LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Invoke((MethodInvoker)delegate
                {
                    btnOpenSettings_Click(null, null);
                    btnToggle.Text = LanguageManager.GetString("Connect");
                });
                return;
            }


            ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
            {
                sshClient = new SshClient(ip, port, username, password);

                try
                {
                    sshClient.Connect();
                    sshClient.AddForwardedPort(portForwarded);
                    portForwarded.Start();

                    set_windows_proxy();

                    Invoke((MethodInvoker)delegate
                    {
                        lblStatus.BackColor = colors.Success;
                        lblStatus.Text = LanguageManager.GetString("Connected") + "      00:00:00";
                        btnToggle.Text = LanguageManager.GetString("Disconnect");

                        timer_check_status.Enabled = true;
                        timer_check_status.Start();

                        // Start bandwidth monitoring
                        if (bandwidthMonitor == null)
                        {
                            bandwidthMonitor = new BandwidthMonitor();
                            bandwidthMonitor.BandwidthUpdated += (status) => {
                                lblBandwidth.Text = status;
                            };
                        }
                        bandwidthMonitor.StartMonitoring();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LanguageManager.GetString("ErrorTitle") + " : " + ex.Message, LanguageManager.GetString("ConnectionErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Invoke((MethodInvoker)delegate { btnToggle.Text = LanguageManager.GetString("Connect"); });
                }
                finally
                {
                    Invoke((MethodInvoker)delegate { btnToggle.Enabled = true; });
                }
            }));
        }

        void Disconnect()
        {
            btnToggle.Text = LanguageManager.GetString("Disconnecting");
            var colors = ThemeManager.GetColors();

            portForwarded.Stop();
            sshClient.Disconnect();
            unset_windows_proxy();

            btnToggle.Text = LanguageManager.GetString("Connect");
            lblStatus.BackColor = colors.Error;
            lblStatus.Text = LanguageManager.GetString("NotConnected");
            lblPing.Text = "Ping: --- ms";
            lblBandwidth.Text = "Bandwidth: 0 KB/s";

            timer_check_status.Enabled = false;
            timer_check_status.Stop();
            seconds = 0;

            if (bandwidthMonitor != null)
            {
                bandwidthMonitor.Reset();
            }

            btnToggle.Enabled = true;
        }


        private void btnToggle_Click(object sender, EventArgs e)
        {
            btnToggle.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            if (sshClient.IsConnected)
                Disconnect();

            else Connect();

            Cursor.Current = Cursors.Default;
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!sshClient.IsConnected) return;

            DialogResult result = MessageBox.Show(LanguageManager.GetString("ExitConfirm"), LanguageManager.GetString("ExitTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                e.Cancel = true;
            else
                Disconnect();

        }
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (sshClient.IsConnected)
                btnToggle_Click(null, null);

            Application.Exit();
        }

        private void btnOpenSettings_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
            LoadSettings();
        }

        private void btnServerList_Click(object sender, EventArgs e)
        {
            ServerListForm serverListForm = new ServerListForm();
            if (serverListForm.ShowDialog() == DialogResult.OK)
            {
                // Server was selected and connected
                LoadSettings();
            }
        }

        bool back_status = false;
        int seconds = 0;
        private void timer_check_status_Tick(object sender, EventArgs e)
        {
            if (!sshClient.IsConnected && sshClient.IsConnected != back_status)
            {
                Disconnect();

                notifyIcon1.Icon = SystemIcons.Warning;
                notifyIcon1.ShowBalloonTip(10);
            }
            else if (sshClient.IsConnected && sshClient.IsConnected != back_status)
            {
                seconds = 0;
                lblStatus.Text = LanguageManager.GetString("Connected") + "      00:00:00";
            }
            else if (sshClient.IsConnected)
            { 
                seconds++;
                lblStatus.Text = LanguageManager.GetString("Connected") + "      " + GetTimeString();

                // Update Ping every 5 seconds
                if (seconds % 5 == 0)
                {
                    UpdatePing();
                }
            }

            back_status = sshClient.IsConnected;
        }

        private void UpdatePing()
        {
            string ip = registery_get_data("ip");
            if (string.IsNullOrEmpty(ip)) return;

            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    Ping ping = new Ping();
                    PingReply reply = ping.Send(ip, 2000);
                    if (reply.Status == IPStatus.Success)
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            lblPing.Text = "Ping: " + reply.RoundtripTime + " ms";
                        });
                    }
                    else
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            lblPing.Text = "Ping: Timeout";
                        });
                    }
                }
                catch
                {
                    Invoke((MethodInvoker)delegate
                    {
                        lblPing.Text = "Ping: Error";
                    });
                }
            });
        }

        private void set_windows_proxy()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 1);
            registry.SetValue("ProxyServer", "socks5://127.0.0.1:9000");

            WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }

        private void unset_windows_proxy()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registry.SetValue("ProxyEnable", 0);
            registry.SetValue("ProxyServer", "");
        }

        private string registery_get_data(string name)
        {
            string keyName = "ssh_vpn";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (key == null)
                    return "";
                else
                    return key.GetValue(name) as string;
            }
        }

        private void btnGh_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/tahatehran/CSharp-SSH-VPN");
        }
    }

    public static class WinINetInterop
    {
        public const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
        public const int INTERNET_OPTION_REFRESH = 37;

        [System.Runtime.InteropServices.DllImport("wininet.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
    }
}
