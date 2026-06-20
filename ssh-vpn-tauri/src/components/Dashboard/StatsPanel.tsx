import { useTranslation } from 'react-i18next';
import { Card, CardContent } from '../UI/Card';
import { useAppStore } from '../../store/appStore';

export default function StatsPanel() {
  const { t } = useTranslation();
  const { connectionStatus } = useAppStore();

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  };

  const formatUptime = (date?: string) => {
    if (!date) return '--:--:--';
    const start = new Date(date).getTime();
    const now = Date.now();
    const diff = Math.floor((now - start) / 1000);
    
    const hours = Math.floor(diff / 3600);
    const minutes = Math.floor((diff % 3600) / 60);
    const seconds = diff % 60;
    
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  };

  return (
    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
      <Card>
        <CardContent className="text-center">
          <p className="text-sm text-[var(--text-secondary)]">{t('dashboard.download')}</p>
          <p className="text-2xl font-bold text-[var(--success)] mt-1">
            {formatBytes(connectionStatus.bytes_received)}
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="text-center">
          <p className="text-sm text-[var(--text-secondary)]">{t('dashboard.upload')}</p>
          <p className="text-2xl font-bold text-[var(--accent)] mt-1">
            {formatBytes(connectionStatus.bytes_sent)}
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="text-center">
          <p className="text-sm text-[var(--text-secondary)]">{t('dashboard.uptime')}</p>
          <p className="text-2xl font-bold text-[var(--text-primary)] mt-1">
            {formatUptime(connectionStatus.connected_at)}
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="text-center">
          <p className="text-sm text-[var(--text-secondary)]">Port</p>
          <p className="text-2xl font-bold text-[var(--text-primary)] mt-1">
            {connectionStatus.local_port}
          </p>
        </CardContent>
      </Card>
    </div>
  );
}