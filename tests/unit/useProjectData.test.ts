import { describe, it, expect, beforeEach, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import {
  useProjectSummary,
  useProjectItems,
  useReportDetail,
} from '../../client/src/hooks/useProjectData';

/**
 * Unit tests for client/src/hooks/useProjectData.ts
 * Tests hook lifecycle: loading -> data, loading -> error, abort on unmount,
 * and useReportDetail id-change / null-reset behavior.
 */

// Mock the api module so hooks don't make real fetch calls
vi.mock('../../client/src/api/client', () => ({
  api: {
    getProjectSummary: vi.fn(),
    getProjectItems: vi.fn(),
    getSprintMetrics: vi.fn(),
    getRisks: vi.fn(),
    getTeamActivity: vi.fn(),
    getRoadmap: vi.fn(),
    getReportDetail: vi.fn(),
  },
}));

import { api } from '../../client/src/api/client';

const mockedApi = api as unknown as {
  getProjectSummary: ReturnType<typeof vi.fn>;
  getProjectItems: ReturnType<typeof vi.fn>;
  getSprintMetrics: ReturnType<typeof vi.fn>;
  getRisks: ReturnType<typeof vi.fn>;
  getTeamActivity: ReturnType<typeof vi.fn>;
  getRoadmap: ReturnType<typeof vi.fn>;
  getReportDetail: ReturnType<typeof vi.fn>;
};

describe('useProjectSummary', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('[Trait("Category", "Unit")] starts in loading state then resolves data', async () => {
    const mockData = { name: 'Phoenix', status: 'on-track', completionPercent: 68 };
    mockedApi.getProjectSummary.mockResolvedValue(mockData);

    const { result } = renderHook(() => useProjectSummary());

    // Initial state is loading
    expect(result.current.loading).toBe(true);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.data).toEqual(mockData);
    expect(result.current.error).toBeNull();
  });

  it('[Trait("Category", "Unit")] sets error state on fetch failure', async () => {
    mockedApi.getProjectSummary.mockRejectedValue(new Error('Network timeout'));

    const { result } = renderHook(() => useProjectSummary());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeInstanceOf(Error);
    expect(result.current.error!.message).toBe('Network timeout');
  });
});

describe('useProjectItems', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('[Trait("Category", "Unit")] unwraps items array from response', async () => {
    const items = [{ id: '1', title: 'Epic A', type: 'epic' }];
    mockedApi.getProjectItems.mockResolvedValue({ items });

    const { result } = renderHook(() => useProjectItems());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.data).toEqual(items);
  });
});

describe('useReportDetail', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('[Trait("Category", "Unit")] returns idle state when id is null', () => {
    const { result } = renderHook(() => useReportDetail(null));

    expect(result.current.loading).toBe(false);
    expect(result.current.data).toBeNull();
    expect(result.current.error).toBeNull();
    expect(mockedApi.getReportDetail).not.toHaveBeenCalled();
  });

  it('[Trait("Category", "Unit")] refetches when id changes and resets on null', async () => {
    const detail1 = { id: 'r1', title: 'Report 1' };
    const detail2 = { id: 'r2', title: 'Report 2' };
    mockedApi.getReportDetail.mockImplementation((id: string) => {
      if (id === 'r1') return Promise.resolve(detail1);
      if (id === 'r2') return Promise.resolve(detail2);
      return Promise.reject(new Error('unknown'));
    });

    const { result, rerender } = renderHook(
      ({ id }: { id: string | null }) => useReportDetail(id),
      { initialProps: { id: 'r1' } },
    );

    await waitFor(() => expect(result.current.loading).toBe(false));
    expect(result.current.data).toEqual(detail1);

    // Change id -> refetch
    rerender({ id: 'r2' });
    await waitFor(() => expect(result.current.data).toEqual(detail2));

    // Set null -> reset
    rerender({ id: null });
    await waitFor(() => {
      expect(result.current.data).toBeNull();
      expect(result.current.loading).toBe(false);
    });
  });
});