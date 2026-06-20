import { useTranslation } from 'react-i18next';
import { useAppStore } from '../../store/appStore';

export default function StatusBar() {
  const { t } = useTranslation();
  const { connectionStatus } = useAppStore();

  const getStatusColor = () => {
    const state = connectionStatus.state;
    if (state === 'connected') return 'bg-[var(--success)]';
    if (state === 'connecting' || state === 'reconnecting') return 'bg-[var(--warning)]';
    return 'bg-[var(--text-secondary)]';
  };

  const getStatusText = () => {
    const state = connectionStatus.state;
    if (state === 'connected') return t('status.connected');
    if (state === 'connecting') return t('status.connecting');
    if (state === 'reconnecting') return t('status.reconnecting');
    if (typeof state === 'object' && 'error' in state) return t('status.error');
    return t('status.disconnected');
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  };

  return (
    <footer className="h-8 bg-[var(--bg-secondary)] border-t border-[var(--border)] flex items-center justify-between px-4 text-xs">
      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2">
          <div className={`w-2 h-2 rounded-full ${getStatusColor()}`} />
          <span className="text-[var(--text-secondary)]">{getStatusText()}</span>
        </div>
        {connectionStatus.state === 'connected' && connectionStatus.server && (
          <span className="text-[var(--text-secondary)]">
            {connectionStatus.server.name}
          </span>
        )}
      </div>
      
      <div className="flex items-center gap-4 text-[var(--text-secondary)]">
        {connectionStatus.state === 'connected' && (
          <>
            <span>↑ {formatBytes(connectionStatus.bytes_sent)}</span>
            <span>↓ {formatBytes(connectionStatus.bytes_received)}</span>
            <span>Port: {connectionStatus.local_port}</span>
          </>
        )}
      </div>
    </footer>
  );
}