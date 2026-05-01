import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useTeamActivity } from '../hooks/useTeamActivity';
import type { TeamActivityEvent } from '../hooks/useTeamActivity';

/** Activity event types per the architecture data model */
type ActivityEventType = 'pr-completed' | 'task-completed' | 'comment' | 'deployment' | 'review';

// ---- Helper Utilities ----

/** Predefined gradients for deterministic avatar backgrounds */
const AVATAR_COLORS = [
  'linear-gradient(135deg, #6366f1, #a855f7)',
  'linear-gradient(135deg, #ec4899, #f43f5e)',
  'linear-gradient(135deg, #14b8a6, #06b6d4)',
  'linear-gradient(135deg, #f59e0b, #ef4444)',
  'linear-gradient(135deg, #8b5cf6, #6366f1)',
  'linear-gradient(135deg, #10b981, #059669)',
  'linear-gradient(135deg, #3b82f6, #2563eb)',
  'linear-gradient(135deg, #f97316, #ea580c)',
  'linear-gradient(135deg, #84cc16, #22c55e)',
  'linear-gradient(135deg, #e879f9, #c084fc)',
];

/** Maps an actor name to a deterministic gradient via simple string hash */
function getAvatarColor(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

/** Badge config per event type - colors per AC: PR=purple, Task=blue, Comment=gray, Deploy=green, Review=orange */
const EVENT_BADGES: Record<ActivityEventType, { label: string; bg: string }> = {
  'pr-completed': { label: 'PR', bg: '#9333ea' },
  'task-completed': { label: 'Task', bg: '#3b82f6' },
  comment: { label: 'Comment', bg: '#6b7280' },
  deployment: { label: 'Deploy', bg: '#16a34a' },
  review: { label: 'Review', bg: '#f97316' },
};

/** Human-readable relative timestamp from an ISO 8601 string */
function relativeTime(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60_000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.floor(hrs / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(iso).toLocaleDateString();
}

/** An event is "recent" if it occurred within the last 60 minutes */
function isRecent(iso: string): boolean {
  return Date.now() - new Date(iso).getTime() < 60 * 60 * 1000;
}

// ---- Framer Motion Variants ----

/** Staggered list container - 50ms between each child */
const listVariants = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.05 } },
};

/** Individual item: fade-in + slide from left */
const itemVariants = {
  hidden: { opacity: 0, x: -20 },
  visible: {
    opacity: 1,
    x: 0,
    transition: { duration: 0.3, ease: 'easeOut' },
  },
};

// ---- Sub-components ----

/** Single activity event row with avatar, badge, description, timestamp, and optional pulse dot */
function ActivityEventRow({ event }: { event: TeamActivityEvent }) {
  const badge = EVENT_BADGES[event.type as ActivityEventType] ?? {
    label: event.type,
    bg: '#6b7280',
  };
  const recent = isRecent(event.timestamp);
  const initials =
    event.actorAvatar ||
    event.actor
      .split(' ')
      .map((w) => w[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  const avatarBg = getAvatarColor(event.actor);

  return (
    <motion.div
      variants={itemVariants}
      style={{
        display: 'flex',
        alignItems: 'flex-start',
        gap: 10,
        padding: '10px 14px',
        borderBottom: '1px solid rgba(255,255,255,0.06)',
      }}
      {/* Avatar circle with initials */}
      <div style={{ position: 'relative', flexShrink: 0 }}>
        <div
          style={{
            width: 36,
            height: 36,
            borderRadius: '50%',
            background: avatarBg,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 11,
            fontWeight: 700,
            color: '#fff',
            textTransform: 'uppercase',
          }}
          {initials}
        </div>
        {/* Activity pulse dot for recent events (within 60 min) */}
        {recent && (
          <span
            style={{
              position: 'absolute',
              top: -2,
              right: -2,
              width: 10,
              height: 10,
              borderRadius: '50%',
              background: '#10b981',
              border: '2px solid rgba(0,0,0,0.6)',
              animation: 'activityPulseDot 1.5s ease-in-out infinite',
            }}
          />
        )}
      </div>

      {/* Content column: actor name + badge, description */}
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 2 }}>
          <span
            style={{
              fontSize: 13,
              fontWeight: 600,
              color: 'rgba(255,255,255,0.9)',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}
            {event.actor}
          </span>
          <span
            style={{
              fontSize: 9,
              fontWeight: 600,
              padding: '1px 6px',
              borderRadius: 3,
              background: badge.bg,
              color: '#fff',
              textTransform: 'uppercase',
              letterSpacing: 0.5,
              flexShrink: 0,
            }}
            {badge.label}
          </span>
        </div>
        <p
          style={{
            margin: 0,
            fontSize: 11.5,
            color: 'rgba(255,255,255,0.55)',
            lineHeight: 1.4,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
          {event.description}
        </p>
      </div>

      {/* Relative timestamp */}
      <span
        style={{
          fontSize: 10,
          color: 'rgba(255,255,255,0.35)',
          whiteSpace: 'nowrap',
          flexShrink: 0,
          paddingTop: 2,
        }}
        {relativeTime(event.timestamp)}
      </span>
    </motion.div>
  );
}

// ---- Main Component ----

/** Team Activity Feed - scrollable list of recent team events with Framer Motion entry animations */
export default function ActivityFeed() {
  const { data, error, isLoading } = useTeamActivity();
  const events = data?.events ?? [];

  return (
    <div
      className="glass-card"
      style={{
        position: 'fixed',
        bottom: 20,
        left: 20,
        width: 360,
        maxHeight: 500,
        zIndex: 10,
        pointerEvents: 'auto',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
        borderRadius: 16,
      }}
      {/* Header row */}
      <div
        style={{
          padding: '12px 16px',
          borderBottom: '1px solid rgba(255,255,255,0.08)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexShrink: 0,
        }}
        <h2
          style={{
            margin: 0,
            fontSize: '0.9rem',
            fontWeight: 600,
            color: 'var(--color-primary, #818cf8)',
          }}
          Team Activity
        </h2>
        {!isLoading && !error && (
          <span style={{ fontSize: 10, color: 'rgba(255,255,255,0.35)' }}>
            {events.length} events
          </span>
        )}
      </div>

      {/* Scrollable event list */}
      <div style={{ flex: 1, overflowY: 'auto', overflowX: 'hidden' }}>
        {/* Loading skeleton */}
        {isLoading && (
          <div style={{ padding: 14, display: 'flex', flexDirection: 'column', gap: 12 }}>
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
                <div
                  style={{
                    width: 36,
                    height: 36,
                    borderRadius: '50%',
                    background: 'rgba(255,255,255,0.06)',
                    animation: 'activityPulseDot 1.5s ease-in-out infinite',
                  }}
                />
                <div style={{ flex: 1 }}>
                  <div
                    style={{
                      width: '60%',
                      height: 10,
                      borderRadius: 4,
                      background: 'rgba(255,255,255,0.06)',
                      marginBottom: 6,
                    }}
                  />
                  <div
                    style={{
                      width: '80%',
                      height: 8,
                      borderRadius: 4,
                      background: 'rgba(255,255,255,0.04)',
                    }}
                  />
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Error state */}
        {error && (
          <div
            style={{
              padding: 16,
              color: '#f87171',
              fontSize: 13,
              textAlign: 'center',
            }}
            Failed to load activity feed
          </div>
        )}

        {/* Empty state */}
        {!isLoading && !error && events.length === 0 && (
          <div
            style={{
              padding: 16,
              color: 'rgba(255,255,255,0.4)',
              fontSize: 13,
              textAlign: 'center',
            }}
            No activity yet
          </div>
        )}

        {/* Animated event list */}
        <AnimatePresence>
          {events.length > 0 && (
            <motion.div variants={listVariants} initial="hidden" animate="visible">
              {events.map((evt) => (
                <ActivityEventRow key={evt.id} event={evt} />
              ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Pulse animation keyframes for the green recent-event dot */}
      <style>{`
        @keyframes activityPulseDot {
          0%, 100% { opacity: 1; transform: scale(1); }
          50% { opacity: 0.5; transform: scale(1.4); }
        }
      `}</style>
    </div>
  );
}