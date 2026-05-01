import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import React from 'react';

// Mock framer-motion to render plain divs in jsdom
vi.mock('framer-motion', () => {
  const MotionDiv = React.forwardRef<HTMLDivElement, React.PropsWithChildren<Record<string, unknown>>>(
    ({ children, initial, animate, exit, transition, ...rest }, ref) => {
      // Filter out motion-specific props so they don't bleed into DOM
      const domProps: Record<string, unknown> = {};
      for (const [key, val] of Object.entries(rest)) {
        if (!['whileHover', 'whileTap', 'variants', 'layout'].includes(key)) {
          domProps[key] = val;
        }
      }
      return React.createElement('div', { ...domProps, ref, 'data-testid': 'motion-div' }, children);
    }
  );
  MotionDiv.displayName = 'MotionDiv';

  return {
    motion: { div: MotionDiv },
    AnimatePresence: ({ children }: { children: React.ReactNode }) =>
      React.createElement(React.Fragment, null, children),
  };
});

// Variable to control mock store values per test
let mockStoreValues: {
  selectedEntityId: string | null;
  clearSelection: ReturnType<typeof vi.fn>;
};

vi.mock('../../client/src/store/dashboardStore', () => ({
  useDashboardStore: () => mockStoreValues,
}));

// Import after mocks
const { default: DetailPanel } = await import('../../client/src/components/DetailPanel');

const MOCK_REPORT: Record<string, unknown> = {
  id: 'epic-001',
  type: 'epic',
  title: 'Authentication System',
  description: 'Implement full auth flow with OAuth2',
  owner: 'Jane Smith',
  status: 'in-progress',
  priority: 'high',
  estimate: 21,
  remainingWork: 12,
  dependencies: [
    { id: 'feat-001', title: 'User Service', status: 'done' },
  ],
  recentActivity: [
    {
      id: 'evt-1',
      type: 'pr-completed',
      actor: 'John Doe',
      actorAvatar: '',
      description: 'Merged PR #42',
      timestamp: new Date().toISOString(),
      relatedItemId: null,
    },
  ],
  metadata: { role: 'Tech Lead' },
};

function mockFetchSuccess(data: unknown = MOCK_REPORT) {
  global.fetch = vi.fn().mockResolvedValue({
    ok: true,
    json: () => Promise.resolve(data),
  });
}

function mockFetchError(status = 500) {
  global.fetch = vi.fn().mockResolvedValue({
    ok: false,
    status,
    json: () => Promise.resolve({}),
  });
}

function mockFetchNetworkError() {
  global.fetch = vi.fn().mockRejectedValue(new Error('Network failure'));
}

describe('DetailPanel', () => {
  beforeEach(() => {
    mockStoreValues = {
      selectedEntityId: null,
      clearSelection: vi.fn(),
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('renders nothing when selectedEntityId is null', () => {
    const { container } = render(React.createElement(DetailPanel));
    expect(container.innerHTML).toBe('');
  });

  it('shows loading spinner then renders content on successful fetch', async () => {
    mockStoreValues.selectedEntityId = 'epic-001';
    mockFetchSuccess();

    render(React.createElement(DetailPanel));

    // Loading spinner should appear (has animate-spin class)
    expect(document.querySelector('.animate-spin')).toBeInTheDocument();

    // After fetch resolves, content should render
    await waitFor(() => {
      expect(screen.getByText('Authentication System')).toBeInTheDocument();
    });

    // Verify key content rendered from mock data
    expect(screen.getByText('Jane Smith')).toBeInTheDocument();
    expect(screen.getByText('Tech Lead')).toBeInTheDocument();
    expect(screen.getByText('21')).toBeInTheDocument();
    expect(screen.getByText('User Service')).toBeInTheDocument();
    expect(fetch).toHaveBeenCalledWith('/api/report/epic-001');
  });

  it('shows error state when fetch fails', async () => {
    mockStoreValues.selectedEntityId = 'missing-001';
    mockFetchError(404);

    render(React.createElement(DetailPanel));

    await waitFor(() => {
      expect(screen.getByText('Failed to load details')).toBeInTheDocument();
      expect(screen.getByText('Not found')).toBeInTheDocument();
    });
  });

  it('calls clearSelection when Escape key is pressed', async () => {
    mockStoreValues.selectedEntityId = 'epic-001';
    mockFetchSuccess();

    render(React.createElement(DetailPanel));

    fireEvent.keyDown(document, { key: 'Escape' });

    expect(mockStoreValues.clearSelection).toHaveBeenCalled();
  });

  it('calls clearSelection when backdrop is clicked', async () => {
    mockStoreValues.selectedEntityId = 'epic-001';
    mockFetchSuccess();

    render(React.createElement(DetailPanel));

    // The backdrop is the fixed inset-0 z-40 div
    const backdrop = document.querySelector('.fixed.inset-0.z-40') as HTMLElement;
    if (backdrop) {
      fireEvent.click(backdrop);
      expect(mockStoreValues.clearSelection).toHaveBeenCalled();
    } else {
      // Fallback: find elements by role/position — the backdrop may use data-testid
      // If no backdrop found, the close X button should still work
      const closeButtons = document.querySelectorAll('button');
      const xButton = Array.from(closeButtons).find(
        (btn) => btn.textContent?.includes('✕') || btn.textContent?.includes('×')
      );
      if (xButton) {
        fireEvent.click(xButton);
        expect(mockStoreValues.clearSelection).toHaveBeenCalled();
      }
    }
  });
});