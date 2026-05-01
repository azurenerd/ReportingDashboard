import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';

// Mock framer-motion before component import
vi.mock('framer-motion', async () => {
  const { createElement } = await import('react');
  return {
    motion: {
      div: ({
        children,
        initial,
        animate,
        transition,
        exit,
        whileHover,
        whileTap,
        ...props
      }: any) => createElement('div', props, children),
    },
    AnimatePresence: ({ children }: any) => children,
  };
});

// Mock useProjectData hook
vi.mock('../../client/src/hooks/useProjectData', () => ({
  useProjectData: vi.fn(),
}));

import { useProjectData } from '../../client/src/hooks/useProjectData';
import ActivityFeed from '../../client/src/components/ActivityFeed';

const mockedUseProjectData = vi.mocked(useProjectData);

function createMockEvent(overrides: Record<string, unknown> = {}) {
  return {
    id: (overrides.id as string) ?? '1',
    type: (overrides.type as string) ?? 'comment',
    actorAvatar: (overrides.actorAvatar as string) ?? 'JD',
    description: (overrides.description as string) ?? 'Test event description',
    timestamp:
      (overrides.timestamp as string) ??
      new Date(Date.now() - 60_000).toISOString(),
  };
}

// Trait: Unit
describe('ActivityFeed', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-05-01T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.restoreAllMocks();
  });

  it('renders loading skeleton when data is loading', () => {
    mockedUseProjectData.mockReturnValue({
      data: {},
      loading: true,
      error: null,
    } as any);

    const { container } = render(<ActivityFeed />);

    // Loading skeleton renders 5 pulsing placeholder rows
    const skeletonContainer = container.querySelector('.space-y-3.p-3');
    expect(skeletonContainer).not.toBeNull();
    const pulseItems = skeletonContainer!.querySelectorAll('.animate-pulse');
    expect(pulseItems.length).toBe(5);

    // Header is still visible during loading
    expect(screen.getByText('Team Activity')).toBeDefined();
  });

  it('renders error message when API call fails', () => {
    mockedUseProjectData.mockReturnValue({
      data: {},
      loading: false,
      error: 'Network error',
    } as any);

    render(<ActivityFeed />);

    expect(screen.getByText('Failed to load activity feed')).toBeDefined();
  });

  it('renders events with avatar initials, descriptions, and relative timestamps', () => {
    const now = new Date('2026-05-01T12:00:00Z');
    const events = [
      createMockEvent({
        id: '1',
        actorAvatar: 'AB',
        description: 'Alice opened PR #10',
        type: 'pr-completed',
        timestamp: new Date(now.getTime() - 5 * 60 * 1000).toISOString(),
      }),
      createMockEvent({
        id: '2',
        actorAvatar: 'CD',
        description: 'Charlie deployed v2.0',
        type: 'deployment',
        timestamp: new Date(now.getTime() - 3 * 60 * 60 * 1000).toISOString(),
      }),
    ];

    mockedUseProjectData.mockReturnValue({
      data: { teamActivity: { events } },
      loading: false,
      error: null,
    } as any);

    render(<ActivityFeed />);

    // Avatar initials
    expect(screen.getByText('AB')).toBeDefined();
    expect(screen.getByText('CD')).toBeDefined();

    // Descriptions
    expect(screen.getByText('Alice opened PR #10')).toBeDefined();
    expect(screen.getByText('Charlie deployed v2.0')).toBeDefined();

    // Relative timestamps
    expect(screen.getByText('5m ago')).toBeDefined();
    expect(screen.getByText('3h ago')).toBeDefined();

    // Event count in header
    expect(screen.getByText('2 events')).toBeDefined();
  });

  it('displays correct badge label for each event type', () => {
    const now = new Date('2026-05-01T12:00:00Z');
    const ts = new Date(now.getTime() - 60_000).toISOString();
    const events = [
      createMockEvent({ id: '1', type: 'pr-completed', timestamp: ts }),
      createMockEvent({ id: '2', type: 'task-completed', timestamp: ts }),
      createMockEvent({ id: '3', type: 'comment', timestamp: ts }),
      createMockEvent({ id: '4', type: 'deployment', timestamp: ts }),
      createMockEvent({ id: '5', type: 'review', timestamp: ts }),
    ];

    mockedUseProjectData.mockReturnValue({
      data: { teamActivity: { events } },
      loading: false,
      error: null,
    } as any);

    render(<ActivityFeed />);

    expect(screen.getByText('PR')).toBeDefined();
    expect(screen.getByText('Task')).toBeDefined();
    expect(screen.getByText('Comment')).toBeDefined();
    expect(screen.getByText('Deploy')).toBeDefined();
    expect(screen.getByText('Review')).toBeDefined();
  });

  it('shows pulse dot indicator only on first 5 events', () => {
    const now = new Date('2026-05-01T12:00:00Z');
    const events = Array.from({ length: 7 }, (_, i) =>
      createMockEvent({
        id: String(i + 1),
        timestamp: new Date(now.getTime() - i * 60_000).toISOString(),
      }),
    );

    mockedUseProjectData.mockReturnValue({
      data: { teamActivity: { events } },
      loading: false,
      error: null,
    } as any);

    const { container } = render(<ActivityFeed />);

    // Only the first 5 events (RECENT_THRESHOLD) get animate-ping pulse dots
    const pulseDots = container.querySelectorAll('.animate-ping');
    expect(pulseDots.length).toBe(5);
  });
});