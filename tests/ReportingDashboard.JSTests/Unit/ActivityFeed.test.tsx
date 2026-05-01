/**
 * Unit tests for ActivityFeed component
 * Trait: Category = Unit
 */
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import React from 'react';

// Mock framer-motion to render children without animation
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, ...props }: any) => <div {...props}>{children}</div>,
  },
}));

// Mock useProjectData hook
const mockUseProjectData = vi.fn();
vi.mock('../hooks/useProjectData', () => ({
  useProjectData: () => mockUseProjectData(),
}));

// We need to import the component after mocks are set up
// Since ActivityFeed uses non-exported internal functions, we test through rendered output
import ActivityFeed from '../../../../client/src/components/ActivityFeed';

// Helper to create mock activity events
function createMockEvent(overrides: Partial<any> = {}) {
  return {
    id: overrides.id ?? 'evt-1',
    actorAvatar: overrides.actorAvatar ?? 'JD',
    description: overrides.description ?? 'Merged PR #42: Fix login bug',
    type: overrides.type ?? 'pr-completed',
    timestamp: overrides.timestamp ?? new Date(Date.now() - 3600000).toISOString(), // 1h ago
    ...overrides,
  };
}

describe('ActivityFeed', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-05-01T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.clearAllMocks();
  });

  // Category: Unit
  it('renders loading skeleton when data is loading', () => {
    mockUseProjectData.mockReturnValue({
      data: {},
      loading: true,
      error: null,
    });

    render(<ActivityFeed />);

    // LoadingSkeleton renders 5 pulse placeholders
    const pulseElements = document.querySelectorAll('.animate-pulse');
    expect(pulseElements.length).toBe(5);

    // Should show header
    expect(screen.getByText('Team Activity')).toBeInTheDocument();

    // Should show 0 events count when loading
    expect(screen.getByText('0 events')).toBeInTheDocument();
  });

  // Category: Unit
  it('renders error state when API call fails', () => {
    mockUseProjectData.mockReturnValue({
      data: {},
      loading: false,
      error: new Error('Network failure'),
    });

    render(<ActivityFeed />);

    expect(screen.getByText('Failed to load activity feed')).toBeInTheDocument();
  });

  // Category: Unit
  it('renders events with correct badge labels for each event type', () => {
    const events = [
      createMockEvent({ id: '1', type: 'pr-completed', description: 'PR merged' }),
      createMockEvent({ id: '2', type: 'task-completed', description: 'Task done' }),
      createMockEvent({ id: '3', type: 'comment', description: 'Comment posted' }),
      createMockEvent({ id: '4', type: 'deployment', description: 'Deployed to prod' }),
      createMockEvent({ id: '5', type: 'review', description: 'Code reviewed' }),
    ];

    mockUseProjectData.mockReturnValue({
      data: { teamActivity: { events } },
      loading: false,
      error: null,
    });

    render(<ActivityFeed />);

    // Badge labels from badgeConfig
    expect(screen.getByText('PR')).toBeInTheDocument();
    expect(screen.getByText('Task')).toBeInTheDocument();
    expect(screen.getByText('Comment')).toBeInTheDocument();
    expect(screen.getByText('Deploy')).toBeInTheDocument();
    expect(screen.getByText('Review')).toBeInTheDocument();

    // Event count in header
    expect(screen.getByText('5 events')).toBeInTheDocument();
  });

  // Category: Unit
  it('renders avatar initials and displays relative timestamps', () => {
    const events = [
      createMockEvent({
        id: '1',
        actorAvatar: 'AB',
        description: 'Did something',
        timestamp: new Date(Date.now() - 2 * 3600000).toISOString(), // 2h ago
      }),
      createMockEvent({
        id: '2',
        actorAvatar: 'XY',
        description: 'Did another thing',
        timestamp: new Date(Date.now() - 3 * 86400000).toISOString(), // 3d ago
      }),
    ];

    mockUseProjectData.mockReturnValue({
      data: { teamActivity: { events } },
      loading: false,
      error: null,
    });

    render(<ActivityFeed />);

    // Avatar initials rendered
    expect(screen.getByText('AB')).toBeInTheDocument();
    expect(screen.getByText('XY')).toBeInTheDocument();

    // Relative timestamps
    expect(screen.getByText('2h ago')).toBeInTheDocument();
    expect(screen.getByText('3d ago')).toBeInTheDocument();
  });

  // Category: Unit
  it('shows pulse dots only for first 5 (RECENT_THRESHOLD) events', () => {
    const events = Array.from({ length: 8 }, (_, i) =>
      createMockEvent({
        id: `evt-${i}`,
        actorAvatar: `U${i}`,
        description: `Event ${i}`,
        type: 'comment',
      })
    );

    mockUseProjectData.mockReturnValue({
      data: { teamActivity: { events } },
      loading: false,
      error: null,
    });

    const { container } = render(<ActivityFeed />);

    // Pulse dots use animate-ping class - should be exactly 5 (RECENT_THRESHOLD)
    const pingElements = container.querySelectorAll('.animate-ping');
    expect(pingElements.length).toBe(5);
  });
});