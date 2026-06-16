using System;
using System.Diagnostics;
using System.Threading;

namespace ssh_vpn
{
    public class BandwidthMonitor
    {
        private readonly object syncRoot = new object();
        private PerformanceCounter downloadCounter;
        private PerformanceCounter uploadCounter;
        private long lastDownloadBytes = 0;
        private long lastUploadBytes = 0;
        private long totalDownloadBytes = 0;
        private long totalUploadBytes = 0;
        private Thread monitorThread;
        private volatile bool isRunning;

        public event Action<string> BandwidthUpdated;
        public bool IsRunning
        {
            get { return isRunning; }
        }

        public BandwidthMonitor()
        {
            try
            {
                downloadCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", "_Total");
                uploadCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", "_Total");
            }
            catch
            {
                // Performance counters may not be available on all systems.
            }
        }

        public void StartMonitoring()
        {
            if (downloadCounter == null || uploadCounter == null)
                return;

            lock (syncRoot)
            {
                if (isRunning)
                    return;

                isRunning = true;
                monitorThread = new Thread(MonitorLoop);
                monitorThread.IsBackground = true;
                monitorThread.Name = "SSH VPN Bandwidth Monitor";
                monitorThread.Start();
            }
        }

        public void StopMonitoring()
        {
            Thread threadToJoin = null;

            lock (syncRoot)
            {
                if (!isRunning && monitorThread == null)
                    return;

                isRunning = false;
                threadToJoin = monitorThread;
                monitorThread = null;
            }

            if (threadToJoin != null && threadToJoin.IsAlive)
                threadToJoin.Join(1500);

            if (BandwidthUpdated != null)
                BandwidthUpdated("Bandwidth: 0 KB/s");
        }

        private void MonitorLoop()
        {
            while (isRunning)
            {
                try
                {
                    long currentDownload = (long)downloadCounter.NextSample().RawValue;
                    long currentUpload = (long)uploadCounter.NextSample().RawValue;

                    long downloadSpeed = Math.Max(0, currentDownload - lastDownloadBytes) / 1024;
                    long uploadSpeed = Math.Max(0, currentUpload - lastUploadBytes) / 1024;

                    totalDownloadBytes += downloadSpeed;
                    totalUploadBytes += uploadSpeed;

                    lastDownloadBytes = currentDownload;
                    lastUploadBytes = currentUpload;

                    string status = "DL: " + FormatBytes(totalDownloadBytes) + " | UL: " + FormatBytes(totalUploadBytes) + " | Speed: " + downloadSpeed + "KB/s↓ " + uploadSpeed + "KB/s↑";

                    if (isRunning && BandwidthUpdated != null)
                        BandwidthUpdated(status);
                }
                catch
                {
                    if (isRunning && BandwidthUpdated != null)
                        BandwidthUpdated("Bandwidth: N/A");
                }

                for (int i = 0; i < 10 && isRunning; i++)
                    Thread.Sleep(100);
            }
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024) + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / 1024 / 1024) + " MB";
            return (bytes / 1024 / 1024 / 1024) + " GB";
        }

        public void Reset()
        {
            lock (syncRoot)
            {
                lastDownloadBytes = 0;
                lastUploadBytes = 0;
                totalDownloadBytes = 0;
                totalUploadBytes = 0;
            }
        }
    }
}
