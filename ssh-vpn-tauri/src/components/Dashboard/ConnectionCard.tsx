import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader } from '../UI/Card';
import { Button } from '../UI/Button';
import { useAppStore } from '../../store/appStore';

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

  return (
    <Card>
      <CardHeader>
        <h3 className="text-lg font-semibold text-[var(--text-primary)]">
          {isConnected ? t('app.connected') : t('app.notConnected')}
        </h3>
      </CardHeader>
      <CardContent className="space-y-4">
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