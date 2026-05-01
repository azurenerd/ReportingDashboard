import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useTeamActivity } from '../api/client';
import GlassCard from './ui/GlassCard';
import type { ActivityEvent, ActivityType } from '../types';

/** Badge configuration per activity event type */
const EVENT_BADGES: Record<ActivityType, { label: string; bg: string }> = {
  'pr-completed': { label: 'PR', bg: '#9333ea' },
  'task-completed': { label: 'Task', bg: '#16a34a' },
  'comment': { label: 'Comment', bg: '#2563eb' },
  'deployment': { label: 'Deploy', bg: '#ea580c' },
  'review': { label: 'Review', bg: '#0891b2' },
};

/** Returns human-readable relative timestamp */
function relativeTime(iso: string): string {
  const diff = Date.now() - new Date(iso).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'just now';
  if (mins < 60) return `${mins}m ago`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `${hrs}h ago`;
  const days = Math.floor(hrs / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(iso).toLocaleDateString();
}

/** An event is "recent" if it occurred within the last 10 minutes */
function isRecent(iso: string): boolean {
  return Date.now() - new Date(iso).getTime() < 10 * 60 * 1000;
}

/** Framer Motion variants for staggered list entry */
const listVariants = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.04 } },
};

const itemVariants = {
  hidden: { opacity: 0, x: -24 },
  visible: { opacity: 1, x: 0, transition: { duration: 0.35, ease: 'easeOut' } },
};

function ActivityItem({ event }: { event: ActivityEvent }) {
  const badge = EVENT_BADGES[event.type] ?? { label: event.type, bg: '#6b7280' };
  const recent = isRecent(event.timestamp);
  const initials = event.actor.avatar || event.actor.name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase();

  return (
    <motion.div
      variants={itemVariants}
      style={{
        display: 'flex',
        alignItems: 'flex-start',
        gap: 10,
        padding: '10px 12px',
        borderBottom: '1px solid rgba(255,255,255,0.06)',
      }}
    >
      {/* Avatar circle with initials + pulse dot */}
      <div style={{ position: 'relative', flexShrink: 0 }}>
        <div
          style={{
            width: 34,
            height: 34,
            borderRadius: '50%',
            background: 'linear-gradient(135deg, #6366f1, #a855f7)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 11,
            fontWeight: 700,
            color: '#fff',
            textTransform: 'uppercase',
          }}
        >
          {initials}
        </div>
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
              animation: 'pulse-dot 1.5s ease-in-out infinite',
            }}
          />
        )}
      </div>

      {/* Content */}
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 2 }}>
          <span style={{ fontSize: 13, fontWeight: 600, color: 'rgba(255,255,255,0.9)' }}>
            {event.actor.name}
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
            }}
          >
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
        >
          {event.action} {event.target}
        </p>
      </div>

      {/* Relative timestamp */}
      <span style={{ fontSize: 10, color: 'rgba(255,255,255,0.35)', whiteSpace: 'nowrap', flexShrink: 0, paddingTop: 2 }}>
        {relativeTime(event.timestamp)}
      </span>
    </motion.div>
  );
}

export default function TeamActivity() {
  const { data, error, isLoading } = useTeamActivity();
  const events = data?.events ?? [];

  return (
    <GlassCard
      className="team-activity"
      style={{
        position: 'fixed',
        bottom: 20,
        left: 20,
        width: 340,
        maxHeight: 340,
        zIndex: 10,
        pointerEvents: 'auto',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}
    >
      {/* Header */}
      <div
        style={{
          padding: '10px 12px',
          borderBottom: '1px solid rgba(255,255,255,0.08)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          flexShrink: 0,
        }}
      >
        <h2 style={{ margin: 0, fontSize: '0.9rem', color: 'var(--color-primary, #818cf8)' }}>
          Team Activity
        </h2>
        <span style={{ fontSize: 10, color: 'rgba(255,255,255,0.35)' }}>
          {events.length} events
        </span>
      </div>

      {/* Scrollable list */}
      <div style={{ flex: 1, overflowY: 'auto', overflowX: 'hidden' }}>
        {isLoading && (
          <p style={{ padding: 12, color: 'rgba(255,255,255,0.5)', fontSize: 13 }}>
            Loading activity...
          </p>
        )}
        {error && (
          <p style={{ padding: 12, color: '#f87171', fontSize: 13 }}>
            Failed to load activity feed
          </p>
        )}
        {!isLoading && !error && events.length === 0 && (
          <p style={{ padding: 12, color: 'rgba(255,255,255,0.4)', fontSize: 13 }}>
            No activity yet
          </p>
        )}
        <AnimatePresence>
          {events.length > 0 && (
            <motion.div variants={listVariants} initial="hidden" animate="visible">
              {events.map((evt) => (
                <ActivityItem key={evt.id} event={evt} />
              ))}
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Inline keyframes for pulse animation */}
      <style>{`
        @keyframes pulse-dot {
          0%, 100% { opacity: 1; transform: scale(1); }
          50% { opacity: 0.5; transform: scale(1.4); }
        }
      `}</style>
    </GlassCard>
  );
}