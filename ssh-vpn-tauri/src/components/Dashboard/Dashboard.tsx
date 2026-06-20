import { useTranslation } from 'react-i18next';
import { Card, CardContent } from '../UI/Card';
import ConnectionCard from './ConnectionCard';
import BandwidthChart from './BandwidthChart';
import StatsPanel from './StatsPanel';

export default function Dashboard() {
  const { t } = useTranslation();

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold text-[var(--text-primary)]">{t('dashboard.title')}</h2>
      
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <ConnectionCard />
        <BandwidthChart />
      </div>
      
      <StatsPanel />
    </div>
  );
}