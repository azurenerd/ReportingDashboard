import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';

// Mock framer-motion to render plain elements
vi.mock('framer-motion', () => {
  const React = require('react');
  return {
    motion: new Proxy(
      {},
      {
        get: (_target: unknown, prop: string) => {
          return React.forwardRef((props: Record<string, unknown>, ref: React.Ref<HTMLElement>) => {
            const {
              initial: _i,
              animate: _a,
              exit: _e,
              transition: _t,
              variants: _v,
              whileHover: _wh,
              whileTap: _wt,
              ...rest
            } = props;
            return React.createElement(prop, { ...rest, ref });
          });
        },
      }
    ),
    AnimatePresence: ({ children }: { children: React.ReactNode }) => children,
  };
});

// Mock useTeamActivity hook
const mockUseTeamActivity = vi.fn();
vi.mock('../../client/src/api/client', () => ({
  useTeamActivity: () => mockUseTeamActivity(),
}));

import ActivityFeed from '../../client/src/components/ActivityFeed';
import type { ActivityEvent } from '../../client/src/types';

function makeEvent(overrides: Partial<ActivityEvent> & { type: ActivityEvent['type'] }): ActivityEvent {
  return {
    id: overrides.id ?? 'evt-1',
    actor: overrides.actor ?? 'Jane Doe',
    actorAvatar: overrides.actorAvatar ?? 'JD',
    type: overrides.type,
    description: overrides.description ?? 'Did something',
    timestamp: overrides.timestamp ?? new Date().toISOString(),
    ...(overrides as Record<string, unknown>),
  } as ActivityEvent;
}

describe('ActivityFeed', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-05-01T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it('renders loading skeleton when loading is true', () => {
    mockUseTeamActivity.mockReturnValue({ data: null, loading: true, error: null });

    const { container } = render(<ActivityFeed />);

    // LoadingSkeleton renders 6 pulsing rows with animate-pulse class
    const pulsingRows = container.querySelectorAll('.animate-pulse');
    expect(pulsingRows.length).toBe(6);
  });

  it('renders error state when error is truthy', () => {
    mockUseTeamActivity.mockReturnValue({
      data: null,
      loading: false,
      error: 'Network failure',
    });

    render(<ActivityFeed />);

    expect(screen.getByText('Failed to load activity')).toBeInTheDocument();
    expect(screen.getByText('Network failure')).toBeInTheDocument();
  });

  it('renders event rows with correct badge labels for all 5 event types', () => {
    const events: ActivityEvent[] = [
      makeEvent({ id: '1', type: 'pr-completed', description: 'Merged PR #42', timestamp: '2026-05-01T10:00:00Z' }),
      makeEvent({ id: '2', type: 'task-completed', description: 'Finished task', timestamp: '2026-05-01T10:00:00Z' }),
      makeEvent({ id: '3', type: 'comment', description: 'Left a comment', timestamp: '2026-05-01T10:00:00Z' }),
      makeEvent({ id: '4', type: 'deployment', description: 'Deployed v1.2', timestamp: '2026-05-01T10:00:00Z' }),
      makeEvent({ id: '5', type: 'review', description: 'Reviewed code', timestamp: '2026-05-01T10:00:00Z' }),
    ];

    mockUseTeamActivity.mockReturnValue({
      data: { events },
      loading: false,
      error: null,
    });

    render(<ActivityFeed />);

    expect(screen.getByText('PR')).toBeInTheDocument();
    expect(screen.getByText('Task')).toBeInTheDocument();
    expect(screen.getByText('Comment')).toBeInTheDocument();
    expect(screen.getByText('Deploy')).toBeInTheDocument();
    expect(screen.getByText('Review')).toBeInTheDocument();
  });

  it('shows pulse dot for recent events and not for old events', () => {
    // Recent: 10 minutes ago (within 30-min threshold)
    const recentTimestamp = new Date('2026-05-01T11:50:00Z').toISOString();
    // Old: 2 hours ago (outside 30-min threshold)
    const oldTimestamp = new Date('2026-05-01T10:00:00Z').toISOString();

    const events: ActivityEvent[] = [
      makeEvent({ id: 'recent', type: 'comment', actorAvatar: 'RC', description: 'Recent event', timestamp: recentTimestamp }),
      makeEvent({ id: 'old', type: 'deployment', actorAvatar: 'OE', description: 'Old event', timestamp: oldTimestamp }),
    ];

    mockUseTeamActivity.mockReturnValue({
      data: { events },
      loading: false,
      error: null,
    });

    const { container } = render(<ActivityFeed />);

    // The pulse dot uses animate-ping class; only the recent event should have one
    const pingDots = container.querySelectorAll('.animate-ping');
    expect(pingDots.length).toBe(1);
  });

  it('renders empty state when events array is empty', () => {
    mockUseTeamActivity.mockReturnValue({
      data: { events: [] },
      loading: false,
      error: null,
    });

    render(<ActivityFeed />);

    // Component should show a fallback message for no events
    expect(screen.getByText(/no recent activity/i)).toBeInTheDocument();
  });
});