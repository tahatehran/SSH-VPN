import { create } from 'zustand';
import { invoke } from '@tauri-apps/api/core';
import type { ServerInfo, ServerConfig, ConnectionStatus, AppSettings, BandwidthStats } from '../types';

export interface DebugLog {
  timestamp: string;
  level: 'Info' | 'Warning' | 'Error' | 'Debug';
  module: string;
  message: string;
}

interface AppState {
  // Connection
  connectionStatus: ConnectionStatus;
  isConnecting: boolean;
  
  // Servers
  servers: ServerInfo[];
  activeServerId: string | null;
  
  // Settings
  settings: AppSettings;
  
  // Bandwidth
  bandwidth: BandwidthStats[];
  
  // Debug Logs
  debugLogs: DebugLog[];

  // UI State
  theme: 'light' | 'dark' | 'system';
  language: 'en' | 'fa';
  activeView: 'dashboard' | 'servers' | 'settings' | 'logs';
  
  // Polling
  startPolling: () => void;
  stopPolling: () => void;
  
  // Actions
  connect: (config: ServerConfig) => Promise<void>;
  disconnect: () => Promise<void>;
  fetchStatus: () => Promise<void>;
  fetchServers: () => Promise<void>;
  addServer: (server: Omit<ServerInfo, 'id' | 'created_at'>) => Promise<void>;
  updateServer: (server: ServerInfo) => Promise<void>;
  deleteServer: (id: string) => Promise<void>;
  setActiveServer: (id: string) => Promise<void>;
  fetchSettings: () => Promise<void>;
  saveSettings: (settings: AppSettings) => Promise<void>;
  testLatency: (host: string, port: number) => Promise<number>;
  setTheme: (theme: 'light' | 'dark' | 'system') => void;
  setLanguage: (language: 'en' | 'fa') => void;
  setActiveView: (view: 'dashboard' | 'servers' | 'settings' | 'logs') => void;
  addBandwidthStats: (stats: BandwidthStats) => void;
  fetchDebugLogs: () => Promise<void>;
  clearDebugLogs: () => Promise<void>;
}

const defaultSettings: AppSettings = {
  language: 'en',
  theme: 'system',
  auto_reconnect: true,
  kill_switch: false,
  dns_protection: false,
  custom_dns: ['1.1.1.1', '8.8.8.8'],
  check_interval_sec: 30,
  max_ping_ms: 200,
  socks_port: 9000,
  system_proxy: true,
  global_vpn: true,
};

const defaultConnectionStatus: ConnectionStatus = {
  state: 'disconnected',
  local_port: 9000,
  bytes_sent: 0,
  bytes_received: 0,
};

let pollingInterval: ReturnType<typeof setInterval> | null = null;

export const useAppStore = create<AppState>((set, get) => ({
  connectionStatus: defaultConnectionStatus,
  isConnecting: false,
  servers: [],
  activeServerId: null,
  settings: defaultSettings,
  bandwidth: [],
  debugLogs: [],
  theme: 'system',
  language: 'en',
  activeView: 'dashboard',

  startPolling: () => {
    if (pollingInterval) return;
    pollingInterval = setInterval(async () => {
      try {
        const status = await invoke<ConnectionStatus>('get_status');
        set({ connectionStatus: status });
        
        if (status.state === 'connected') {
          const stats = await invoke<BandwidthStats>('get_bandwidth');
          get().addBandwidthStats(stats);
        }

        // Also poll debug logs
        await get().fetchDebugLogs();
      } catch (error) {}
    }, 1000);
  },

  stopPolling: () => {
    if (pollingInterval) {
      clearInterval(pollingInterval);
      pollingInterval = null;
    }
  },

  connect: async (config: ServerConfig) => {
    set({ isConnecting: true });
    try {
      const status = await invoke<ConnectionStatus>('connect', { config });
      const currentSettings = get().settings;

      if (currentSettings.system_proxy) {
        const socksPort = status.local_port || 9000;
        await invoke('set_system_proxy', { port: socksPort }).catch(err => console.warn(err));
      }

      if (currentSettings.global_vpn) {
        await invoke('start_vpn').catch(err => console.warn(err));
      }

      set({ connectionStatus: status, isConnecting: false });
      get().startPolling();
    } catch (error) {
      set({ isConnecting: false });
      throw error;
    }
  },

  disconnect: async () => {
    try {
      await invoke('stop_vpn').catch(err => console.warn(err));
      await invoke('unset_system_proxy').catch(err => console.warn(err));
      await invoke('disconnect');
      get().stopPolling();
      set({ connectionStatus: defaultConnectionStatus, bandwidth: [] });
    } catch (error) {
      throw error;
    }
  },

  fetchStatus: async () => {
    const status = await invoke<ConnectionStatus>('get_status');
    set({ connectionStatus: status });
  },

  fetchServers: async () => {
    const servers = await invoke<ServerInfo[]>('get_servers');
    const activeServer = servers.find(s => s.is_active);
    set({ servers, activeServerId: activeServer?.id || null });
  },

  addServer: async (server) => {
    await invoke('add_server', { server });
    await get().fetchServers();
  },

  updateServer: async (server) => {
    await invoke('update_server', { server });
    await get().fetchServers();
  },

  deleteServer: async (id) => {
    await invoke('delete_server', { id });
    await get().fetchServers();
  },

  setActiveServer: async (id) => {
    await invoke('set_active_server', { id });
    set({ activeServerId: id });
    await get().fetchServers();
  },

  fetchSettings: async () => {
    const settings = await invoke<AppSettings>('get_settings');
    set({
      settings,
      theme: settings.theme as any,
      language: settings.language as any,
    });
  },

  saveSettings: async (settings) => {
    await invoke('save_settings', { settings });
    set({
      settings,
      theme: settings.theme as any,
      language: settings.language as any,
    });
  },

  testLatency: async (host, port) => {
    return await invoke<number>('test_latency', { host, port });
  },

  fetchDebugLogs: async () => {
    try {
      const logs = await invoke<DebugLog[]>('get_debug_logs');
      set({ debugLogs: logs });
    } catch (error) {}
  },

  clearDebugLogs: async () => {
    try {
      await invoke('clear_debug_logs');
      set({ debugLogs: [] });
    } catch (error) {}
  },

  setTheme: (theme) => {
    set({ theme });
    const root = document.documentElement;
    if (theme === 'dark') root.setAttribute('data-theme', 'dark');
    else if (theme === 'light') root.removeAttribute('data-theme');
    else if (window.matchMedia('(prefers-color-scheme: dark)').matches) root.setAttribute('data-theme', 'dark');
    else root.removeAttribute('data-theme');
  },

  setLanguage: (language) => set({ language }),
  setActiveView: (view) => set({ activeView: view }),
  addBandwidthStats: (stats) => {
    set((state) => ({
      bandwidth: [...state.bandwidth.slice(-59), stats],
    }));
  },
}));
