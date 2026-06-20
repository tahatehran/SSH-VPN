import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader } from '../UI/Card';
import { Button } from '../UI/Button';
import { useAppStore } from '../../store/appStore';
import { invoke } from '@tauri-apps/api/core';

// Check IP Button Component
function CheckIPButton() {
  const [checking, setChecking] = useState(false);
  const [publicIp, setPublicIp] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const checkIP = async () => {
    setChecking(true);
    setError(null);
    setPublicIp(null);
    try {
      const ip = await invoke<string>('get_public_ip');
      setPublicIp(ip);
    } catch (e) {
      setError('Failed to check IP');
    }
    setChecking(false);
  };

  return (
    <div className="mt-3">
      <button
        onClick={checkIP}
        disabled={checking}
        className="w-full px-3 py-2 bg-[var(--accent)] hover:bg-[var(--accent)]/80 
                   text-white rounded-lg transition-colors disabled:opacity-50"
      >
        {checking ? 'Checking IP...' : '🌐 Check My IP'}
      </button>
      
      {publicIp && (
        <div className="mt-2 p-2 bg-[var(--success)]/20 rounded border border-[var(--success)] text-center">
          <span className="text-[var(--success)] font-bold">Your IP: {publicIp}</span>
        </div>
      )}
      
      {error && (
        <div className="mt-2 p-2 bg-[var(--error)]/20 rounded border border-[var(--error)] text-center">
          <span className="text-[var(--error)]">{error}</span>
        </div>
      )}
    </div>
  );
}

export default function ConnectionCard() {
  const { t } = useTranslation();
  const { 
    connectionStatus, 
    isConnecting, 
    connect, 
    disconnect, 
    servers, 
    activeServerId 
  } = useAppStore();
  
  const [selectedServerId, setSelectedServerId] = useState<string | null>(activeServerId);
  const [showDebug, setShowDebug] = useState(false);

  const isConnected = connectionStatus.state === 'connected';
  const selectedServer = servers.find(s => s.id === selectedServerId);

  const handleConnect = async () => {
    if (!selectedServer) return;
    
    try {
      await connect({
        host: selectedServer.host,
        port: selectedServer.port,
        username: selectedServer.username,
        password: selectedServer.password,
      });
    } catch (error) {
      console.error('Connection failed:', error);
    }
  };

  const handleDisconnect = async () => {
    try {
      await disconnect();
    } catch (error) {
      console.error('Disconnect failed:', error);
    }
  };

  const getButtonText = () => {
    if (isConnecting) return t('app.connecting');
    if (isConnected) return t('app.disconnect');
    return t('app.connect');
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const getConnectionDuration = () => {
    if (!connectionStatus.connected_at) return 'N/A';
    const connected = new Date(connectionStatus.connected_at).getTime();
    const now = Date.now();
    const seconds = Math.floor((now - connected) / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    if (hours > 0) return `${hours}h ${minutes % 60}m`;
    if (minutes > 0) return `${minutes}m ${seconds % 60}s`;
    return `${seconds}s`;
  };

  // Check if traffic is actually flowing
  const isTrafficFlowing = connectionStatus.bytes_sent > 0 || connectionStatus.bytes_received > 0;

  return (
    <Card>
      <CardHeader className="flex-row justify-between items-center">
        <h3 className="text-lg font-semibold text-[var(--text-primary)]">
          {isConnected ? t('app.connected') : t('app.notConnected')}
        </h3>
        <button
          onClick={() => setShowDebug(!showDebug)}
          className={`p-2 rounded-lg transition-colors ${
            showDebug 
              ? 'bg-[var(--accent)] text-white' 
              : 'bg-[var(--bg-tertiary)] text-[var(--text-secondary)] hover:bg-[var(--border)]'
          }`}
          title="Debug Info"
        >
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} 
              d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </button>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Debug Panel */}
        {showDebug && (
          <div className="p-3 bg-[var(--bg-tertiary)] rounded-lg text-xs font-mono space-y-2">
            <div className="font-bold text-[var(--text-primary)] mb-2">🔧 Debug Info</div>
            <div className="grid grid-cols-2 gap-x-4 gap-y-1">
              <span className="text-[var(--text-secondary)]">Status:</span>
              <span className={`font-bold ${
                isConnected ? 'text-[var(--success)]' : 'text-[var(--error)]'
              }`}>
                {isConnected ? 'CONNECTED ✓' : 
                 typeof connectionStatus.state === 'string' 
                   ? connectionStatus.state 
                   : 'ERROR'}
              </span>
              
              <span className="text-[var(--text-secondary)]">SOCKS5 Port:</span>
              <span className="text-[var(--text-primary)]">{connectionStatus.local_port}</span>
              
              <span className="text-[var(--text-secondary)]">Server:</span>
              <span className="text-[var(--text-primary)]">
                {connectionStatus.server?.host || 'N/A'}
              </span>
              
              <span className="text-[var(--text-secondary)]">Duration:</span>
              <span className="text-[var(--text-primary)]">{getConnectionDuration()}</span>
              
              <span className="text-[var(--text-secondary)]">Bytes Sent:</span>
              <span className={isTrafficFlowing ? 'text-[var(--success)]' : 'text-[var(--text-secondary)]'}>
                {formatBytes(connectionStatus.bytes_sent)}
              </span>
              
              <span className="text-[var(--text-secondary)]">Bytes Recv:</span>
              <span className={isTrafficFlowing ? 'text-[var(--success)]' : 'text-[var(--text-secondary)]'}>
                {formatBytes(connectionStatus.bytes_received)}
              </span>
            </div>
            
            {/* Traffic Status */}
            {isConnected && !isTrafficFlowing && (
              <div className="mt-2 p-2 bg-[var(--warning)]/20 rounded border border-[var(--warning)] text-[var(--warning)]">
                ⚠️ No traffic detected! Make sure your browser is using the proxy.
              </div>
            )}
            
            {isConnected && isTrafficFlowing && (
              <div className="mt-2 p-2 bg-[var(--success)]/20 rounded border border-[var(--success)] text-[var(--success)]">
                ✓ Traffic flowing through SOCKS5 proxy!
              </div>
            )}
            
            {isConnected && (
              <div className="mt-3 p-2 bg-[var(--bg-secondary)] rounded border border-[var(--border)]">
                <div className="text-[var(--text-secondary)] mb-1 font-bold">📋 Browser Proxy Settings:</div>
                <div className="text-[var(--text-primary)]">
                  Host: <span className="font-bold text-[var(--accent)]">127.0.0.1</span>
                </div>
                <div className="text-[var(--text-primary)]">
                  Port: <span className="font-bold text-[var(--accent)]">{connectionStatus.local_port}</span>
                </div>
                <div className="text-[var(--text-secondary)] mt-1">Protocol: SOCKS5</div>
              </div>
            )}
            
            {/* Check IP Button */}
            {isConnected && (
              <CheckIPButton />
            )}
            
            {!isConnected && (
              <div className="mt-2 text-[var(--warning)]">
                ⚠️ Connect to a server to start the SOCKS5 proxy
              </div>
            )}
          </div>
        )}

        {/* Server Selector */}
        <div>
          <label className="block text-sm font-medium text-[var(--text-secondary)] mb-2">
            {t('dashboard.servers')}
          </label>
          <select
            value={selectedServerId || ''}
            onChange={(e) => setSelectedServerId(e.target.value || null)}
            disabled={isConnected || isConnecting}
            className="w-full px-3 py-2 bg-[var(--bg-tertiary)] border border-[var(--border)] rounded-lg 
                       text-[var(--text-primary)] focus:outline-none focus:ring-2 focus:ring-[var(--accent)]
                       disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <option value="">{t('dashboard.selectServer')}</option>
            {servers.map((server) => (
              <option key={server.id} value={server.id}>
                {server.name} ({server.host})
              </option>
            ))}
          </select>
        </div>

        {/* Status Indicator */}
        <div className="flex items-center gap-3">
          <motion.div
            animate={{ 
              scale: isConnected ? [1, 1.1, 1] : isConnecting ? [1, 0.9, 1] : 1,
              opacity: isConnected || isConnecting ? 1 : 0.5
            }}
            transition={{ repeat: Infinity, duration: 2 }}
            className={`w-3 h-3 rounded-full ${
              isConnected ? 'bg-[var(--success)]' : 
              isConnecting ? 'bg-[var(--warning)]' : 
              'bg-[var(--text-secondary)]'
            }`}
          />
          <span className="text-sm text-[var(--text-secondary)]">
            {isConnected ? selectedServer?.name : 
             isConnecting ? t('app.connecting') : 
             t('dashboard.noServerSelected')}
          </span>
        </div>

        {/* Connect Button */}
        <Button
          variant={isConnected ? 'danger' : 'primary'}
          size="lg"
          className="w-full"
          onClick={isConnected ? handleDisconnect : handleConnect}
          disabled={!selectedServer && !isConnected}
          isLoading={isConnecting}
        >
          {getButtonText()}
        </Button>
      </CardContent>
    </Card>
  );
}