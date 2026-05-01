import { useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useTeamActivity } from '../hooks/useProjectData';
import type { ActivityEvent, ActivityEventType } from '../types';

// ── Event type badge configuration ──

interface BadgeConfig {
  label: string;
  bg: string;
  text: string;
  dot: string;
}

/**
 * Color-coded badge config for each activity event type.
 * Uses accent-* color tokens defined in globals.css @theme block
 * (Tailwind v4 CSS-first config auto-generates utility classes).
 */
const EVENT_BADGE_MAP: Record<ActivityEventType, BadgeConfig> = {
  'pr-completed': {
    label: 'PR',
    bg: 'bg-accent-purple/20',
    text: 'text-accent-purple',
    dot: 'bg-accent-purple',
  },
  'task-completed': {
    label: 'Task',
    bg: 'bg-accent-green/20',
    text: 'text-accent-green',
    dot: 'bg-accent-green',
  },
  comment: {
    label: 'Comment',
    bg: 'bg-accent-cyan/20',
    text: 'text-accent-cyan',
    dot: 'bg-accent-cyan',
  },
  deployment: {
    label: 'Deploy',
    bg: 'bg-accent-orange/20',
    text: 'text-accent-orange',
    dot: 'bg-accent-orange',
  },
  review: {
    label: 'Review',
    bg: 'bg-blue-500/20',
    text: 'text-blue-400',
    dot: 'bg-blue-400',
  },
};

// ── Relative timestamp helper ──

/** Converts an ISO-8601 timestamp to a human-friendly relative string (e.g., "2h ago"). */
function relativeTime(isoTimestamp: string): string {
  const now = Date.now();
  const then = new Date(isoTimestamp).getTime();
  const diffMs = now - then;

  if (diffMs < 0) return 'just now';

  const seconds = Math.floor(diffMs / 1000);
  if (seconds < 60) return `${seconds}s ago`;

  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;

  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;

  const days = Math.floor(hours / 24);
  if (days < 30) return `${days}d ago`;

  const months = Math.floor(days / 30);
  return `${months}mo ago`;
}

// ── Avatar component ──

/** Renders a circular avatar with the actor's initials and a color derived from the event type. */
function AvatarCircle({ initials, eventType }: { initials: string; eventType: ActivityEventType }) {
  const badge = EVENT_BADGE_MAP[eventType];
  return (
    <div
      className={`flex-shrink-0 w-9 h-9 rounded-full flex items-center justify-center text-xs font-bold ${badge.bg} ${badge.text} ring-1 ring-white/10`}
      {initials}
    </div>
  );
}

// ── Pulse dot for recent events ──

function PulseDot() {
  return (
    <span className="relative flex h-2.5 w-2.5 flex-shrink-0">
      <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-accent-cyan opacity-75" />
      <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-accent-cyan" />
    </span>
  );
}

// ── Single event row ──

/** Framer Motion variants for staggered fade-in + slide-from-left entry. */
const eventRowVariants = {
  hidden: { opacity: 0, x: -24 },
  visible: (i: number) => ({
    opacity: 1,
    x: 0,
    transition: {
      delay: i * 0.04,
      duration: 0.35,
      ease: [0.25, 0.46, 0.45, 0.94],
    },
  }),
};

function EventRow({ event, index, isRecent }: { event: ActivityEvent; index: number; isRecent: boolean }) {
  const badge = EVENT_BADGE_MAP[event.type] ?? EVENT_BADGE_MAP['comment'];
  const timestamp = useMemo(() => relativeTime(event.timestamp), [event.timestamp]);

  return (
    <motion.div
      className="flex items-start gap-3 px-3 py-2.5 rounded-lg hover:bg-white/[0.04] transition-colors duration-200 group"
      variants={eventRowVariants}
      custom={index}
      initial="hidden"
      animate="visible"
      layout
      {/* Avatar */}
      <AvatarCircle initials={event.actorAvatar} eventType={event.type} />

      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-0.5">
          {/* Actor name */}
          <span className="text-sm font-semibold text-white truncate">{event.actor}</span>

          {/* Event type badge */}
          <span
            className={`inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-semibold uppercase tracking-wide ${badge.bg} ${badge.text}`}
            {badge.label}
          </span>

          {/* Pulse dot for recent events — index-based so it works with static mock data */}
          {isRecent && <PulseDot />}
        </div>

        {/* Description */}
        <p className="text-xs text-gray-400 leading-relaxed line-clamp-2">{event.description}</p>
      </div>

      {/* Timestamp */}
      <span className="text-[10px] text-gray-500 whitespace-nowrap pt-0.5 flex-shrink-0">
        {timestamp}
      </span>
    </motion.div>
  );
}

// ── Loading skeleton ──

function LoadingSkeleton() {
  return (
    <div className="absolute bottom-4 right-4 z-10 w-[380px]">
      <div className="glass-card glow-border-cyan">
        <div className="flex items-center gap-2 mb-3">
          <div className="w-2 h-2 rounded-full bg-accent-cyan/40 animate-pulse" />
          <div className="h-4 w-28 rounded bg-white/10 animate-pulse" />
        </div>
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="flex items-start gap-3 animate-pulse">
              <div className="w-9 h-9 rounded-full bg-white/10 flex-shrink-0" />
              <div className="flex-1 space-y-1.5">
                <div className="h-3 w-24 rounded bg-white/10" />
                <div className="h-2.5 w-full rounded bg-white/[0.06]" />
              </div>
              <div className="h-2.5 w-10 rounded bg-white/[0.06]" />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

// ── Error state ──

function ErrorState({ message }: { message: string }) {
  return (
    <div className="absolute bottom-4 right-4 z-10 w-[380px]">
      <div className="glass-card glow-border-red">
        <div className="flex items-center gap-2">
          <svg
            className="w-5 h-5 text-accent-red flex-shrink-0"
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
          <div>
            <p className="text-sm font-semibold text-accent-red">Activity feed unavailable</p>
            <p className="text-xs text-gray-400 mt-0.5">{message}</p>
          </div>
        </div>
      </div>
    </div>
  );
}

// ── Main component ──

/**
 * Team Activity Feed (US-08)
 *
 * Scrollable feed of recent team activity events displayed as an HTML overlay
 * in the bottom-right corner of the dashboard. Features:
 * - Avatar circles with actor initials
 * - Color-coded event type badges (PR, Task, Comment, Deploy, Review)
 * - Relative timestamps (e.g., "2h ago")
 * - Animated pulse dot on the 5 most recent events (index-based for static mock data)
 * - Framer Motion staggered entry animations (fade-in + slide from left)
 * - Glassmorphism container (glass-card-lg from globals.css) with fixed height and internal scroll
 *
 * Data fetched from GET /api/team-activity via the useTeamActivity hook
 * (exported from hooks/useProjectData.ts).
 */
export default function ActivityFeed() {
  const { data, loading, error } = useTeamActivity();

  if (loading) return <LoadingSkeleton />;
  if (error) return <ErrorState message={String(error)} />;
  if (!data || data.events.length === 0) return null;

  // Mark the top 5 events as "recent" for the pulse dot indicator.
  // Uses index-based threshold (not time-based) so it works with static mock data.
  const recentThreshold = 5;

  return (
    <div className="absolute bottom-4 right-4 z-10 w-[380px] pointer-events-auto">
      <motion.div
        className="glass-card-lg glow-border-cyan flex flex-col max-h-[480px]"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, delay: 0.6 }}
        {/* Header */}
        <div className="flex items-center justify-between mb-3 flex-shrink-0">
          <div className="flex items-center gap-2">
            <span className="relative flex h-2 w-2">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-accent-cyan opacity-75" />
              <span className="relative inline-flex h-2 w-2 rounded-full bg-accent-cyan" />
            </span>
            <h3 className="text-sm font-bold text-white uppercase tracking-wider">
              Team Activity
            </h3>
          </div>
          <span className="text-[10px] text-gray-500 font-medium tabular-nums">
            {data.events.length} events · {data.teamMembers.length} members
          </span>
        </div>

        {/* Scrollable event list */}
        <div className="overflow-y-auto flex-1 -mx-2 px-2 scrollbar-thin">
          <AnimatePresence mode="popLayout">
            {data.events.map((event, index) => (
              <EventRow
                key={event.id}
                event={event}
                index={index}
                isRecent={index < recentThreshold}
              />
            ))}
          </AnimatePresence>
        </div>
      </motion.div>
    </div>
  );
}