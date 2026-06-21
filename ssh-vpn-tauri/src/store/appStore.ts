import { create } from 'zustand';
import { invoke } from '@tauri-apps/api/core';
import type { ServerInfo, ServerConfig, ConnectionStatus, AppSettings, BandwidthStats } from '../types';

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
  
  // UI State
  theme: 'light' | 'dark' | 'system';
  language: 'en' | 'fa';
  activeView: 'dashboard' | 'servers' | 'settings';
  
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
  setActiveView: (view: 'dashboard' | 'servers' | 'settings') => void;
  addBandwidthStats: (stats: BandwidthStats) => void;
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

// Polling interval reference
let pollingInterval: ReturnType<typeof setInterval> | null = null;

export const useAppStore = create<AppState>((set, get) => ({
  // Initial state
  connectionStatus: defaultConnectionStatus,
  isConnecting: false,
  servers: [],
  activeServerId: null,
  settings: defaultSettings,
  bandwidth: [],
  theme: 'system',
  language: 'en',
  activeView: 'dashboard',

  // Polling
  startPolling: () => {
    if (pollingInterval) return;
    
    pollingInterval = setInterval(async () => {
      try {
        // Fetch connection status
        const status = await invoke<ConnectionStatus>('get_status');
        set({ connectionStatus: status });
        
        // If connected, fetch bandwidth stats
        if (status.state === 'connected') {
          const stats = await invoke<BandwidthStats>('get_bandwidth');
          get().addBandwidthStats(stats);
        }
      } catch (error) {
        // Silently handle polling errors
      }
    }, 1000); // Poll every second
  },

  stopPolling: () => {
    if (pollingInterval) {
      clearInterval(pollingInterval);
      pollingInterval = null;
    }
  },

  // Actions
  connect: async (config: ServerConfig) => {
    set({ isConnecting: true });
    try {
      const status = await invoke<ConnectionStatus>('connect', { config });
      
      // Set system proxy only if enabled in settings
      const currentSettings = get().settings;
      if (currentSettings.system_proxy) {
        const socksPort = status.local_port || 9000;
        await invoke('set_system_proxy', { port: socksPort }).catch(err => {
          console.warn('Failed to set system proxy:', err);
        });
      }
      

      // Handle Global VPN (TUN)
      if (currentSettings.global_vpn) {
        await invoke('start_vpn').catch(err => {
          console.warn('Failed to start Global VPN:', err);
        });
      }

      set({ connectionStatus: status, isConnecting: false });
      // Start polling after successful connection
      get().startPolling();
    } catch (error) {
      set({ isConnecting: false });
      throw error;
    }
  },

  disconnect: async () => {
    try {
      // Unset system proxy first
      await invoke('unset_system_proxy').catch(err => {
        console.warn('Failed to unset system proxy:', err);
      });
      

      // Stop Global VPN
      await invoke('stop_vpn').catch(err => {
        console.warn('Failed to stop Global VPN:', err);
      });

      await invoke('disconnect');
      get().stopPolling();
      set({ connectionStatus: defaultConnectionStatus, bandwidth: [] });
    } catch (error) {
      throw error;
    }
  },

  fetchStatus: async () => {
    try {
      const status = await invoke<ConnectionStatus>('get_status');
      set({ connectionStatus: status });
    } catch (error) {
      console.error('Failed to fetch status:', error);
    }
  },

  fetchServers: async () => {
    try {
      const servers = await invoke<ServerInfo[]>('get_servers');
      const activeServer = servers.find(s => s.is_active);
      set({ 
        servers, 
        activeServerId: activeServer?.id || null 
      });
    } catch (error) {
      console.error('Failed to fetch servers:', error);
    }
  },

  addServer: async (server) => {
    try {
      const newServer: ServerInfo = {
        ...server,
        id: '', // Will be generated by backend
        created_at: new Date().toISOString(),
      } as ServerInfo;
      await invoke('add_server', { server: newServer });
      await get().fetchServers();
    } catch (error) {
      throw error;
    }
  },

  updateServer: async (server) => {
    try {
      await invoke('update_server', { server });
      await get().fetchServers();
    } catch (error) {
      throw error;
    }
  },

  deleteServer: async (id) => {
    try {
      await invoke('delete_server', { id });
      await get().fetchServers();
    } catch (error) {
      throw error;
    }
  },

  setActiveServer: async (id) => {
    try {
      await invoke('set_active_server', { id });
      set({ activeServerId: id });
      await get().fetchServers();
    } catch (error) {
      throw error;
    }
  },

  fetchSettings: async () => {
    try {
      const settings = await invoke<AppSettings>('get_settings');
      set({ 
        settings,
        theme: settings.theme as 'light' | 'dark' | 'system',
        language: settings.language as 'en' | 'fa',
      });
    } catch (error) {
      console.error('Failed to fetch settings:', error);
    }
  },

  saveSettings: async (settings) => {
    try {
      await invoke('save_settings', { settings });
      set({ 
        settings,
        theme: settings.theme as 'light' | 'dark' | 'system',
        language: settings.language as 'en' | 'fa',
      });
    } catch (error) {
      throw error;
    }
  },

  testLatency: async (host, port) => {
    try {
      return await invoke<number>('test_latency', { host, port });
    } catch (error) {
      throw error;
    }
  },

  setTheme: (theme) => {
    set({ theme });
    // Apply theme to document
    const root = document.documentElement;
    if (theme === 'dark') {
      root.setAttribute('data-theme', 'dark');
    } else if (theme === 'light') {
      root.removeAttribute('data-theme');
    } else {
      // System preference
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      if (prefersDark) {
        root.setAttribute('data-theme', 'dark');
      } else {
        root.removeAttribute('data-theme');
      }
    }
  },

  setLanguage: (language) => {
    set({ language });
  },

  setActiveView: (view) => {
    set({ activeView: view });
  },

  addBandwidthStats: (stats) => {
    set((state) => ({
      bandwidth: [...state.bandwidth.slice(-59), stats], // Keep last 60 data points
    }));
  },
}));