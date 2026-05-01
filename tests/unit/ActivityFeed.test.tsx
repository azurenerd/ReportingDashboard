import React from 'react';
import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

// Mock framer-motion to avoid animation issues in tests
vi.mock('framer-motion', () => ({
  motion: {
    div: React.forwardRef(({ children, variants, initial, animate, exit, ...props }: any, ref: any) => (
      <div ref={ref} data-testid="motion-div" {...props}>{children}</div>
    )),
  },
  AnimatePresence: ({ children }: any) => <>{children}</>,
}));

// Mock the useTeamActivity hook
const mockUseTeamActivity = vi.fn();
vi.mock('../hooks/useTeamActivity', () => ({
  useTeamActivity: () => mockUseTeamActivity(),
}));

// We need to adjust the mock path for the actual import resolution
vi.mock('@/hooks/useTeamActivity', () => ({
  useTeamActivity: () => mockUseTeamActivity(),
}));

// Import after mocks are set up
import ActivityFeed from '../../client/src/components/ActivityFeed';

describe('ActivityFeed', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-05-01T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
    vi.clearAllMocks();
  });

  describe('[Unit] Loading state', () => {
    it('renders loading skeleton when data is loading', () => {
      mockUseTeamActivity.mockReturnValue({
        data: null,
        error: null,
        isLoading: true,
      });

      const { container } = render(<ActivityFeed />);

      // Should show the "Team Activity" heading
      expect(screen.getByText('Team Activity')).toBeInTheDocument();
      // Should not show event count when loading
      expect(screen.queryByText(/events$/)).not.toBeInTheDocument();
      // Should have loading skeleton divs with pulse animation
      const pulseElements = container.querySelectorAll('[style*="animation"]');
      expect(pulseElements.length).toBeGreaterThan(0);
    });
  });

  describe('[Unit] Error state', () => {
    it('renders error message when API call fails', () => {
      mockUseTeamActivity.mockReturnValue({
        data: null,
        error: new Error('Network error'),
        isLoading: false,
      });

      render(<ActivityFeed />);

      expect(screen.getByText('Team Activity')).toBeInTheDocument();
      // The component should display an error indicator
      expect(screen.getByText(/failed to load/i)).toBeInTheDocument();
    });
  });

  describe('[Unit] Event rendering', () => {
    const mockEvents = [
      {
        id: '1',
        actor: 'John Doe',
        actorAvatar: '',
        type: 'pr-completed',
        description: 'Merged PR #42: Add login feature',
        timestamp: '2026-05-01T11:30:00Z', // 30 min ago - recent
      },
      {
        id: '2',
        actor: 'Jane Smith',
        actorAvatar: '',
        type: 'task-completed',
        description: 'Completed task: Setup CI pipeline',
        timestamp: '2026-05-01T09:00:00Z', // 3 hours ago - not recent
      },
      {
        id: '3',
        actor: 'Bob Wilson',
        actorAvatar: 'BW',
        type: 'deployment',
        description: 'Deployed v2.1.0 to production',
        timestamp: '2026-04-30T12:00:00Z', // 1 day ago
      },
    ];

    beforeEach(() => {
      mockUseTeamActivity.mockReturnValue({
        data: { events: mockEvents },
        error: null,
        isLoading: false,
      });
    });

    it('renders all event rows with actor names and descriptions', () => {
      render(<ActivityFeed />);

      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('Jane Smith')).toBeInTheDocument();
      expect(screen.getByText('Bob Wilson')).toBeInTheDocument();

      expect(screen.getByText('Merged PR #42: Add login feature')).toBeInTheDocument();
      expect(screen.getByText('Completed task: Setup CI pipeline')).toBeInTheDocument();
      expect(screen.getByText('Deployed v2.1.0 to production')).toBeInTheDocument();
    });

    it('displays correct event type badge labels', () => {
      render(<ActivityFeed />);

      // PR badge for pr-completed
      expect(screen.getByText('PR')).toBeInTheDocument();
      // Task badge for task-completed
      expect(screen.getByText('Task')).toBeInTheDocument();
      // Deploy badge for deployment
      expect(screen.getByText('Deploy')).toBeInTheDocument();
    });

    it('shows event count in header', () => {
      render(<ActivityFeed />);

      expect(screen.getByText('3 events')).toBeInTheDocument();
    });

    it('displays relative timestamps correctly', () => {
      render(<ActivityFeed />);

      // 30 min ago
      expect(screen.getByText('30m ago')).toBeInTheDocument();
      // 3 hours ago
      expect(screen.getByText('3h ago')).toBeInTheDocument();
      // 1 day ago
      expect(screen.getByText('1d ago')).toBeInTheDocument();
    });
  });
});