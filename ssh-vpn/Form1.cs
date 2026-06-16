using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace ssh_vpn
{
    public partial class Form1 : Form
    {
        private const int DynamicProxyPort = 9000;
        private const int ConnectTimeoutSeconds = 10;

        private readonly object connectionLock = new object();

        private SshClient sshClient;
        private ForwardedPortDynamic currentPortForwarded;
        private BandwidthMonitor bandwidthMonitor;

        private bool isBusy;
        private bool closeAfterDisconnect;
        private bool isFormClosing;
        private volatile bool cancelConnection;
        private bool back_status;
        private int seconds;
        private int pingInProgress;

        public Form1()
        {
            InitializeComponent();
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
            FormClosed += Form1_FormClosed;
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("ssh_vpn"))
                {
                    if (key != null)
                    {
                        object langValue = key.GetValue("language");
                        if (langValue != null)
                        {
                            int languageIndex;
                            if (int.TryParse(Convert.ToString(langValue), out languageIndex) && Enum.IsDefined(typeof(LanguageManager.Language), languageIndex))
                                LanguageManager.CurrentLanguage = (LanguageManager.Language)languageIndex;
                        }

                        object themeValue = key.GetValue("theme");
                        if (themeValue != null)
                        {
                            int themeIndex;
                            if (int.TryParse(Convert.ToString(themeValue), out themeIndex) && Enum.IsDefined(typeof(ThemeManager.Theme), themeIndex))
                                ThemeManager.CurrentTheme = (ThemeManager.Theme)themeIndex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(LanguageManager.GetString("RegistryError") + Environment.NewLine + ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxIcon.Error);
            }

            UpdateLanguage();
            ApplyTheme();
        }

        public void UpdateLanguage()
        {
            this.Text = "SSH VPN";
            btnOpenSettings.Text = LanguageManager.GetString("OpenSettings");
            lblSponsor.Text = LanguageManager.CurrentLanguage == LanguageManager.Language.Farsi ? "حمایت شده توسط Movti Group" : "Supported by Movti Group";

            if (IsSshConnected())
            {
                btnToggle.Text = LanguageManager.GetString("Disconnect");
                lblStatus.Text = LanguageManager.GetString("Connected") + "      " + GetTimeString();
            }
            else
            {
                btnToggle.Text = LanguageManager.GetString("Connect");
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
            try
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

                btnServerList.BackColor = colors.ControlBack;
                btnServerList.ForeColor = colors.ControlFore;
                btnServerList.FlatAppearance.BorderColor = colors.Accent;

                lblSponsor.ForeColor = colors.Foreground;
                lblPing.ForeColor = colors.Foreground;
                lblBandwidth.ForeColor = colors.Foreground;

                lblStatus.BackColor = IsSshConnected() ? colors.Success : colors.Error;
                lblStatus.ForeColor = Color.White;
            }
            catch
            {
                // Theme changes should never break the main form.
            }
        }

        private string GetTimeString()
        {
            int safeSeconds = Math.Max(0, seconds);
            int hours = safeSeconds / 3600;
            int minutes = (safeSeconds % 3600) / 60;
            int remainingSeconds = safeSeconds % 60;
            return hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + remainingSeconds.ToString("D2");
        }

        private void Connect()
        {
            if (!Monitor.TryEnter(connectionLock))
                return;

            try
            {
                if (isBusy)
                    return;

                isBusy = true;
                cancelConnection = false;
                ThemeManager.ThemeColors colors = ThemeManager.GetColors();

                RunOnUiThread(delegate
                {
                    btnToggle.Text = LanguageManager.GetString("Connecting");
                    btnToggle.Enabled = false;
                    Cursor.Current = Cursors.WaitCursor;
                });

                string password = GetRegistryString("password");
                string username = GetRegistryString("username");
                string ip = GetRegistryString("ip");
                int port = GetRegistryInt("port", 22);

                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(ip))
                {
                    isBusy = false;
                    cancelConnection = false;
                    RunOnUiThread(delegate
                    {
                        ShowError(LanguageManager.GetString("ErrorSettings"), LanguageManager.GetString("ErrorTitle"), MessageBoxIcon.Error);
                        btnOpenSettings_Click(null, null);
                        ResetDisconnectedUi(colors);
                        btnToggle.Enabled = true;
                        Cursor.Current = Cursors.Default;
                    });
                    return;
                }

                if (port < 1 || port > 65535)
                {
                    isBusy = false;
                    cancelConnection = false;
                    RunOnUiThread(delegate
                    {
                        ShowError(LanguageManager.GetString("InvalidPort"), LanguageManager.GetString("ErrorTitle"), MessageBoxIcon.Error);
                        btnOpenSettings_Click(null, null);
                        ResetDisconnectedUi(colors);
                        btnToggle.Enabled = true;
                        Cursor.Current = Cursors.Default;
                    });
                    return;
                }

                ThreadPool.QueueUserWorkItem(delegate
                {
                    ConnectInBackground(ip, port, username, password, colors);
                });
            }
            finally
            {
                Monitor.Exit(connectionLock);
            }
        }

        private void ConnectInBackground(string ip, int port, string username, string password, ThemeManager.ThemeColors colors)
        {
            SshClient client = null;
            ForwardedPortDynamic forwardedPort = null;
            bool connectionEstablished = false;

            try
            {
                if (cancelConnection || isFormClosing)
                    throw new OperationCanceledException();

                ConnectionInfo connectionInfo = new ConnectionInfo(ip, port, username, new PasswordAuthenticationMethod(username, password));
                connectionInfo.Timeout = TimeSpan.FromSeconds(ConnectTimeoutSeconds);

                client = new SshClient(connectionInfo);
                client.KeepAliveInterval = TimeSpan.FromSeconds(30);
                client.Connect();

                forwardedPort = new ForwardedPortDynamic(DynamicProxyPort);
                client.AddForwardedPort(forwardedPort);
                forwardedPort.Start();

                set_windows_proxy();

                if (cancelConnection || isFormClosing)
                    throw new OperationCanceledException();

                lock (connectionLock)
                {
                    sshClient = client;
                    currentPortForwarded = forwardedPort;
                }

                connectionEstablished = true;

                bool uiUpdated = RunOnUiThread(delegate
                {
                    lblStatus.BackColor = colors.Success;
                    lblStatus.Text = LanguageManager.GetString("Connected") + "      00:00:00";
                    btnToggle.Text = LanguageManager.GetString("Disconnect");

                    timer_check_status.Enabled = true;
                    timer_check_status.Start();

                    seconds = 0;
                    back_status = true;
                    lblPing.Text = "Ping: --- ms";
                    lblBandwidth.Text = "Bandwidth: 0 KB/s";

                    if (bandwidthMonitor == null || !bandwidthMonitor.IsRunning)
                    {
                        bandwidthMonitor = new BandwidthMonitor();
                        bandwidthMonitor.BandwidthUpdated += delegate(string status)
                        {
                            RunOnUiThread(delegate
                            {
                                lblBandwidth.Text = status;
                            });
                        };
                    }

                    bandwidthMonitor.StartMonitoring();
                });

                if (!uiUpdated)
                    throw new OperationCanceledException();
            }
            catch (Exception ex)
            {
                DisconnectClient(client);
                StopPortForward(forwardedPort);
                UnsetWindowsProxySafe();

                if (ex is OperationCanceledException)
                {
                    RunOnUiThread(delegate
                    {
                        ResetDisconnectedUi(colors);
                    });
                }
                else
                {
                    RunOnUiThread(delegate
                    {
                        ShowConnectionError(ex);
                        ResetDisconnectedUi(colors);
                    });
                }
            }
            finally
            {
                if (!connectionEstablished)
                {
                    lock (connectionLock)
                    {
                        if (ReferenceEquals(sshClient, client))
                            sshClient = null;
                        if (ReferenceEquals(currentPortForwarded, forwardedPort))
                            currentPortForwarded = null;
                    }
                }

                cancelConnection = false;
                RunOnUiThread(delegate
                {
                    isBusy = false;
                    btnToggle.Enabled = true;
                    Cursor.Current = Cursors.Default;
                });
            }
        }

        private void Disconnect()
        {
            if (!Monitor.TryEnter(connectionLock))
                return;

            try
            {
                if (isBusy && !IsSshConnected())
                {
                    cancelConnection = true;
                    RunOnUiThread(delegate
                    {
                        btnToggle.Text = LanguageManager.GetString("Disconnecting");
                        btnToggle.Enabled = false;
                        Cursor.Current = Cursors.WaitCursor;
                    });
                    return;
                }

                if (isBusy)
                    return;

                isBusy = true;
                cancelConnection = true;
                ThemeManager.ThemeColors colors = ThemeManager.GetColors();

                RunOnUiThread(delegate
                {
                    btnToggle.Text = LanguageManager.GetString("Disconnecting");
                    btnToggle.Enabled = false;
                    Cursor.Current = Cursors.WaitCursor;
                });

                ThreadPool.QueueUserWorkItem(delegate
                {
                    DisconnectInBackground(colors);
                });
            }
            finally
            {
                Monitor.Exit(connectionLock);
            }
        }

        private void DisconnectInBackground(ThemeManager.ThemeColors colors)
        {
            SshClient clientToDisconnect = null;
            ForwardedPortDynamic portToStop = null;

            try
            {
                lock (connectionLock)
                {
                    clientToDisconnect = sshClient;
                    portToStop = currentPortForwarded;
                    sshClient = null;
                    currentPortForwarded = null;
                }

                StopPortForward(portToStop);
                DisconnectClient(clientToDisconnect);
                unset_windows_proxy();
                StopBandwidthMonitor();

                RunOnUiThread(delegate
                {
                    ResetDisconnectedUi(colors);
                });
            }
            catch (Exception ex)
            {
                StopBandwidthMonitor();
                RunOnUiThread(delegate
                {
                    ShowError(LanguageManager.GetString("DisconnectErrorTitle") + " : " + ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxIcon.Error);
                    ResetDisconnectedUi(colors);
                });
            }
            finally
            {
                cancelConnection = false;
                RunOnUiThread(delegate
                {
                    isBusy = false;
                    btnToggle.Enabled = true;
                    Cursor.Current = Cursors.Default;
                });

                if (closeAfterDisconnect)
                {
                    closeAfterDisconnect = false;
                    RunOnUiThread(delegate
                    {
                        if (IsHandleCreated && !IsDisposed)
                            Close();
                    });
                }
            }
        }

        private void DisconnectNow()
        {
            isFormClosing = true;
            cancelConnection = true;
            StopBandwidthMonitor();

            SshClient clientToDisconnect = null;
            ForwardedPortDynamic portToStop = null;

            lock (connectionLock)
            {
                clientToDisconnect = sshClient;
                portToStop = currentPortForwarded;
                sshClient = null;
                currentPortForwarded = null;
            }

            StopPortForward(portToStop);
            DisconnectClient(clientToDisconnect);
            UnsetWindowsProxySafe();
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            if (isBusy)
                return;

            if (IsSshConnected())
                Disconnect();
            else
                Connect();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isFormClosing)
                return;

            if (IsSshConnected() || isBusy)
            {
                DialogResult result = MessageBox.Show(LanguageManager.GetString("ExitConfirm"), LanguageManager.GetString("ExitTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                e.Cancel = true;
                closeAfterDisconnect = true;

                if (isBusy)
                {
                    cancelConnection = true;
                    RunOnUiThread(delegate
                    {
                        btnToggle.Text = LanguageManager.GetString("Disconnecting");
                        btnToggle.Enabled = false;
                        Cursor.Current = Cursors.WaitCursor;
                    });
                }
                else
                {
                    RunOnUiThread(delegate
                    {
                        Disconnect();
                    });
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisconnectNow();
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            try
            {
                DisconnectNow();
            }
            finally
            {
                Application.Exit();
            }
        }

        private void btnOpenSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using (SettingsForm settingsForm = new SettingsForm())
                {
                    settingsForm.ShowDialog(this);
                }

                LoadSettings();
            }
            catch (Exception ex)
            {
                ShowError(LanguageManager.GetString("RegistryError") + Environment.NewLine + ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxIcon.Error);
            }
        }

        private void btnServerList_Click(object sender, EventArgs e)
        {
            try
            {
                using (ServerListForm serverListForm = new ServerListForm())
                {
                    if (serverListForm.ShowDialog(this) == DialogResult.OK)
                        LoadSettings();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxIcon.Error);
            }
        }

        private void timer_check_status_Tick(object sender, EventArgs e)
        {
            if (isFormClosing)
                return;

            bool connected = IsSshConnected();

            if (isBusy)
            {
                back_status = connected;
                return;
            }

            if (!connected && back_status)
            {
                Disconnect();
                notifyIcon1.Icon = SystemIcons.Warning;
                notifyIcon1.ShowBalloonTip(10);
            }
            else if (connected && !back_status)
            {
                seconds = 0;
                back_status = true;
                lblStatus.Text = LanguageManager.GetString("Connected") + "      00:00:00";
                lblPing.Text = "Ping: --- ms";
                lblBandwidth.Text = "Bandwidth: 0 KB/s";
            }
            else if (connected)
            {
                seconds++;
                lblStatus.Text = LanguageManager.GetString("Connected") + "      " + GetTimeString();

                if (seconds % 5 == 0)
                    UpdatePing();
            }
            else
            {
                back_status = false;
            }
        }

        private void UpdatePing()
        {
            string ip = GetRegistryString("ip");
            if (string.IsNullOrWhiteSpace(ip) || isFormClosing)
                return;

            if (Interlocked.Exchange(ref pingInProgress, 1) == 1)
                return;

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    using (Ping ping = new Ping())
                    {
                        PingReply reply = ping.Send(ip, 2000);
                        if (reply.Status == IPStatus.Success)
                        {
                            RunOnUiThread(delegate
                            {
                                lblPing.Text = "Ping: " + reply.RoundtripTime + " ms";
                            });
                        }
                        else
                        {
                            RunOnUiThread(delegate
                            {
                                lblPing.Text = "Ping: Timeout";
                            });
                        }
                    }
                }
                catch
                {
                    RunOnUiThread(delegate
                    {
                        lblPing.Text = "Ping: Error";
                    });
                }
                finally
                {
                    Interlocked.Exchange(ref pingInProgress, 0);
                }
            });
        }

        private void set_windows_proxy()
        {
            try
            {
                using (RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true))
                {
                    if (registry == null)
                        throw new InvalidOperationException("Could not open Windows proxy registry key.");

                    registry.SetValue("ProxyEnable", 1, RegistryValueKind.DWord);
                    registry.SetValue("ProxyServer", "socks5=127.0.0.1:" + DynamicProxyPort, RegistryValueKind.String);
                }

                if (!WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0))
                    throw new InvalidOperationException("Windows could not refresh proxy settings.");

                WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(LanguageManager.GetString("ProxyError"), ex);
            }
        }

        private void unset_windows_proxy()
        {
            try
            {
                using (RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true))
                {
                    if (registry == null)
                        return;

                    registry.SetValue("ProxyEnable", 0, RegistryValueKind.DWord);
                    registry.SetValue("ProxyServer", "", RegistryValueKind.String);
                }

                WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                WinINetInterop.InternetSetOption(IntPtr.Zero, WinINetInterop.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not restore Windows proxy settings.", ex);
            }
        }

        private void UnsetWindowsProxySafe()
        {
            try
            {
                unset_windows_proxy();
            }
            catch
            {
                // Best-effort cleanup during shutdown or failed connection attempts.
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
                    return value == null ? "" : Convert.ToString(value).Trim();
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

                    int value;
                    if (int.TryParse(Convert.ToString(key.GetValue(name)), out value))
                        return value;
                }
            }
            catch
            {
                // Return the default value when registry access fails.
            }

            return defaultValue;
        }

        private string registery_get_data(string name)
        {
            return GetRegistryString(name);
        }

        private bool IsSshConnected()
        {
            try
            {
                return sshClient != null && sshClient.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        private void StopBandwidthMonitor()
        {
            try
            {
                if (bandwidthMonitor != null)
                {
                    bandwidthMonitor.StopMonitoring();
                    bandwidthMonitor = null;
                }
            }
            catch
            {
                // Bandwidth monitoring is non-critical.
            }
        }

        private void StopPortForward(ForwardedPortDynamic port)
        {
            if (port == null)
                return;

            try
            {
                if (port.IsStarted)
                    port.Stop();
            }
            catch
            {
                // The SSH session may already be gone.
            }

            try
            {
                port.Dispose();
            }
            catch
            {
                // Ignore disposal errors.
            }
        }

        private void DisconnectClient(SshClient client)
        {
            if (client == null)
                return;

            try
            {
                if (client.IsConnected)
                    client.Disconnect();
            }
            catch
            {
                // The connection may already be closed.
            }

            try
            {
                client.Dispose();
            }
            catch
            {
                // Ignore disposal errors.
            }
        }

        private void ResetDisconnectedUi(ThemeManager.ThemeColors colors)
        {
            back_status = false;
            seconds = 0;
            timer_check_status.Enabled = false;
            timer_check_status.Stop();

            lblStatus.BackColor = colors.Error;
            lblStatus.Text = LanguageManager.GetString("NotConnected");
            lblPing.Text = "Ping: --- ms";
            lblBandwidth.Text = "Bandwidth: 0 KB/s";
            btnToggle.Text = LanguageManager.GetString("Connect");
        }

        private void ShowConnectionError(Exception ex)
        {
            if (ex is OperationCanceledException)
                return;

            ShowError(GetConnectionErrorMessage(ex), LanguageManager.GetString("ConnectionErrorTitle"), MessageBoxIcon.Error);
        }

        private string GetConnectionErrorMessage(Exception ex)
        {
            if (ex is SshAuthenticationException)
                return LanguageManager.GetString("AuthenticationError");

            if (ex is TimeoutException || (ex is SshConnectionException && ContainsTimeout(ex.Message)))
                return LanguageManager.GetString("ConnectionTimeout");

            if (ContainsTimeout(ex.Message))
                return LanguageManager.GetString("ConnectionTimeout");

            return ex.Message;
        }

        private bool ContainsTimeout(string message)
        {
            return !string.IsNullOrEmpty(message) && message.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ShowError(string message, string title, MessageBoxIcon icon)
        {
            RunOnUiThread(delegate
            {
                try
                {
                    MessageBox.Show(this, message, title, MessageBoxButtons.OK, icon);
                }
                catch
                {
                    // Avoid crashing when the form is already closing.
                }
            });
        }

        private bool RunOnUiThread(Action action)
        {
            if (action == null || IsDisposed)
                return false;

            try
            {
                if (InvokeRequired)
                {
                    if (!IsHandleCreated)
                        return false;

                    BeginInvoke((MethodInvoker)delegate
                    {
                        try
                        {
                            action();
                        }
                        catch
                        {
                            // Ignore UI update failures during shutdown.
                        }
                    });
                    return true;
                }

                action();
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private void btnGh_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/tahatehran/CSharp-SSH-VPN");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, LanguageManager.GetString("ErrorTitle"), MessageBoxIcon.Error);
            }
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
