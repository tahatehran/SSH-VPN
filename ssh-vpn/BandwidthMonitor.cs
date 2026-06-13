using System;
using System.Diagnostics;
using System.Threading;

namespace ssh_vpn
{
    public class BandwidthMonitor
    {
        private PerformanceCounter downloadCounter;
        private PerformanceCounter uploadCounter;
        private long lastDownloadBytes = 0;
        private long lastUploadBytes = 0;
        private long totalDownloadBytes = 0;
        private long totalUploadBytes = 0;

        public event Action<string> BandwidthUpdated;

        public BandwidthMonitor()
        {
            try
            {
                downloadCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", "_Total");
                uploadCounter = new PerformanceCounter("Network Interface", "Bytes Total/sec", "_Total");
            }
            catch
            {
                // Performance counters may not be available on all systems
            }
        }

        public void StartMonitoring()
        {
            if (downloadCounter == null) return;

            ThreadPool.QueueUserWorkItem((state) =>
            {
                while (true)
                {
                    try
                    {
                        long currentDownload = (long)downloadCounter.NextSample().RawValue;
                        long currentUpload = (long)uploadCounter.NextSample().RawValue;

                        long downloadSpeed = (currentDownload - lastDownloadBytes) / 1024; // KB/s
                        long uploadSpeed = (currentUpload - lastUploadBytes) / 1024; // KB/s

                        totalDownloadBytes += downloadSpeed;
                        totalUploadBytes += uploadSpeed;

                        lastDownloadBytes = currentDownload;
                        lastUploadBytes = currentUpload;

                        string status = $"DL: {FormatBytes(totalDownloadBytes)} | UL: {FormatBytes(totalUploadBytes)} | Speed: {downloadSpeed}KB/s↓ {uploadSpeed}KB/s↑";
                        BandwidthUpdated?.Invoke(status);
                    }
                    catch
                    {
                        BandwidthUpdated?.Invoke("Bandwidth: N/A");
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / 1024 / 1024} MB";
            return $"{bytes / 1024 / 1024 / 1024} GB";
        }

        public void Reset()
        {
            totalDownloadBytes = 0;
            totalUploadBytes = 0;
        }
    }
}