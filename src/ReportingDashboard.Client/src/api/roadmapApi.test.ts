import { describe, it, expect, vi, beforeEach } from 'vitest';
import { fetchRoadmap, fetchWorkItems, triggerSync } from './roadmapApi';
import type { RoadmapData, WorkItemDto, SyncResult } from '../models/types';

const mockRoadmapData: RoadmapData = {
  workstreams: [{ id: 'M1', name: 'Test', color: '#0078D4', sortOrder: 1 }],
  milestones: [],
  workItems: [],
  months: [{ name: 'Apr', isCurrent: true }],
  dateRange: { start: '2026-01-01', end: '2026-06-30' },
  lastSyncUtc: null,
};

beforeEach(() => {
  vi.restoreAllMocks();
});

describe('fetchRoadmap', () => {
  it('returns RoadmapData on 200', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockRoadmapData),
    }));

    const result = await fetchRoadmap();
    expect(result).toEqual(mockRoadmapData);
    expect(fetch).toHaveBeenCalledWith('/api/roadmap');
  });

  it('throws on non-200 response', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      statusText: 'Internal Server Error',
      json: () => Promise.resolve({ error: 'Something broke' }),
    }));

    await expect(fetchRoadmap()).rejects.toThrow('Something broke');
  });
});

describe('fetchWorkItems', () => {
  it('calls correct URL with encoded params', async () => {
    const items: WorkItemDto[] = [
      { id: '1', title: 'Item 1', status: 'InProgress', month: 'Apr', adoUrl: 'https://dev.azure.com/test' },
    ];
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(items),
    }));

    const result = await fetchWorkItems('InProgress', 'Apr');
    expect(result).toEqual(items);
    expect(fetch).toHaveBeenCalledWith('/api/workitems?status=InProgress&month=Apr');
  });

  it('throws with server error message on 400', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: false,
      status: 400,
      statusText: 'Bad Request',
      json: () => Promise.resolve({
        errors: {
          status: ["Invalid status 'Doing'. Valid values: Shipped, InProgress, Carryover, Blocked"],
        },
      }),
    }));

    await expect(fetchWorkItems('Doing', 'Apr')).rejects.toThrow(
      "Invalid status 'Doing'. Valid values: Shipped, InProgress, Carryover, Blocked"
    );
  });
});

describe('triggerSync', () => {
  it('returns SyncResult on 200', async () => {
    const syncResult: SyncResult = { itemCount: 42, syncedAtUtc: '2026-05-01T06:15:00Z' };
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(syncResult),
    }));

    const result = await triggerSync();
    expect(result).toEqual(syncResult);
    expect(fetch).toHaveBeenCalledWith('/api/sync', { method: 'POST' });
  });

  it('throws with server error on 401', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: false,
      status: 401,
      statusText: 'Unauthorized',
      json: () => Promise.resolve({ error: 'Invalid or expired ADO Personal Access Token.' }),
    }));

    await expect(triggerSync()).rejects.toThrow('Invalid or expired ADO Personal Access Token.');
  });

  it('throws with server error on 502', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: false,
      status: 502,
      statusText: 'Bad Gateway',
      json: () => Promise.resolve({ error: 'Could not reach Azure DevOps.' }),
    }));

    await expect(triggerSync()).rejects.toThrow('Could not reach Azure DevOps.');
  });

  it('falls back to statusText when JSON parsing fails', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      statusText: 'Internal Server Error',
      json: () => Promise.reject(new Error('not json')),
    }));

    await expect(triggerSync()).rejects.toThrow('Internal Server Error');
  });
});