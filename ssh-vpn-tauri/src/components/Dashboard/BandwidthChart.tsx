import { useTranslation } from 'react-i18next';
import { AreaChart, Area, XAxis, YAxis, ResponsiveContainer, Tooltip } from 'recharts';
import { Card, CardContent, CardHeader } from '../UI/Card';
import { useAppStore } from '../../store/appStore';

export default function BandwidthChart() {
  const { t } = useTranslation();
  const { bandwidth } = useAppStore();

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  };

  return (
    <Card>
      <CardHeader>
        <h3 className="text-lg font-semibold text-[var(--text-primary)]">{t('dashboard.bandwidth')}</h3>
      </CardHeader>
      <CardContent>
        <div className="h-48">
          {bandwidth.length > 0 ? (
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={bandwidth}>
                <defs>
                  <linearGradient id="downloadGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#22c55e" stopOpacity={0.3} />
                    <stop offset="95%" stopColor="#22c55e" stopOpacity={0} />
                  </linearGradient>
                  <linearGradient id="uploadGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3} />
                    <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <XAxis 
                  dataKey="timestamp" 
                  tick={false}
                  axisLine={{ stroke: 'var(--border)' }}
                />
                <YAxis 
                  tickFormatter={formatBytes}
                  tick={{ fill: 'var(--text-secondary)', fontSize: 12 }}
                  axisLine={{ stroke: 'var(--border)' }}
                />
                <Tooltip 
                  formatter={(value: number) => formatBytes(value)}
                  contentStyle={{ 
                    backgroundColor: 'var(--bg-secondary)', 
                    border: '1px solid var(--border)',
                    borderRadius: '8px',
                  }}
                  labelStyle={{ color: 'var(--text-primary)' }}
                />
                <Area
                  type="monotone"
                  dataKey="download_speed"
                  stroke="#22c55e"
                  fill="url(#downloadGradient)"
                  name={t('dashboard.download')}
                />
                <Area
                  type="monotone"
                  dataKey="upload_speed"
                  stroke="#3b82f6"
                  fill="url(#uploadGradient)"
                  name={t('dashboard.upload')}
                />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <div className="h-full flex items-center justify-center text-[var(--text-secondary)]">
              {t('dashboard.noServerSelected')}
            </div>
          )}
        </div>
        
        {/* Legend */}
        <div className="flex items-center justify-center gap-6 mt-4">
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-green-500" />
            <span className="text-sm text-[var(--text-secondary)]">{t('dashboard.download')}</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-blue-500" />
            <span className="text-sm text-[var(--text-secondary)]">{t('dashboard.upload')}</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}