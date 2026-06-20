use serde::{Deserialize, Serialize};
use std::sync::atomic::{AtomicU64, Ordering};
use std::sync::Arc;
use tokio::sync::broadcast;

/// Bandwidth statistics
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct BandwidthStats {
    pub bytes_sent: u64,
    pub bytes_received: u64,
    pub upload_speed: f64,
    pub download_speed: f64,
    pub timestamp: i64,
}

impl Default for BandwidthStats {
    fn default() -> Self {
        Self {
            bytes_sent: 0,
            bytes_received: 0,
            upload_speed: 0.0,
            download_speed: 0.0,
            timestamp: chrono::Utc::now().timestamp(),
        }
    }
}

/// Bandwidth monitor for tracking network usage
pub struct BandwidthMonitor {
    bytes_sent: Arc<AtomicU64>,
    bytes_received: Arc<AtomicU64>,
    last_sent: Arc<AtomicU64>,
    last_received: Arc<AtomicU64>,
    stats_tx: broadcast::Sender<BandwidthStats>,
}

impl BandwidthMonitor {
    pub fn new() -> Self {
        let (stats_tx, _) = broadcast::channel(100);
        Self {
            bytes_sent: Arc::new(AtomicU64::new(0)),
            bytes_received: Arc::new(AtomicU64::new(0)),
            last_sent: Arc::new(AtomicU64::new(0)),
            last_received: Arc::new(AtomicU64::new(0)),
            stats_tx,
        }
    }

    /// Add sent bytes
    pub fn add_sent(&self, bytes: u64) {
        self.bytes_sent.fetch_add(bytes, Ordering::SeqCst);
    }

    /// Add received bytes
    pub fn add_received(&self, bytes: u64) {
        self.bytes_received.fetch_add(bytes, Ordering::SeqCst);
    }

    /// Get current bandwidth statistics
    pub fn get_stats(&self) -> BandwidthStats {
        let sent = self.bytes_sent.load(Ordering::SeqCst);
        let received = self.bytes_received.load(Ordering::SeqCst);
        
        // Calculate speed (bytes per second)
        let sent_delta = sent.saturating_sub(self.last_sent.load(Ordering::SeqCst));
        let received_delta = received.saturating_sub(self.last_received.load(Ordering::SeqCst));
        
        // Update last values
        self.last_sent.store(sent, Ordering::SeqCst);
        self.last_received.store(received, Ordering::SeqCst);
        
        BandwidthStats {
            bytes_sent: sent,
            bytes_received: received,
            upload_speed: sent_delta as f64,      // bytes per interval
            download_speed: received_delta as f64,
            timestamp: chrono::Utc::now().timestamp(),
        }
    }

    /// Subscribe to bandwidth updates
    pub fn subscribe(&self) -> broadcast::Receiver<BandwidthStats> {
        self.stats_tx.subscribe()
    }

    /// Reset counters
    pub fn reset(&self) {
        self.bytes_sent.store(0, Ordering::SeqCst);
        self.bytes_received.store(0, Ordering::SeqCst);
        self.last_sent.store(0, Ordering::SeqCst);
        self.last_received.store(0, Ordering::SeqCst);
    }
}

impl Default for BandwidthMonitor {
    fn default() -> Self {
        Self::new()
    }
}