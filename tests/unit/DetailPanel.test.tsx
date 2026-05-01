import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import React from 'react';

// Mock framer-motion
vi.mock('framer-motion', () => ({
  AnimatePresence: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  motion: {
    div: ({ children, ...props }: any) => {
      const { initial, animate, exit, transition, ...domProps } = props;
      return (
        <div data-testid="motion-div" {...domProps}>
          {children}
        </div>
      );
    },
  },
}));

// Mock the store
const mockClearSelection = vi.fn();
const mockStore = {
  selectedEntityId: null as string | null,
  focusTarget: null,
  setSelectedEntity: vi.fn(),
  setFocusTarget: vi.fn(),
  clearSelection: mockClearSelection,
};

vi.mock('../../client/src/store/dashboardStore', () => ({
  useDashboardStore: () => mockStore,
}));

// Mock the API
const mockGetReportDetail = vi.fn();
vi.mock('../../client/src/api/client', () => ({
  api: {
    getReportDetail: (...args: any[]) => mockGetReportDetail(...args),
  },
}));

import DetailPanel from '../../client/src/components/DetailPanel';

const mockReportDetail = {
  id: 'epic-001',
  title: 'User Authentication System',
  description: 'Implement OAuth2 authentication flow',
  type: 'epic',
  status: 'in-progress',
  priority: 'high',
  owner: 'Alice Smith',
  estimate: 21,
  remainingWork: 8,
  dependencies: ['epic-002', 'feat-003'],
  recentActivity: [
    { type: 'task-completed', description: 'Login form implemented', timestamp: new Date(Date.now() - 3600000).toISOString(), user: 'Bob' },
    { type: 'comment', description: 'Need to review token refresh', timestamp: new Date(Date.now() - 7200000).toISOString(), user: 'Carol' },
  ],
  metadata: { sprint: 'Sprint 5', createdAt: '2026-01-15T00:00:00Z' },
};

describe('DetailPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockStore.selectedEntityId = null;
    mockGetReportDetail.mockResolvedValue(mockReportDetail);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('does not render panel when selectedEntityId is null', () => {
    mockStore.selectedEntityId = null;
    const { container } = render(<DetailPanel />);
    expect(container.querySelector('[role="dialog"]')).not.toBeInTheDocument();
  });

  it('renders loading state when fetch is in-flight', async () => {
    mockStore.selectedEntityId = 'epic-001';
    mockGetReportDetail.mockReturnValue(new Promise(() => {}));

    render(<DetailPanel />);

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });
    const spinner = document.querySelector('.animate-spin');
    expect(spinner).toBeInTheDocument();
  });

  it('renders error state when fetch fails', async () => {
    mockStore.selectedEntityId = 'invalid-id';
    mockGetReportDetail.mockRejectedValue(new Error('Not found'));

    render(<DetailPanel />);

    await waitFor(() => {
      expect(screen.getByText(/not found/i)).toBeInTheDocument();
    });
  });

  it('calls clearSelection when close button is clicked', async () => {
    mockStore.selectedEntityId = 'epic-001';

    render(<DetailPanel />);

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    const closeButton = screen.getByLabelText('Close panel');
    fireEvent.click(closeButton);
    expect(mockClearSelection).toHaveBeenCalledTimes(1);
  });

  it('calls clearSelection when Escape key is pressed', async () => {
    mockStore.selectedEntityId = 'epic-001';

    render(<DetailPanel />);

    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeInTheDocument();
    });

    fireEvent.keyDown(document, { key: 'Escape' });
    expect(mockClearSelection).toHaveBeenCalledTimes(1);
  });
});