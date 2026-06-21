import React from 'react';
import ConnectionCard from './ConnectionCard';
import StatsPanel from './StatsPanel';
import BandwidthChart from './BandwidthChart';
import { useAppStore } from '../../store/appStore';
import DebugLogs from './DebugLogs';

export default function Dashboard() {
  const { connectionStatus } = useAppStore();

  return (
    <div className="space-y-6 max-w-6xl mx-auto pb-12">
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-1">
          <ConnectionCard />
        </div>
        <div className="lg:col-span-2">
          <StatsPanel />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 h-[400px]">
          <BandwidthChart />
        </div>
        <div className="lg:col-span-1 h-[400px]">
          <DebugLogs />
        </div>
      </div>
    </div>
  );
}
