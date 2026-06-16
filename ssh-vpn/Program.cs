using System;
using System.Windows.Forms;

namespace ssh_vpn
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ShowUnhandledException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            ShowUnhandledException(exception ?? new Exception("Unknown application error."));
            Environment.Exit(1);
        }

        private static void ShowUnhandledException(Exception exception)
        {
            try
            {
                string message = "Unexpected error:" + Environment.NewLine + exception.Message;
                MessageBox.Show(message, LanguageManager.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Avoid recursive crashes while showing the error dialog.
            }
        }
    }
}
