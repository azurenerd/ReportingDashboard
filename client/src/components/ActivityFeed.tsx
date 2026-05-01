import { motion, AnimatePresence } from 'framer-motion';
import { useTeamActivity } from '../api/client';
import type { ActivityEvent, ActivityEventType } from '../types';

// ── Relative timestamp formatting ────────────────────────────────────────────

/** Converts an ISO timestamp to a human-readable relative string (e.g. "2h ago"). */
function relativeTime(iso: string): string {
  const now = Date.now();
  const then = new Date(iso).getTime();
  const diffSec = Math.max(0, Math.floor((now - then) / 1000));

  if (diffSec < 60) return 'Just now';
  const diffMin = Math.floor(diffSec / 60);
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffHr = Math.floor(diffMin / 60);
  if (diffHr < 24) return `${diffHr}h ago`;
  const diffDay = Math.floor(diffHr / 24);
  if (diffDay < 7) return `${diffDay}d ago`;
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}

/** Returns true if the event happened within the last 30 minutes (considered "recent"). */
function isRecent(iso: string): boolean {
  return Date.now() - new Date(iso).getTime() < 30 * 60 * 1000;
}

// ── Event type badge configuration ───────────────────────────────────────────

interface BadgeConfig {
  label: string;
  icon: string;
  color: string;        // Tailwind text color
  bg: string;           // Tailwind bg + border classes
}

const BADGE_MAP: Record<ActivityEventType, BadgeConfig> = {
  'pr-completed': {
    label: 'PR',
    icon: '🔀',
    color: 'text-purple-400',
    bg: 'bg-purple-500/20 border-purple-500/40',
  },
  'task-completed': {
    label: 'Task',
    icon: '✅',
    color: 'text-green-400',
    bg: 'bg-green-500/20 border-green-500/40',
  },
  comment: {
    label: 'Comment',
    icon: '💬',
    color: 'text-blue-400',
    bg: 'bg-blue-500/20 border-blue-500/40',
  },
  deployment: {
    label: 'Deploy',
    icon: '🚀',
    color: 'text-orange-400',
    bg: 'bg-orange-500/20 border-orange-500/40',
  },
  review: {
    label: 'Review',
    icon: '👁️',
    color: 'text-cyan-400',
    bg: 'bg-cyan-500/20 border-cyan-500/40',
  },
};

function getBadge(type: ActivityEventType): BadgeConfig {
  return BADGE_MAP[type] ?? { label: type, icon: '📌', color: 'text-gray-400', bg: 'bg-gray-500/20 border-gray-500/40' };
}

// ── Avatar helper ────────────────────────────────────────────────────────────

/** Deterministic gradient based on initials for visual variety. */
const AVATAR_GRADIENTS = [
  'from-cyan-500 to-blue-600',
  'from-purple-500 to-pink-500',
  'from-emerald-500 to-teal-500',
  'from-orange-500 to-red-500',
  'from-indigo-500 to-purple-600',
  'from-rose-500 to-pink-600',
  'from-amber-500 to-orange-500',
  'from-teal-500 to-cyan-500',
];

function avatarGradient(initials: string): string {
  let hash = 0;
  for (let i = 0; i < initials.length; i++) {
    hash = initials.charCodeAt(i) + ((hash << 5) - hash);
  }
  return AVATAR_GRADIENTS[Math.abs(hash) % AVATAR_GRADIENTS.length];
}

// ── Framer Motion variants ───────────────────────────────────────────────────

/** Container staggers children entry on mount. */
const listVariants = {
  hidden: {},
  visible: {
    transition: { staggerChildren: 0.04 },
  },
};

/** Each item fades in and slides from the left. */
const itemVariants = {
  hidden: { opacity: 0, x: -24 },
  visible: {
    opacity: 1,
    x: 0,
    transition: { duration: 0.35, ease: 'easeOut' },
  },
};

// ── Sub-components ───────────────────────────────────────────────────────────

/** Single activity event row with avatar, description, badge, and timestamp. */
function ActivityRow({ event }: { event: ActivityEvent }) {
  const badge = getBadge(event.type);
  const recent = isRecent(event.timestamp);

  return (
    <motion.li
      variants={itemVariants}
      className="flex items-start gap-2.5 px-3 py-2 rounded-lg hover:bg-white/5 transition-colors group"
      {/* Avatar circle with initials */}
      <div className="relative shrink-0 mt-0.5">
        <div
          className={`w-7 h-7 rounded-full bg-gradient-to-br ${avatarGradient(event.actorAvatar)} flex items-center justify-center text-[10px] font-bold text-white select-none`}
          {event.actorAvatar}
        </div>

        {/* Activity pulse dot for recent events */}
        {recent && (
          <span className="absolute -top-0.5 -right-0.5 flex h-2.5 w-2.5">
            <span className="absolute inline-flex h-full w-full rounded-full bg-cyan-400 opacity-75 animate-ping" />
            <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-cyan-400" />
          </span>
        )}
      </div>

      {/* Content: description + meta line */}
      <div className="min-w-0 flex-1">
        <p className="text-xs text-gray-200 leading-snug line-clamp-2">
          {event.description}
        </p>
        <div className="flex items-center gap-2 mt-1">
          {/* Event type badge */}
          <span
            className={`inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-[9px] font-semibold uppercase tracking-wide border ${badge.bg} ${badge.color}`}
            <span className="text-[10px] leading-none">{badge.icon}</span>
            {badge.label}
          </span>

          <span className="text-[10px] text-gray-500">{event.actor}</span>
          <span className="text-[10px] text-gray-600">·</span>
          <span className="text-[10px] text-gray-500 tabular-nums">
            {relativeTime(event.timestamp)}
          </span>
        </div>
      </div>
    </motion.li>
  );
}

/** Loading skeleton with pulsing placeholder rows. */
function LoadingSkeleton() {
  return (
    <div className="space-y-2 p-3">
      {Array.from({ length: 6 }).map((_, i) => (
        <div key={i} className="flex items-center gap-2.5 animate-pulse">
          <div className="w-7 h-7 rounded-full bg-white/10 shrink-0" />
          <div className="flex-1 space-y-1.5">
            <div className="h-2.5 bg-white/10 rounded w-3/4" />
            <div className="h-2 bg-white/10 rounded w-1/2" />
          </div>
        </div>
      ))}
    </div>
  );
}

/** Error state with retry hint. */
function ErrorState({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-8 px-4 gap-2">
      <svg
        className="w-6 h-6 text-red-400"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"
        />
      </svg>
      <p className="text-xs text-red-400 text-center">Failed to load activity</p>
      <p className="text-[10px] text-gray-500 text-center">{message}</p>
    </div>
  );
}

// ── Main component ───────────────────────────────────────────────────────────

/**
 * Team Activity Feed (US-08)
 *
 * Scrollable list of recent team activity events displayed in the bottom-right
 * corner of the dashboard overlay. Features:
 * - Fetches data from GET /api/team-activity via useTeamActivity hook
 * - Framer Motion staggered entry animations (fade-in + slide from left)
 * - Activity pulse dot on events within the last 30 minutes
 * - Avatar circles with initials and deterministic color gradients
 * - Relative timestamps (e.g. "2h ago", "3d ago")
 * - Typed event badges (PR, Task, Comment, Deploy, Review)
 * - Glassmorphism container matching dashboard visual language
 */
export default function ActivityFeed() {
  const { data, loading, error } = useTeamActivity();

  const events = data?.events ?? [];

  return (
    <motion.div
      className="glass-card w-[340px] max-h-[420px] flex flex-col overflow-hidden"
      style={{
        boxShadow: '0 0 20px rgba(0, 212, 255, 0.1), inset 0 0 20px rgba(0, 212, 255, 0.03)',
      }}
      initial={{ opacity: 0, y: 30 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.6, delay: 0.3 }}
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-2.5 border-b border-white/10 shrink-0">
        <div className="flex items-center gap-2">
          <span className="text-sm">⚡</span>
          <h3 className="text-xs font-semibold text-gray-300 uppercase tracking-wider">
            Team Activity
          </h3>
        </div>
        {events.length > 0 && (
          <span className="text-[10px] text-gray-500 tabular-nums">
            {events.length} events
          </span>
        )}
      </div>

      {/* Content area — scrollable */}
      <div className="flex-1 overflow-y-auto min-h-0">
        {loading && <LoadingSkeleton />}

        {!loading && error && <ErrorState message={error.message} />}

        {!loading && !error && events.length === 0 && (
          <div className="flex items-center justify-center py-10">
            <p className="text-xs text-gray-500">No activity yet</p>
          </div>
        )}

        <AnimatePresence>
          {!loading && !error && events.length > 0 && (
            <motion.ul
              className="py-1.5 space-y-0.5"
              variants={listVariants}
              initial="hidden"
              animate="visible"
              {events.map((event) => (
                <ActivityRow key={event.id} event={event} />
              ))}
            </motion.ul>
          )}
        </AnimatePresence>
      </div>
    </motion.div>
  );
}