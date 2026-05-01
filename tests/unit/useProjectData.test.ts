import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';

vi.mock('../../client/src/api/client', () => ({
  fetchProjectSummary: vi.fn(),
  fetchProjectItems: vi.fn(),
  fetchSprintMetrics: vi.fn(),
  fetchRisks: vi.fn(),
  fetchTeamActivity: vi.fn(),
  fetchRoadmap: vi.fn(),
  fetchReportDetail: vi.fn(),
  ApiError: class ApiError extends Error {
    status: number;
    code: string;
    constructor(s: number, c: string, m: string) {
      super(m);
      this.status = s;
      this.code = c;
    }
  },
}));

import {
  fetchProjectSummary,
  fetchProjectItems,
  fetchReportDetail,
} from '../../client/src/api/client';
import {
  useProjectSummary,
  useProjectItems,
  useReportDetail,
} from '../../client/src/hooks/useProjectData';

beforeEach(() => {
  vi.resetAllMocks();
});

describe('useProjectSummary', () => {
  it('sets data after successful fetch', async () => {
    const mockSummary = {
      name: 'Project Phoenix',
      status: 'Active',
      completion: 72,
      currentSprint: 'Sprint 7',
    };
    vi.mocked(fetchProjectSummary).mockResolvedValue(mockSummary as never);

    const { result } = renderHook(() => useProjectSummary());
    expect(result.current.loading).toBe(true);

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.data).toEqual(mockSummary);
    expect(result.current.error).toBeNull();
  });
});

describe('useProjectItems', () => {
  it('extracts items array from wrapped response and sets error on failure', async () => {
    vi.mocked(fetchProjectItems).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useProjectItems());

    await waitFor(() => expect(result.current.loading).toBe(false));

    expect(result.current.error).toBe('Network error');
    expect(result.current.data).toEqual([]);
  });
});

describe('useReportDetail', () => {
  it('skips fetch and returns null when id is null', () => {
    const { result } = renderHook(() => useReportDetail(null));

    expect(result.current.data).toBeNull();
    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
    expect(fetchReportDetail).not.toHaveBeenCalled();
  });
});