import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { motion } from 'framer-motion';
import { Card, CardContent } from '../UI/Card';
import { useAppStore } from '../../store/appStore';
import type { ServerInfo } from '../../types';

interface ServerCardProps {
  server: ServerInfo;
  onEdit: () => void;
  onDelete: () => void;
  onSetActive: () => void;
}

export default function ServerCard({ server, onEdit, onDelete, onSetActive }: ServerCardProps) {
  const { t } = useTranslation();
  const { testLatency } = useAppStore();
  const [ping, setPing] = useState<number | null>(null);

  useEffect(() => {
    const testPing = async () => {
      try {
        const latency = await testLatency(server.host, server.port);
        setPing(latency);
      } catch {
        setPing(null);
      }
    };
    testPing();
  }, [server.host, server.port, testLatency]);

  return (
    <Card hover onClick={onSetActive}>
      <CardContent className="space-y-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className={`w-3 h-3 rounded-full ${server.is_active ? 'bg-[var(--success)]' : 'bg-[var(--text-secondary)]'}`} />
            <div>
              <h3 className="font-medium text-[var(--text-primary)]">{server.name}</h3>
              <p className="text-sm text-[var(--text-secondary)]">{server.host}:{server.port}</p>
            </div>
          </div>
          
          {server.country && (
            <span className="text-2xl">{getCountryFlag(server.country)}</span>
          )}
        </div>

        <div className="flex items-center justify-between text-sm">
          <span className="text-[var(--text-secondary)]">{server.username}</span>
          {ping !== null ? (
            <motion.span
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              className={`font-medium ${
                ping < 100 ? 'text-[var(--success)]' :
                ping < 200 ? 'text-[var(--warning)]' :
                'text-[var(--error)]'
              }`}
            >
              {ping}ms
            </motion.span>
          ) : (
            <span className="text-[var(--text-secondary)]">--</span>
          )}
        </div>

        <div className="flex gap-2 pt-2 border-t border-[var(--border)]">
          <button
            onClick={(e) => { e.stopPropagation(); onEdit(); }}
            className="flex-1 px-3 py-1.5 text-sm rounded-lg bg-[var(--bg-tertiary)] hover:bg-[var(--border)] transition-colors"
          >
            {t('servers.editServer')}
          </button>
          <button
            onClick={(e) => { e.stopPropagation(); onDelete(); }}
            className="px-3 py-1.5 text-sm rounded-lg bg-[var(--error)]/10 text-[var(--error)] hover:bg-[var(--error)]/20 transition-colors"
          >
            {t('servers.deleteServer')}
          </button>
        </div>
      </CardContent>
    </Card>
  );
}

function getCountryFlag(countryCode: string): string {
  const codePoints = countryCode
    .toUpperCase()
    .split('')
    .map(char => 127397 + char.charCodeAt(0));
  return String.fromCodePoint(...codePoints);
}