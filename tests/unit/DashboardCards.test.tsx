import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import type { ProjectSummary } from '../../client/src/types';

// Mock framer-motion to render plain divs
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, className, ...props }: any) => (
      <div className={className} data-testid={props['data-testid']}>
        {children}
      </div>
    ),
  },
}));

// Mock useAnimatedValue to return the target value immediately
vi.mock('../../client/src/hooks/useAnimatedValue', () => ({
  useAnimatedValue: (value: number) => value,
}));

// Mock the API client
const mockGet = vi.fn();
vi.mock('../../client/src/api/client', () => ({
  get: (...args: any[]) => mockGet(...args),
}));

// Import the component after mocks are set up
import DashboardCards from '../../client/src/components/DashboardCards';

const mockProjectSummary: ProjectSummary = {
  name: 'ReportingDashboard',
  status: 'In Progress',
  currentSprint: 'Sprint 4',
  completionPercent: 67,
  deliveryConfidence: 82,
  daysRemaining: 8,
  healthScore: 85,
  healthColor: 'green',
  totalEpics: 4,
  totalFeatures: 12,
  totalStories: 48,
};

describe('DashboardCards', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('[Unit] Loading State', () => {
    it('renders loading skeleton with 5 pulsing cards while data is being fetched', () => {
      // Never resolve the promise to keep loading state
      mockGet.mockReturnValue(new Promise(() => {}));

      const { container } = render(<DashboardCards />);

      const pulsingCards = container.querySelectorAll('.animate-pulse');
      expect(pulsingCards).toHaveLength(5);

      // Each skeleton card should have the spinner element
      const spinners = container.querySelectorAll('.animate-spin');
      expect(spinners).toHaveLength(5);
    });
  });

  describe('[Unit] Successful Data Rendering', () => {
    it('renders project name, status, and sprint after successful fetch', async () => {
      mockGet.mockResolvedValue(mockProjectSummary);

      render(<DashboardCards />);

      await waitFor(() => {
        expect(screen.getByText('ReportingDashboard')).toBeInTheDocument();
      });

      expect(screen.getByText('In Progress')).toBeInTheDocument();
      expect(screen.getByText('Sprint 4')).toBeInTheDocument();
      expect(screen.getByText('4 Epics')).toBeInTheDocument();
      expect(screen.getByText('12 Features')).toBeInTheDocument();
      expect(screen.getByText('48 Stories')).toBeInTheDocument();
    });

    it('renders all metric cards with correct values and suffixes', async () => {
      mockGet.mockResolvedValue(mockProjectSummary);

      render(<DashboardCards />);

      await waitFor(() => {
        expect(screen.getByText('Completion')).toBeInTheDocument();
      });

      // Completion percentage with % suffix
      expect(screen.getByText('67%')).toBeInTheDocument();
      // Delivery confidence with % suffix
      expect(screen.getByText('82%')).toBeInTheDocument();
      // Days remaining (no suffix)
      expect(screen.getByText('8')).toBeInTheDocument();
      // Health score (no suffix)
      expect(screen.getByText('85')).toBeInTheDocument();
      // Labels
      expect(screen.getByText('Confidence')).toBeInTheDocument();
      expect(screen.getByText('Days Left')).toBeInTheDocument();
      expect(screen.getByText('Health')).toBeInTheDocument();
    });
  });

  describe('[Unit] Error State', () => {
    it('displays error message when API call fails', async () => {
      mockGet.mockRejectedValue(new Error('Network timeout'));

      render(<DashboardCards />);

      await waitFor(() => {
        expect(screen.getByText('Failed to load project data')).toBeInTheDocument();
      });

      expect(screen.getByText('Network timeout')).toBeInTheDocument();
    });

    it('displays "Unknown error" for non-Error thrown values', async () => {
      mockGet.mockRejectedValue('some string error');

      render(<DashboardCards />);

      await waitFor(() => {
        expect(screen.getByText('Unknown error')).toBeInTheDocument();
      });
    });
  });

  describe('[Unit] Health Color Coding', () => {
    it('applies green glow and text classes when healthColor is green', async () => {
      mockGet.mockResolvedValue({ ...mockProjectSummary, healthColor: 'green' });

      const { container } = render(<DashboardCards />);

      await waitFor(() => {
        expect(screen.getByText('Health')).toBeInTheDocument();
      });

      // The health metric card should use green glow border
      const healthCard = container.querySelector('.glow-border-green');
      expect(healthCard).toBeInTheDocument();

      // The health value should use green text color
      const greenText = container.querySelector('.text-accent-green');
      expect(greenText).toBeInTheDocument();
    });

    it('applies orange/red classes for yellow and red healthColor', async () => {
      // Test yellow (maps to orange classes)
      mockGet.mockResolvedValue({ ...mockProjectSummary, healthScore: 65, healthColor: 'yellow' });

      const { container, unmount } = render(<DashboardCards />);

      await waitFor(() => {
        expect(screen.getByText('Health')).toBeInTheDocument();
      });

      expect(container.querySelector('.glow-border-orange')).toBeInTheDocument();
      expect(container.querySelector('.text-accent-orange')).toBeInTheDocument();

      unmount();

      // Test red
      mockGet.mockResolvedValue({ ...mockProjectSummary, healthScore: 35, healthColor: 'red' });

      const { container: container2 } = render(<DashboardCards />);

      await waitFor(() => {
        expect(screen.getByText('Health')).toBeInTheDocument();
      });

      expect(container2.querySelector('.glow-border-red')).toBeInTheDocument();
      expect(container2.querySelector('.text-accent-red')).toBeInTheDocument();
    });
  });
});