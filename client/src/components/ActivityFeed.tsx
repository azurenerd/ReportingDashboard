import { motion } from 'framer-motion';
import { useProjectData } from '../hooks/useProjectData';
import type { ActivityEvent, ActivityEventType } from '../types';

// ── Relative time utility ──

/** Converts an ISO timestamp to a human-readable relative string (e.g. "2h ago"). */
function getRelativeTime(timestamp: string): string {
  const now = Date.now();
  const then = new Date(timestamp).getTime();
  const diffMs = now - then;

  // Guard against future timestamps (timezone drift in mock data)
  if (diffMs < 0) return 'just now';

  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHr = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHr / 24);
  const diffWeek = Math.floor(diffDay / 7);

  if (diffSec < 60) return 'just now';
  if (diffMin < 60) return `${diffMin}m ago`;
  if (diffHr < 24) return `${diffHr}h ago`;
  if (diffDay < 7) return `${diffDay}d ago`;
  if (diffWeek < 4) return `${diffWeek}w ago`;
  return new Date(timestamp).toLocaleDateString();
}

// ── Badge configuration per event type ──

const badgeConfig: Record<ActivityEventType, { label: string; className: string }> = {
  'pr-completed': {
    label: 'PR',
    className: 'bg-purple-500/20 text-purple-300 border-purple-500/30',
  },
  'task-completed': {
    label: 'Task',
    className: 'bg-green-500/20 text-green-300 border-green-500/30',
  },
  comment: {
    label: 'Comment',
    className: 'bg-blue-500/20 text-blue-300 border-blue-500/30',
  },
  deployment: {
    label: 'Deploy',
    className: 'bg-orange-500/20 text-orange-300 border-orange-500/30',
  },
  review: {
    label: 'Review',
    className: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/30',
  },
};

// ── Avatar color palette — deterministic by initials ──

const avatarColors = [
  'bg-indigo-500',
  'bg-rose-500',
  'bg-emerald-500',
  'bg-amber-500',
  'bg-cyan-500',
  'bg-fuchsia-500',
  'bg-teal-500',
  'bg-violet-500',
  'bg-orange-500',
  'bg-sky-500',
];

function getAvatarColor(initials: string): string {
  let hash = 0;
  for (let i = 0; i < initials.length; i++) {
    hash = initials.charCodeAt(i) + ((hash << 5) - hash);
  }
  return avatarColors[Math.abs(hash) % avatarColors.length];
}

// ── Local hook wrapping useProjectData for team activity slice ──

function useTeamActivity() {
  const { data, loading, error } = useProjectData();
  return {
    events: data.teamActivity?.events ?? [],
    loading,
    error,
  };
}

// ── Loading skeleton shown while data is in flight ──

function LoadingSkeleton() {
  return (
    <div className="space-y-3 p-3">
      {Array.from({ length: 5 }).map((_, i) => (
        <div key={i} className="flex items-center gap-3 animate-pulse">
          <div className="w-8 h-8 rounded-full bg-white/10 shrink-0" />
          <div className="flex-1 space-y-1.5">
            <div className="h-3 bg-white/10 rounded w-3/4" />
            <div className="h-2.5 bg-white/5 rounded w-1/2" />
          </div>
        </div>
      ))}
    </div>
  );
}

// ── Number of recent events that get the pulse dot ──
const RECENT_THRESHOLD = 5;

// ── Cap stagger animations so below-fold items don't accumulate invisible delay ──
const MAX_STAGGER_INDEX = 15;

// ── Main component ──

export default function ActivityFeed() {
  const { events, loading, error } = useTeamActivity();

  return (
    <div className="fixed bottom-6 right-6 w-80 max-h-[480px] flex flex-col z-30 pointer-events-auto">
      {/* Glassmorphism panel */}
      <div className="backdrop-blur-xl bg-white/5 border border-white/10 rounded-2xl shadow-2xl overflow-hidden flex flex-col">
        {/* Header */}
        <div className="px-4 py-3 border-b border-white/10 flex items-center gap-2">
          <div className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
          <h3 className="text-sm font-semibold text-white/90 tracking-wide uppercase">
            Team Activity
          </h3>
          <span className="ml-auto text-xs text-white/40">
            {events.length} events
          </span>
        </div>

        {/* Content area */}
        {loading && <LoadingSkeleton />}

        {error && (
          <div className="p-4 text-center text-red-400 text-xs">
            Failed to load activity feed
          </div>
        )}

        {!loading && !error && (
          <div
            className="overflow-y-auto flex-1 max-h-[400px] px-2 py-2 space-y-1"
            style={{
              scrollbarWidth: 'thin',
              scrollbarColor: 'rgba(255,255,255,0.15) transparent',
            }}
            {events.map((event: ActivityEvent, index: number) => (
              <motion.div
                key={event.id}
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{
                  delay: Math.min(index, MAX_STAGGER_INDEX) * 0.05,
                  duration: 0.3,
                  ease: 'easeOut',
                }}
                className="flex items-start gap-2.5 p-2 rounded-lg hover:bg-white/5 transition-colors group"
                {/* Avatar with optional pulse dot for recent events */}
                <div className="relative shrink-0">
                  <div
                    className={`w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-bold text-white ${getAvatarColor(event.actorAvatar)}`}
                    {event.actorAvatar}
                  </div>
                  {/* Pulse dot indicator for the top N recent events */}
                  {index < RECENT_THRESHOLD && (
                    <span className="absolute -top-0.5 -right-0.5 flex h-2.5 w-2.5">
                      <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75" />
                      <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-emerald-400" />
                    </span>
                  )}
                </div>

                {/* Event content */}
                <div className="flex-1 min-w-0">
                  <p className="text-xs text-white/80 leading-snug line-clamp-2">
                    {event.description}
                  </p>
                  <div className="flex items-center gap-2 mt-1">
                    {/* Type badge */}
                    <span
                      className={`inline-flex items-center px-1.5 py-0.5 rounded text-[9px] font-medium border ${badgeConfig[event.type].className}`}
                      {badgeConfig[event.type].label}
                    </span>
                    {/* Relative timestamp */}
                    <span className="text-[10px] text-white/40">
                      {getRelativeTime(event.timestamp)}
                    </span>
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}