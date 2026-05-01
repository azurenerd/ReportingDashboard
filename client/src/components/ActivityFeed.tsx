/**
 * Team Activity Feed — scrollable list of recent team events.
 *
 * Renders 20+ activity events fetched via the useTeamActivity hook.
 * Each entry uses Framer Motion for a staggered fade-in + slide-from-left
 * entrance. Recent events (< 5 min old) show a pulsing activity dot.
 * Avatars display initials in colored circles. Timestamps are relative.
 * Event type badges are color-coded by category.
 */

import { motion } from 'framer-motion';
import { useTeamActivity } from '../hooks/useProjectData';
import type { ActivityEvent, ActivityEventType } from '../types';

// ── Event type badge configuration ──

interface BadgeConfig {
  label: string;
  bg: string;
  text: string;
}

const BADGE_MAP: Record<ActivityEventType, BadgeConfig> = {
  'pr-completed': { label: 'PR', bg: 'bg-purple-500/20', text: 'text-purple-300' },
  'task-completed': { label: 'Task', bg: 'bg-green-500/20', text: 'text-green-300' },
  comment: { label: 'Comment', bg: 'bg-blue-500/20', text: 'text-blue-300' },
  deployment: { label: 'Deploy', bg: 'bg-orange-500/20', text: 'text-orange-300' },
  review: { label: 'Review', bg: 'bg-cyan-500/20', text: 'text-cyan-300' },
};

// ── Avatar color palette — deterministic per initials ──

const AVATAR_COLORS = [
  'from-cyan-500 to-blue-600',
  'from-purple-500 to-pink-600',
  'from-green-500 to-emerald-600',
  'from-orange-500 to-red-600',
  'from-indigo-500 to-violet-600',
  'from-teal-500 to-cyan-600',
  'from-rose-500 to-pink-600',
  'from-amber-500 to-yellow-600',
];

function avatarColor(initials: string): string {
  let hash = 0;
  for (let i = 0; i < initials.length; i++) {
    hash = initials.charCodeAt(i) + ((hash << 5) - hash);
  }
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

// ── Relative timestamp formatting ──

function relativeTime(isoTimestamp: string): string {
  const now = Date.now();
  const then = new Date(isoTimestamp).getTime();
  const diffSec = Math.max(0, Math.floor((now - then) / 1000));

  if (diffSec < 60) return 'just now';
  const diffMin = Math.floor(diffSec / 60);
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffHr = Math.floor(diffMin / 60);
  if (diffHr < 24) return `${diffHr}h ago`;
  const diffDay = Math.floor(diffHr / 24);
  if (diffDay < 30) return `${diffDay}d ago`;
  const diffMonth = Math.floor(diffDay / 30);
  return `${diffMonth}mo ago`;
}

/** Returns true if the event is less than 5 minutes old (shows pulse). */
function isRecent(isoTimestamp: string): boolean {
  return Date.now() - new Date(isoTimestamp).getTime() < 5 * 60 * 1000;
}

// ── Framer Motion variants ──

const listVariants = {
  hidden: {},
  visible: {
    transition: { staggerChildren: 0.06 },
  },
};

const itemVariants = {
  hidden: { opacity: 0, x: -24 },
  visible: {
    opacity: 1,
    x: 0,
    transition: { duration: 0.35, ease: 'easeOut' },
  },
};

// ── Sub-components ──

function EventTypeBadge({ type }: { type: ActivityEventType }) {
  const config = BADGE_MAP[type] ?? { label: type, bg: 'bg-gray-500/20', text: 'text-gray-300' };
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wider ${config.bg} ${config.text}`}
      {config.label}
    </span>
  );
}

function AvatarCircle({ initials }: { initials: string }) {
  const gradient = avatarColor(initials);
  return (
    <div
      className={`flex-shrink-0 w-8 h-8 rounded-full bg-gradient-to-br ${gradient} flex items-center justify-center text-[11px] font-bold text-white shadow-lg shadow-black/30`}
      {initials}
    </div>
  );
}

function ActivityItem({ event }: { event: ActivityEvent }) {
  const recent = isRecent(event.timestamp);

  return (
    <motion.div
      variants={itemVariants}
      className="flex items-start gap-3 px-3 py-2.5 rounded-lg hover:bg-white/[0.04] transition-colors duration-200 group"
      {/* Avatar */}
      <div className="relative mt-0.5">
        <AvatarCircle initials={event.actorAvatar} />
        {/* Activity pulse dot for recent events */}
        {recent && (
          <span className="absolute -top-0.5 -right-0.5 flex h-2.5 w-2.5">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-accent-cyan opacity-75" />
            <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-accent-cyan" />
          </span>
        )}
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-0.5">
          <span className="text-xs font-semibold text-white truncate">{event.actor}</span>
          <EventTypeBadge type={event.type} />
        </div>
        <p className="text-xs text-gray-400 leading-relaxed line-clamp-2">{event.description}</p>
        <span className="text-[10px] text-gray-500 mt-1 block">{relativeTime(event.timestamp)}</span>
      </div>
    </motion.div>
  );
}

// ── Main component ──

export default function ActivityFeed() {
  const { data, loading, error } = useTeamActivity();

  // Loading state
  if (loading) {
    return (
      <div className="absolute bottom-4 right-4 z-10 w-80">
        <div className="glass-card glow-border-purple">
          <h2 className="text-sm font-semibold text-accent-purple mb-3 flex items-center gap-2">
            <ActivityIcon />
            Team Activity
          </h2>
          <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="flex items-start gap-3 animate-pulse">
                <div className="w-8 h-8 rounded-full bg-white/10" />
                <div className="flex-1 space-y-2">
                  <div className="h-3 bg-white/10 rounded w-3/4" />
                  <div className="h-2 bg-white/10 rounded w-1/2" />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="absolute bottom-4 right-4 z-10 w-80">
        <div className="glass-card glow-border-purple">
          <h2 className="text-sm font-semibold text-accent-purple mb-2 flex items-center gap-2">
            <ActivityIcon />
            Team Activity
          </h2>
          <p className="text-xs text-red-400">{error}</p>
        </div>
      </div>
    );
  }

  const events = data?.events ?? [];

  return (
    <div className="absolute bottom-4 right-4 z-10 w-80">
      <div className="glass-card glow-border-purple flex flex-col max-h-[420px]">
        {/* Header */}
        <div className="flex items-center justify-between mb-3 flex-shrink-0">
          <h2 className="text-sm font-semibold text-accent-purple flex items-center gap-2">
            <ActivityIcon />
            Team Activity
          </h2>
          <span className="text-[10px] text-gray-500 font-medium">
            {events.length} events
          </span>
        </div>

        {/* Scrollable event list */}
        <motion.div
          className="overflow-y-auto flex-1 -mx-1 px-1 space-y-0.5 scrollbar-thin"
          style={{
            scrollbarWidth: 'thin',
            scrollbarColor: 'rgba(179,136,255,0.3) transparent',
          }}
          variants={listVariants}
          initial="hidden"
          animate="visible"
          {events.map((event) => (
            <ActivityItem key={event.id} event={event} />
          ))}

          {events.length === 0 && (
            <p className="text-xs text-gray-500 text-center py-6">No activity yet.</p>
          )}
        </motion.div>
      </div>
    </div>
  );
}

/** Small activity/pulse icon for the header. */
function ActivityIcon() {
  return (
    <svg
      className="w-4 h-4 text-accent-purple"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      <polyline points="22 12 18 12 15 21 9 3 6 12 2 12" />
    </svg>
  );
}