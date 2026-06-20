export interface ServerInfo {
  id: string;
  name: string;
  name_fa?: string;
  host: string;
  port: number;
  username: string;
  password?: string;
  private_key_path?: string;
  country?: string;
  city?: string;
  priority: number;
  is_active: boolean;
  created_at: string;
  last_used?: string;
}

export interface ServerConfig {
  host: string;
  port: number;
  username: string;
  password?: string;
  private_key_path?: string;
}

export interface ConnectionStatus {
  state: ConnectionState;
  connected_at?: string;
  server?: ServerInfo;
  local_port: number;
  bytes_sent: number;
  bytes_received: number;
}

export type ConnectionState = 
  | 'disconnected'
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | { error: string };

export interface AppSettings {
  language: string;
  theme: string;
  auto_reconnect: boolean;
  kill_switch: boolean;
  dns_protection: boolean;
  custom_dns: string[];
  check_interval_sec: number;
  max_ping_ms: number;
}

export interface BandwidthStats {
  bytes_sent: number;
  bytes_received: number;
  upload_speed: number;
  download_speed: number;
  timestamp: number;
}