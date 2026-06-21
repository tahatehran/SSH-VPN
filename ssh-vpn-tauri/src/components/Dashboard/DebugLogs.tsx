import { useEffect, useRef, useState } from 'react';
import { useAppStore } from '../../store/appStore';
import { Card } from '../UI/Card';

export default function DebugLogs() {
  const { debugLogs, clearDebugLogs } = useAppStore();
  const scrollRef = useRef<HTMLDivElement>(null);
  const [autoScroll, setAutoScroll] = useState(true);

  useEffect(() => {
    if (autoScroll && scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [debugLogs, autoScroll]);

  const getLevelColor = (level: string) => {
    switch (level) {
      case 'Error': return 'text-red-500';
      case 'Warning': return 'text-yellow-500';
      case 'Debug': return 'text-blue-400';
      default: return 'text-green-400';
    }
  };

  const copyToClipboard = () => {
    const text = debugLogs.map(log => `[${new Date(log.timestamp).toLocaleString()}] [${log.level}] [${log.module}] ${log.message}`).join('\n');
    navigator.clipboard.writeText(text);
  };

  return (
    <Card className="h-full flex flex-col overflow-hidden">
      <div className="flex items-center justify-between mb-4 p-4 border-b border-[var(--border)]">
        <div className="flex items-center gap-3">
          <h3 className="text-lg font-semibold text-[var(--text-primary)]">System Logs</h3>
          <span className="px-2 py-0.5 rounded-full bg-[var(--bg-tertiary)] text-[10px] text-[var(--text-secondary)] font-bold uppercase tracking-tight">
            Engine v2
          </span>
        </div>
        <div className="flex gap-2">
          <button
            onClick={() => setAutoScroll(!autoScroll)}
            className={`text-[10px] px-2 py-1 rounded transition-colors ${autoScroll ? 'bg-[var(--accent)] text-white' : 'bg-[var(--bg-tertiary)] text-[var(--text-secondary)]'}`}
          >
            {autoScroll ? 'Auto' : 'Manual'}
          </button>
          <button
            onClick={copyToClipboard}
            className="text-[10px] px-2 py-1 bg-[var(--bg-tertiary)] hover:bg-[var(--border)] rounded text-[var(--text-secondary)] transition-colors"
          >
            Copy
          </button>
          <button
            onClick={clearDebugLogs}
            className="text-[10px] px-2 py-1 bg-red-500/10 hover:bg-red-500/20 rounded text-red-500 transition-colors font-bold"
          >
            Clear
          </button>
        </div>
      </div>
      <div
        ref={scrollRef}
        onScroll={(e) => {
          const target = e.currentTarget;
          const isAtBottom = target.scrollHeight - target.scrollTop <= target.clientHeight + 10;
          if (!isAtBottom && autoScroll) setAutoScroll(false);
          if (isAtBottom && !autoScroll) setAutoScroll(true);
        }}
        className="flex-1 overflow-y-auto bg-[#0a0a0a] p-4 font-mono text-[11px] space-y-1.5 scrollbar-thin shadow-inner"
      >
        {debugLogs.length === 0 ? (
          <div className="text-gray-600 italic">Waiting for events...</div>
        ) : (
          debugLogs.map((log, index) => (
            <div key={index} className="flex gap-2 leading-relaxed opacity-90 hover:opacity-100 transition-opacity">
              <span className="text-gray-500 shrink-0 select-none">[{new Date(log.timestamp).toLocaleTimeString()}]</span>
              <span className={`${getLevelColor(log.level)} font-bold shrink-0 min-w-[70px] select-none`}>[{log.level.toUpperCase()}]</span>
              <span className="text-purple-400 shrink-0 select-none opacity-80">[{log.module}]</span>
              <span className="text-gray-300 break-all">{log.message}</span>
            </div>
          ))
        )}
      </div>
    </Card>
  );
}
