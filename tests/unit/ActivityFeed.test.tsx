import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';

// Mock framer-motion before importing the component
vi.mock('framer-motion', () => {
  const React = require('react');
  const createMotionComponent = (tag: string) => {
    return React.forwardRef((props: any, ref: any) => {
      const { initial, animate, variants, transition, whileHover, whileTap, ...rest } = props;
      return React.createElement(tag, { ...rest, ref });
    });
  };
  return {
    motion: new Proxy({}, {
      get: (_target: any, prop: string) => createMotionComponent(prop),
    }),
    AnimatePresence: ({ children }: any) => children,
  };
});

// Mock the useTeamActivity hook
const mockUseTeamActivity = vi.fn();
vi.mock('../../client/src/hooks/useProjectData', () => ({
  useTeamActivity: (...args: any[]) => mockUseTeamActivity(...args),
}));

import ActivityFeed from '../../client/src/components/ActivityFeed';
import type { ActivityEvent } from '../../client/src/types';

// Helper to create a mock event
function createMockEvent(overrides: Partial<ActivityEvent> = {}): ActivityEvent {
  return {
    id: overrides.id ?? 'evt-1',
    actor: overrides.actor ?? 'Alice Johnson',
    actorAvatar: overrides.actorAvatar ?? 'AJ',
    type: overrides.type ?? 'pr-completed',
    description: overrides.description ?? 'Merged PR #42: Fix dashboard layout',
    timestamp: overrides.timestamp ?? new Date(Date.now() - 3600_000).toISOString(), // 1h ago
    ...(overrides as any),
  };
}

describe('ActivityFeed', () => {
  let dateSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    vi.clearAllMocks();
    // Pin Date.now so relative timestamps are deterministic
    dateSpy = vi.spyOn(Date, 'now').mockReturnValue(new Date('2026-05-01T14:00:00Z').getTime());
  });

  afterEach(() => {
    dateSpy.mockRestore();
  });

  describe('[Unit]', () => {
    it('renders loading skeleton when data is loading', () => {
      mockUseTeamActivity.mockReturnValue({ data: null, loading: true, error: null });

      render(<ActivityFeed />);

      // Header should say "Team Activity"
      expect(screen.getByText('Team Activity')).toBeInTheDocument();
      // 4 skeleton placeholder rows (pulse-animated divs)
      const pulseContainers = document.querySelectorAll('.animate-pulse');
      expect(pulseContainers.length).toBe(4);
    });

    it('renders error message when API call fails', () => {
      mockUseTeamActivity.mockReturnValue({
        data: null,
        loading: false,
        error: 'Failed to load activity feed',
      });

      render(<ActivityFeed />);

      expect(screen.getByText('Team Activity')).toBeInTheDocument();
      const errorEl = screen.getByText('Failed to load activity feed');
      expect(errorEl).toBeInTheDocument();
      expect(errorEl).toHaveClass('text-red-400');
    });

    it('renders events with actor names, badges, and event count', () => {
      const events: ActivityEvent[] = [
        createMockEvent({
          id: 'e1',
          actor: 'Bob Smith',
          actorAvatar: 'BS',
          type: 'task-completed',
          description: 'Completed task: Setup CI pipeline',
          timestamp: new Date('2026-05-01T12:00:00Z').toISOString(), // 2h ago
        }),
        createMockEvent({
          id: 'e2',
          actor: 'Carol Danvers',
          actorAvatar: 'CD',
          type: 'deployment',
          description: 'Deployed v2.1.0 to production',
          timestamp: new Date('2026-05-01T11:00:00Z').toISOString(), // 3h ago
        }),
      ];

      mockUseTeamActivity.mockReturnValue({
        data: { events },
        loading: false,
        error: null,
      });

      render(<ActivityFeed />);

      // Actor names rendered
      expect(screen.getByText('Bob Smith')).toBeInTheDocument();
      expect(screen.getByText('Carol Danvers')).toBeInTheDocument();

      // Initials rendered in avatar circles
      expect(screen.getByText('BS')).toBeInTheDocument();
      expect(screen.getByText('CD')).toBeInTheDocument();

      // Badge labels rendered (Task for task-completed, Deploy for deployment)
      expect(screen.getByText('Task')).toBeInTheDocument();
      expect(screen.getByText('Deploy')).toBeInTheDocument();

      // Event count in header
      expect(screen.getByText('2 events')).toBeInTheDocument();

      // Relative timestamps: 2h ago, 3h ago
      expect(screen.getByText('2h ago')).toBeInTheDocument();
      expect(screen.getByText('3h ago')).toBeInTheDocument();
    });

    it('shows empty state when events array is empty', () => {
      mockUseTeamActivity.mockReturnValue({
        data: { events: [] },
        loading: false,
        error: null,
      });

      render(<ActivityFeed />);

      expect(screen.getByText('No activity yet.')).toBeInTheDocument();
      expect(screen.getByText('0 events')).toBeInTheDocument();
    });

    it('shows pulse dot only for events less than 5 minutes old', () => {
      const recentTimestamp = new Date('2026-05-01T13:57:00Z').toISOString(); // 3 min ago → recent
      const oldTimestamp = new Date('2026-05-01T10:00:00Z').toISOString();    // 4h ago → not recent

      const events: ActivityEvent[] = [
        createMockEvent({
          id: 'recent-1',
          actor: 'Recent User',
          actorAvatar: 'RU',
          type: 'comment',
          timestamp: recentTimestamp,
        }),
        createMockEvent({
          id: 'old-1',
          actor: 'Old User',
          actorAvatar: 'OU',
          type: 'review',
          timestamp: oldTimestamp,
        }),
      ];

      mockUseTeamActivity.mockReturnValue({
        data: { events },
        loading: false,
        error: null,
      });

      render(<ActivityFeed />);

      // Recent event's avatar container should have a pulse dot (animate-ping span)
      const pingSpans = document.querySelectorAll('.animate-ping');
      expect(pingSpans.length).toBe(1);

      // Verify the recent event shows "just now" or "3m ago"
      expect(screen.getByText('3m ago')).toBeInTheDocument();
      // Old event shows "4h ago"
      expect(screen.getByText('4h ago')).toBeInTheDocument();
    });
  });
});