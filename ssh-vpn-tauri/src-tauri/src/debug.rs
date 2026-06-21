use serde::{Deserialize, Serialize};
use std::sync::Arc;
use tokio::sync::Mutex;
use chrono::{DateTime, Utc};
use tracing::info;

#[derive(Debug, Clone, Serialize, Deserialize)]
pub enum LogLevel {
    Info,
    Warning,
    Error,
    Debug,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DebugLog {
    pub timestamp: DateTime<Utc>,
    pub level: LogLevel,
    pub module: String,
    pub message: String,
}

pub struct DebugManager {
    logs: Arc<Mutex<Vec<DebugLog>>>,
}

impl DebugManager {
    pub fn new() -> Self {
        Self {
            logs: Arc::new(Mutex::new(Vec::with_capacity(1000))),
        }
    }

    pub async fn log(&self, level: LogLevel, module: &str, message: &str) {
        let log = DebugLog {
            timestamp: Utc::now(),
            level,
            module: module.to_string(),
            message: message.to_string(),
        };

        // Log to console for development
        info!("[{}] [{}] {}", log.timestamp.format("%H:%M:%S"), log.module, log.message);

        let mut logs = self.logs.lock().await;
        if logs.len() >= 1000 {
            logs.remove(0);
        }
        logs.push(log);
    }

    pub async fn get_logs(&self) -> Vec<DebugLog> {
        self.logs.lock().await.clone()
    }

    pub async fn clear_logs(&self) {
        self.logs.lock().await.clear();
    }
}

impl Default for DebugManager {
    fn default() -> Self {
        Self::new()
    }
}
