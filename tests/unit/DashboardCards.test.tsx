import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';

// Mock framer-motion to render plain divs so we can test content without animation overhead
vi.mock('framer-motion', () => ({
  motion: {
    div: ({ children, className, ...rest }: any) => (
      <div className={className} data-testid={rest['data-testid']}>
        {children}
      </div>
    ),
  },
}));

// Mock useAnimatedValue to return the target value instantly (no GSAP animation in tests)
vi.mock('../../client/src/hooks/useAnimatedValue', () => ({
  useAnimatedValue: (value: number) => value,
}));

// Mock the API client – each test configures its own resolved/rejected value
const mockGet = vi.fn();
vi.mock('../../client/src/api/client', () => ({
  get: (...args: any[]) => mockGet(...args),
}));

import DashboardCards from '../../client/src/components/DashboardCards';

/** Factory for a valid ProjectSummary payload matching the shape consumed by the component. */
function makeSummary(overrides: Record<string, any> = {}) {
  return {
    name: 'Project Alpha',
    status: 'In Progress',
    currentSprint: 'Sprint 7',
    totalEpics: 4,
    totalFeatures: 12,
    totalStories: 48,
    completionPercent: 63,
    deliveryConfidence: 78,
    daysRemaining: 5,
    healthScore: 85,
    healthColor: 'green' as const,
    ...overrides,
  };
}

describe('DashboardCards', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  /**
   * Tier 1 – Unit Tests
   */

  it('displays loading skeleton while data is being fetched', async () => {
    // Never resolve so the component stays in loading state
    mockGet.mockReturnValue(new Promise(() => {}));

    const { container } = render(<DashboardCards />);

    // LoadingSkeleton renders 5 pulse placeholders with animate-pulse class
    const pulseCards = container.querySelectorAll('.animate-pulse');
    expect(pulseCards.length).toBe(5);

    // Spinning loader inside each skeleton card
    const spinners = container.querySelectorAll('.animate-spin');
    expect(spinners.length).toBe(5);
  });

  it('shows error state with message when API call fails', async () => {
    mockGet.mockRejectedValue(new Error('Network timeout'));

    render(<DashboardCards />);

    await waitFor(() => {
      expect(screen.getByText('Failed to load project data')).toBeInTheDocument();
    });

    expect(screen.getByText('Network timeout')).toBeInTheDocument();
  });

  it('renders project name, status, sprint, and item counts after successful fetch', async () => {
    const summary = makeSummary();
    mockGet.mockResolvedValue(summary);

    render(<DashboardCards />);

    await waitFor(() => {
      expect(screen.getByText('Project Alpha')).toBeInTheDocument();
    });

    expect(screen.getByText('In Progress')).toBeInTheDocument();
    expect(screen.getByText('Sprint 7')).toBeInTheDocument();
    expect(screen.getByText('4 Epics')).toBeInTheDocument();
    expect(screen.getByText('12 Features')).toBeInTheDocument();
    expect(screen.getByText('48 Stories')).toBeInTheDocument();

    // Verify the API was called with the correct endpoint
    expect(mockGet).toHaveBeenCalledWith('/project-summary');
  });

  it('applies green health color classes when healthColor is green', async () => {
    mockGet.mockResolvedValue(makeSummary({ healthColor: 'green', healthScore: 90 }));

    const { container } = render(<DashboardCards />);

    await waitFor(() => {
      expect(screen.getByText('Health')).toBeInTheDocument();
    });

    // The health card should use glow-border-green (from healthGlowClass)
    const healthCard = screen.getByText('Health').closest('div.glass-card');
    expect(healthCard?.className).toContain('glow-border-green');

    // The dot should use health-dot-green (from healthDotClass)
    const dot = container.querySelector('.health-dot-green');
    expect(dot).toBeInTheDocument();
  });

  it('applies red health color classes when healthColor is red', async () => {
    mockGet.mockResolvedValue(makeSummary({ healthColor: 'red', healthScore: 35 }));

    const { container } = render(<DashboardCards />);

    await waitFor(() => {
      expect(screen.getByText('Health')).toBeInTheDocument();
    });

    const healthCard = screen.getByText('Health').closest('div.glass-card');
    expect(healthCard?.className).toContain('glow-border-red');

    const dot = container.querySelector('.health-dot-red');
    expect(dot).toBeInTheDocument();

    // Score text should use text-accent-red (from healthColorClass)
    const scoreSpan = container.querySelector('.text-accent-red');
    expect(scoreSpan).toBeInTheDocument();
  });
});