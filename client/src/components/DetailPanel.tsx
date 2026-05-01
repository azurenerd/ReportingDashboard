import { useEffect, useRef, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useDashboardStore } from '../store/dashboardStore';
import { get } from '../api/client';
import type { ReportDetail, ActivityEvent } from '../types';

// ---------- useReportDetail hook ----------

interface UseReportDetailResult {
  data: ReportDetail | null;
  loading: boolean;
  error: string | null;
}

function useReportDetail(id: string | null): UseReportDetailResult {
  const [data, setData] = useState<ReportDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      setData(null);
      setLoading(false);
      setError(null);
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError(null);

    get<ReportDetail>(`/report/${id}`)
      .then((result) => {
        if (!cancelled) {
          setData(result);
          setLoading(false);
        }
      })
      .catch((err: Error) => {
        if (!cancelled) {
          setError(err.message || 'Failed to load report detail');
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [id]);

  return { data, loading, error };
}

// ---------- Badge helpers ----------

function statusColor(status: string): string {
  switch (status.toLowerCase()) {
    case 'done':
      return 'bg-green-500/20 text-green-400 border-green-500/40';
    case 'in-progress':
      return 'bg-blue-500/20 text-blue-400 border-blue-500/40';
    case 'blocked':
      return 'bg-red-500/20 text-red-400 border-red-500/40';
    case 'at-risk':
      return 'bg-orange-500/20 text-orange-400 border-orange-500/40';
    case 'not-started':
      return 'bg-gray-500/20 text-gray-400 border-gray-500/40';
    case 'open':
      return 'bg-yellow-500/20 text-yellow-400 border-yellow-500/40';
    case 'mitigated':
      return 'bg-emerald-500/20 text-emerald-400 border-emerald-500/40';
    case 'closed':
      return 'bg-gray-500/20 text-gray-400 border-gray-500/40';
    default:
      return 'bg-gray-500/20 text-gray-400 border-gray-500/40';
  }
}

function priorityColor(priority: string): string {
  switch (priority.toLowerCase()) {
    case 'critical':
      return 'bg-red-500/20 text-red-400 border-red-500/40';
    case 'high':
      return 'bg-orange-500/20 text-orange-400 border-orange-500/40';
    case 'medium':
      return 'bg-yellow-500/20 text-yellow-400 border-yellow-500/40';
    case 'low':
      return 'bg-cyan-500/20 text-cyan-400 border-cyan-500/40';
    default:
      return 'bg-gray-500/20 text-gray-400 border-gray-500/40';
  }
}

function formatTimestamp(ts: string): string {
  const date = new Date(ts);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffHours / 24);

  if (diffHours < 1) return 'Just now';
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}

function activityIcon(type: string): string {
  switch (type) {
    case 'pr-completed':
      return '🔀';
    case 'task-completed':
      return '✅';
    case 'comment':
      return '💬';
    case 'deployment':
      return '🚀';
    case 'review':
      return '👁️';
    default:
      return '📌';
  }
}

// ---------- DetailPanel component ----------

export default function DetailPanel() {
  const selectedEntityId = useDashboardStore((s) => s.selectedEntityId);
  const clearSelection = useDashboardStore((s) => s.clearSelection);
  const { data, loading, error } = useReportDetail(selectedEntityId);
  const panelRef = useRef<HTMLDivElement>(null);

  const handleClose = useCallback(() => {
    clearSelection();
  }, [clearSelection]);

  // Close on Escape key
  useEffect(() => {
    if (!selectedEntityId) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        handleClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [selectedEntityId, handleClose]);

  // Close on outside click
  const handleBackdropClick = useCallback(
    (e: React.MouseEvent<HTMLDivElement>) => {
      if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
        handleClose();
      }
    },
    [handleClose]
  );

  return (
    <AnimatePresence>
      {selectedEntityId && (
        <div
          className="fixed inset-0 z-50"
          onClick={handleBackdropClick}
          aria-hidden="true"
          <motion.div
            ref={panelRef}
            initial={{ x: 400 }}
            animate={{ x: 0 }}
            exit={{ x: 400 }}
            transition={{ duration: 0.3, ease: 'easeOut' }}
            className="absolute top-0 right-0 h-full w-[400px] overflow-y-auto border-l border-white/10 shadow-2xl"
            style={{
              backgroundColor: 'rgba(13, 17, 23, 0.9)',
              backdropFilter: 'blur(24px)',
              WebkitBackdropFilter: 'blur(24px)',
            }}
            role="dialog"
            aria-label="Report detail panel"
            {/* Header with close button */}
            <div className="sticky top-0 z-10 flex items-center justify-between p-4 border-b border-white/10 bg-[#0d1117]/80 backdrop-blur-sm">
              <h2 className="text-sm font-medium text-gray-400 uppercase tracking-wider">
                Detail
              </h2>
              <button
                onClick={handleClose}
                className="flex items-center justify-center w-8 h-8 rounded-md text-gray-400 hover:text-white hover:bg-white/10 transition-colors"
                aria-label="Close panel"
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="18"
                  height="18"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  <line x1="18" y1="6" x2="6" y2="18" />
                  <line x1="6" y1="6" x2="18" y2="18" />
                </svg>
              </button>
            </div>

            {/* Content area */}
            <div className="p-4">
              {loading && <LoadingState />}
              {error && <ErrorState message={error} />}
              {!loading && !error && data && <DetailContent data={data} />}
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
}

// ---------- Sub-components ----------

function LoadingState() {
  return (
    <div className="flex flex-col items-center justify-center py-16 gap-3">
      <div className="w-8 h-8 border-2 border-cyan-500/30 border-t-cyan-400 rounded-full animate-spin" />
      <p className="text-sm text-gray-500">Loading details…</p>
    </div>
  );
}

function ErrorState({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 gap-3">
      <div className="w-10 h-10 rounded-full bg-red-500/10 flex items-center justify-center">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          className="text-red-400"
          <circle cx="12" cy="12" r="10" />
          <line x1="15" y1="9" x2="9" y2="15" />
          <line x1="9" y1="9" x2="15" y2="15" />
        </svg>
      </div>
      <p className="text-sm text-red-400 text-center">{message}</p>
    </div>
  );
}

function DetailContent({ data }: { data: ReportDetail }) {
  return (
    <div className="space-y-5">
      {/* Title & Type */}
      <div>
        <span className="text-xs font-medium text-gray-500 uppercase tracking-wider">
          {data.type}
        </span>
        <h3 className="mt-1 text-lg font-semibold text-white leading-snug">
          {data.title}
        </h3>
      </div>

      {/* Description */}
      {data.description && (
        <p className="text-sm text-gray-300 leading-relaxed">
          {data.description}
        </p>
      )}

      {/* Badges: Status & Priority */}
      <div className="flex flex-wrap gap-2">
        <span
          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${statusColor(data.status)}`}
          {data.status}
        </span>
        <span
          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${priorityColor(data.priority)}`}
          {data.priority}
        </span>
      </div>

      {/* Owner */}
      <div className="flex items-center gap-2">
        <div className="w-7 h-7 rounded-full bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center text-[10px] font-bold text-white">
          {data.owner
            .split(' ')
            .map((n) => n[0])
            .join('')
            .slice(0, 2)}
        </div>
        <div>
          <p className="text-sm text-white">{data.owner}</p>
          <p className="text-xs text-gray-500">Owner</p>
        </div>
      </div>

      {/* Metrics: Estimate & Remaining Work */}
      {(data.estimate !== null || data.remainingWork !== null) && (
        <div className="grid grid-cols-2 gap-3">
          {data.estimate !== null && (
            <div className="rounded-lg border border-white/10 bg-white/5 p-3">
              <p className="text-xs text-gray-500">Estimate</p>
              <p className="text-lg font-semibold text-white">
                {data.estimate}
                <span className="text-xs text-gray-400 ml-1">pts</span>
              </p>
            </div>
          )}
          {data.remainingWork !== null && (
            <div className="rounded-lg border border-white/10 bg-white/5 p-3">
              <p className="text-xs text-gray-500">Remaining</p>
              <p className="text-lg font-semibold text-white">
                {data.remainingWork}
                <span className="text-xs text-gray-400 ml-1">hrs</span>
              </p>
            </div>
          )}
        </div>
      )}

      {/* Dependencies */}
      {data.dependencies.length > 0 && (
        <div>
          <h4 className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-2">
            Dependencies
          </h4>
          <ul className="space-y-1.5">
            {data.dependencies.map((dep) => (
              <li
                key={dep.id}
                className="flex items-center justify-between rounded-md border border-white/10 bg-white/5 px-3 py-2"
                <span className="text-sm text-gray-200 truncate mr-2">
                  {dep.title}
                </span>
                <span
                  className={`shrink-0 inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-medium border ${statusColor(dep.status)}`}
                  {dep.status}
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Recent Activity */}
      {data.recentActivity.length > 0 && (
        <div>
          <h4 className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-2">
            Recent Activity
          </h4>
          <ul className="space-y-2">
            {data.recentActivity.map((event: ActivityEvent) => (
              <li
                key={event.id}
                className="flex items-start gap-2 rounded-md border border-white/10 bg-white/5 px-3 py-2"
                <span className="text-sm mt-0.5 shrink-0">
                  {activityIcon(event.type)}
                </span>
                <div className="min-w-0 flex-1">
                  <p className="text-sm text-gray-200 leading-snug">
                    {event.description}
                  </p>
                  <p className="text-xs text-gray-500 mt-0.5">
                    {event.actor} · {formatTimestamp(event.timestamp)}
                  </p>
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}