import React, { useEffect, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useDashboardStore } from '../store/dashboardStore';

interface ActivityEvent {
  id: string;
  type: string;
  actor: string;
  actorAvatar: string;
  description: string;
  timestamp: string;
  relatedItemId: string | null;
}

interface Dependency {
  id: string;
  title: string;
  status: string;
}

interface ReportDetail {
  id: string;
  type: string;
  title: string;
  description: string;
  owner: string;
  status: string;
  priority: string;
  estimate: number | null;
  remainingWork: number | null;
  dependencies: Dependency[];
  recentActivity: ActivityEvent[];
  metadata: Record<string, string>;
}

const STATUS_COLORS: Record<string, string> = {
  done: '#00ff88',
  'in-progress': '#00aaff',
  blocked: '#ff4444',
  'not-started': '#666666',
  'at-risk': '#ff8800',
  open: '#ff4444',
  mitigated: '#00aaff',
  closed: '#00ff88',
};

const PRIORITY_COLORS: Record<string, string> = {
  critical: '#ff4444',
  high: '#ff8800',
  medium: '#ffaa00',
  low: '#666666',
};

function getInitials(name: string): string {
  return name
    .split(' ')
    .map((w) => w[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

function hashColor(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  const hue = Math.abs(hash) % 360;
  return `hsl(${hue}, 60%, 45%)`;
}

function relativeTime(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.floor(hrs / 24);
  if (days === 1) return 'yesterday';
  return `${days}d ago`;
}

function formatType(type: string): string {
  return type.charAt(0).toUpperCase() + type.slice(1);
}

const EVENT_TYPE_LABELS: Record<string, { label: string; color: string }> = {
  'pr-completed': { label: 'PR', color: '#8b5cf6' },
  'task-completed': { label: 'Done', color: '#00ff88' },
  comment: { label: 'Comment', color: '#00aaff' },
  deployment: { label: 'Deploy', color: '#ff8800' },
  review: { label: 'Review', color: '#00d4ff' },
};

/** Fetches report detail with caching and AbortController cleanup. */
function useReportDetail(id: string | null) {
  const [data, setData] = useState<ReportDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fetchedId, setFetchedId] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      setData(null);
      setError(null);
      setLoading(false);
      setFetchedId(null);
      return undefined;
    }

    if (id === fetchedId && data) return undefined;

    const controller = new AbortController();
    setLoading(true);
    setError(null);

    fetch(`/api/report/${encodeURIComponent(id)}`, { signal: controller.signal })
      .then((res) => {
        if (!res.ok) throw new Error(res.status === 404 ? 'Not found' : `Error ${res.status}`);
        return res.json();
      })
      .then((json: ReportDetail) => {
        setData(json);
        setFetchedId(id);
        setLoading(false);
      })
      .catch((err: Error) => {
        if (err.name === 'AbortError') return;
        setError(err.message);
        setLoading(false);
      });

    return () => {
      controller.abort();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  return { data, loading, error };
}

function StatusBadge({ status }: { status: string }) {
  const color = STATUS_COLORS[status] ?? '#666666';
  return (
    <span
      className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
      style={{ backgroundColor: `${color}20`, color, border: `1px solid ${color}40` }}
      {formatType(status.replace('-', ' '))}
    </span>
  );
}

function PriorityBadge({ priority }: { priority: string }) {
  const color = PRIORITY_COLORS[priority] ?? '#666666';
  return (
    <span
      className="inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium"
      style={{ backgroundColor: `${color}20`, color, border: `1px solid ${color}40` }}
      {formatType(priority)}
    </span>
  );
}

function AvatarCircle({ name, size = 32 }: { name: string; size?: number }) {
  return (
    <div
      className="flex items-center justify-center rounded-full text-white font-bold shrink-0"
      style={{
        width: size,
        height: size,
        fontSize: size * 0.4,
        backgroundColor: hashColor(name),
      }}
      {getInitials(name)}
    </div>
  );
}

function LoadingSpinner() {
  return (
    <div className="flex items-center justify-center h-48">
      <div className="w-8 h-8 rounded-full border-2 border-cyan-400/30 border-t-cyan-400 animate-spin" />
    </div>
  );
}

function ErrorState({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center justify-center h-48 text-center px-4">
      <div className="text-red-400 text-sm font-medium mb-2">Failed to load details</div>
      <div className="text-gray-500 text-xs">{message}</div>
    </div>
  );
}

function MetricCard({ label, value, unit }: { label: string; value: number | null; unit: string }) {
  return (
    <div className="rounded-lg bg-white/5 border border-white/5 p-3">
      <div className="text-[10px] uppercase tracking-wider text-gray-500 mb-1">{label}</div>
      <div className="text-lg font-bold text-white">
        {value !== null ? value : '\u2014'}
        {value !== null && <span className="text-xs font-normal text-gray-500 ml-1">{unit}</span>}
      </div>
    </div>
  );
}

function SectionHeader({ children }: { children: React.ReactNode }) {
  return (
    <h3 className="text-xs uppercase tracking-widest text-gray-500 mb-2 font-medium">{children}</h3>
  );
}

function CloseIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
      <line x1="4" y1="4" x2="12" y2="12" />
      <line x1="12" y1="4" x2="4" y2="12" />
    </svg>
  );
}

function PanelContent({ data }: { data: ReportDetail }) {
  return (
    <div className="space-y-6">
      <div>
        <div className="text-[10px] uppercase tracking-widest text-gray-500 mb-1">
          {formatType(data.type)}
        </div>
        <h2 className="text-xl font-bold text-white leading-tight">{data.title}</h2>
      </div>

      <div className="flex items-center gap-3">
        <AvatarCircle name={data.owner} size={36} />
        <div>
          <div className="text-sm text-white font-medium">{data.owner}</div>
          {data.metadata?.role && <div className="text-xs text-gray-500">{data.metadata.role}</div>}
        </div>
      </div>

      <div className="flex items-center gap-2 flex-wrap">
        <StatusBadge status={data.status} />
        <PriorityBadge priority={data.priority} />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <MetricCard label="Estimate" value={data.estimate} unit="pts" />
        <MetricCard label="Remaining" value={data.remainingWork} unit="hrs" />
      </div>

      {data.description && (
        <div>
          <SectionHeader>Description</SectionHeader>
          <p className="text-sm text-gray-300 leading-relaxed">{data.description}</p>
        </div>
      )}

      <div>
        <SectionHeader>Dependencies</SectionHeader>
        {data.dependencies.length === 0 ? (
          <p className="text-xs text-gray-500">None</p>
        ) : (
          <ul className="space-y-1.5">
            {data.dependencies.map((dep) => (
              <li key={dep.id} className="flex items-center gap-2 text-sm">
                <span
                  className="w-2 h-2 rounded-full shrink-0"
                  style={{ backgroundColor: STATUS_COLORS[dep.status] ?? '#666666' }}
                />
                <span className="text-gray-300 truncate">{dep.title}</span>
              </li>
            ))}
          </ul>
        )}
      </div>

      {data.recentActivity.length > 0 && (
        <div>
          <SectionHeader>Recent Activity</SectionHeader>
          <ul className="space-y-3">
            {data.recentActivity.slice(0, 5).map((evt) => {
              const typeInfo = EVENT_TYPE_LABELS[evt.type] ?? { label: evt.type, color: '#666' };
              return (
                <li key={evt.id} className="flex items-start gap-2.5">
                  <AvatarCircle name={evt.actor} size={24} />
                  <div className="min-w-0 flex-1">
                    <div className="text-xs text-gray-300 leading-snug truncate">
                      {evt.description}
                    </div>
                    <div className="flex items-center gap-2 mt-0.5">
                      <span
                        className="text-[10px] font-medium px-1.5 py-px rounded"
                        style={{ color: typeInfo.color, backgroundColor: `${typeInfo.color}15` }}
                        {typeInfo.label}
                      </span>
                      <span className="text-[10px] text-gray-600">{relativeTime(evt.timestamp)}</span>
                    </div>
                  </div>
                </li>
              );
            })}
          </ul>
        </div>
      )}

      {Object.keys(data.metadata).length > 0 && (
        <div>
          <SectionHeader>Details</SectionHeader>
          <dl className="space-y-1">
            {Object.entries(data.metadata).map(([key, val]) => (
              <div key={key} className="flex justify-between text-xs">
                <dt className="text-gray-500 capitalize">{key.replace(/([A-Z])/g, ' $1').trim()}</dt>
                <dd className="text-gray-300">{val}</dd>
              </div>
            ))}
          </dl>
        </div>
      )}
    </div>
  );
}

export function DetailPanel() {
  const { selectedEntityId, clearSelection } = useDashboardStore();
  const { data, loading, error } = useReportDetail(selectedEntityId);

  const isOpen = selectedEntityId !== null;

  useEffect(() => {
    if (!isOpen) return undefined;
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') clearSelection();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [isOpen, clearSelection]);

  const handleBackdropClick = useCallback(() => {
    clearSelection();
  }, [clearSelection]);

  const stopPropagation = useCallback((e: React.MouseEvent) => {
    e.stopPropagation();
  }, []);

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          <motion.div
            key="detail-backdrop"
            className="fixed inset-0 z-40"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={handleBackdropClick}
          />
          <motion.div
            key="detail-panel"
            className="fixed right-0 top-0 h-full w-[400px] z-50 flex flex-col"
            style={{
              backgroundColor: 'rgba(13, 17, 23, 0.9)',
              backdropFilter: 'blur(24px)',
              WebkitBackdropFilter: 'blur(24px)',
              borderLeft: '1px solid rgba(255, 255, 255, 0.1)',
            }}
            initial={{ x: 400 }}
            animate={{ x: 0 }}
            exit={{ x: 400 }}
            transition={{ duration: 0.3, ease: 'easeOut' }}
            onClick={stopPropagation}
            <button
              onClick={clearSelection}
              className="absolute top-4 right-4 text-gray-400 hover:text-cyan-400 transition-colors duration-200 z-10 p-1 rounded"
              aria-label="Close panel"
              type="button"
              <CloseIcon />
            </button>

            <div className="flex-1 overflow-y-auto p-6 pt-12 scrollbar-thin scrollbar-thumb-white/10">
              {loading && <LoadingSpinner />}
              {error && !loading && <ErrorState message={error} />}
              {data && !loading && !error && <PanelContent data={data} />}
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
}

export default DetailPanel;