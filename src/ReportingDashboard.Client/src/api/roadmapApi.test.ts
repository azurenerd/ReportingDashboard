import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { fetchRoadmap, fetchWorkItems, triggerSync } from './roadmapApi';
import type { RoadmapData, WorkItemDto, SyncResult } from '../models/types';

function jsonResponse(body: unknown, status = 200, statusText = 'OK'): Response {
  return {
    ok: status >= 200 && status < 300,
    status,
    statusText,
    json: () => Promise.resolve(body),
    headers: new Headers({ 'content-type': 'application/json' }),
  } as Response;
}

const sampleRoadmap: RoadmapData = {
  workstreams: [{ id: 'M1', name: 'Chatbot & MS Role', color: '#0078D4', sortOrder: 1 }],
  milestones: [{
    id: 'm1-poc', workstreamId: 'M1', name: 'Mar 26 PoC',
    date: '2026-03-26', type: 'PoC', subType: 'Major',
  }],
  workItems: [{
    id: 'seed-001', title: 'Test item', status: 'InProgress',
    month: 'Apr', workstreamId: 'M1', adoUrl: 'https://dev.azure.com/test/1',
  }],
  months: [
    { name: 'Mar', isCurrent: false },
    { name: 'Apr', isCurrent: true },
  ],
  dateRange: { start: '2026-01-01', end: '2026-06-30' },
  lastSyncUtc: '2026-05-01T00:00:00Z',
};

const sampleWorkItems: WorkItemDto[] = [
  { id: '100', title: 'Item A', status: 'InProgress', month: 'Apr', adoUrl: 'https://dev.azure.com/test/100' },
  { id: '101', title: 'Item B', status: 'InProgress', month: 'Apr', adoUrl: 'https://dev.azure.com/test/101' },
];

const sampleSyncResult: SyncResult = {
  itemCount: 42,
  syncedAtUtc: '2026-05-01T06:15:00.0000000Z',
};

describe('roadmapApi', () => {
  let fetchSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchSpy = vi.fn();
    vi.stubGlobal('fetch', fetchSpy);
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('fetchRoadmap', () => {
    it('sends GET /api/roadmap and returns typed RoadmapData on 200', async () => {
      fetchSpy.mockResolvedValueOnce(jsonResponse(sampleRoadmap));

      const result = await fetchRoadmap();

      expect(fetchSpy).toHaveBeenCalledOnce();
      expect(fetchSpy).toHaveBeenCalledWith('/api/roadmap');
      expect(result).toEqual(sampleRoadmap);
      expect(result.workstreams).toHaveLength(1);
      expect(result.milestones[0].type).toBe('PoC');
    });

    it('throws an Error with descriptive message on 500', async () => {
      fetchSpy.mockResolvedValueOnce(
        jsonResponse({ error: 'Internal server error' }, 500, 'Internal Server Error'),
      );

      await expect(fetchRoadmap()).rejects.toThrow('Internal server error');
    });

    it('falls back to statusText when response has no error field', async () => {
      fetchSpy.mockResolvedValueOnce(
        jsonResponse({}, 503, 'Service Unavailable'),
      );

      await expect(fetchRoadmap()).rejects.toThrow('Service Unavailable');
    });

    it('falls back to statusText when response body is not valid JSON', async () => {
      const resp = {
        ok: false,
        status: 502,
        statusText: 'Bad Gateway',
        json: () => Promise.reject(new SyntaxError('Unexpected token')),
        headers: new Headers(),
      } as unknown as Response;
      fetchSpy.mockResolvedValueOnce(resp);

      await expect(fetchRoadmap()).rejects.toThrow('Bad Gateway');
    });

    it('returns empty arrays when database is empty', async () => {
      const emptyData: RoadmapData = {
        workstreams: [],
        milestones: [],
        workItems: [],
        months: [],
        dateRange: { start: '2026-01-01', end: '2026-06-30' },
        lastSyncUtc: null,
      };
      fetchSpy.mockResolvedValueOnce(jsonResponse(emptyData));

      const result = await fetchRoadmap();
      expect(result.workstreams).toHaveLength(0);
      expect(result.lastSyncUtc).toBeNull();
    });
  });

  describe('fetchWorkItems', () => {
    it('sends GET with correct query parameters and returns WorkItemDto[]', async () => {
      fetchSpy.mockResolvedValueOnce(jsonResponse(sampleWorkItems));

      const result = await fetchWorkItems('InProgress', 'Apr');

      expect(fetchSpy).toHaveBeenCalledOnce();
      const calledUrl = fetchSpy.mock.calls[0][0] as string;
      expect(calledUrl).toContain('/api/workitems');
      expect(calledUrl).toContain('status=InProgress');
      expect(calledUrl).toContain('month=Apr');
      expect(result).toEqual(sampleWorkItems);
      expect(result).toHaveLength(2);
    });

    it('URL-encodes special characters in parameters', async () => {
      fetchSpy.mockResolvedValueOnce(jsonResponse([]));

      await fetchWorkItems('In Progress', 'Apr');

      const calledUrl = fetchSpy.mock.calls[0][0] as string;
      expect(calledUrl).toContain('status=In+Progress');
    });

    it('throws with server error message on 400', async () => {
      fetchSpy.mockResolvedValueOnce(
        jsonResponse(
          { error: "Invalid status 'Doing'. Valid values: Shipped, InProgress, Carryover, Blocked" },
          400,
          'Bad Request',
        ),
      );

      await expect(fetchWorkItems('Doing', 'Apr')).rejects.toThrow(
        "Invalid status 'Doing'",
      );
    });

    it('returns empty array for cells with no items', async () => {
      fetchSpy.mockResolvedValueOnce(jsonResponse([]));

      const result = await fetchWorkItems('Blocked', 'Jun');
      expect(result).toEqual([]);
    });
  });

  describe('triggerSync', () => {
    it('sends POST /api/sync and returns SyncResult on 200', async () => {
      fetchSpy.mockResolvedValueOnce(jsonResponse(sampleSyncResult));

      const result = await triggerSync();

      expect(fetchSpy).toHaveBeenCalledOnce();
      expect(fetchSpy).toHaveBeenCalledWith('/api/sync', { method: 'POST' });
      expect(result).toEqual(sampleSyncResult);
      expect(result.itemCount).toBe(42);
    });

    it('uses POST method', async () => {
      fetchSpy.mockResolvedValueOnce(jsonResponse(sampleSyncResult));

      await triggerSync();

      const options = fetchSpy.mock.calls[0][1] as RequestInit;
      expect(options.method).toBe('POST');
    });

    it('throws with server error on 401 (expired PAT)', async () => {
      fetchSpy.mockResolvedValueOnce(
        jsonResponse(
          { error: "Invalid or expired ADO Personal Access Token. Generate a new PAT with 'Work Items (Read)' scope." },
          401,
          'Unauthorized',
        ),
      );

      await expect(triggerSync()).rejects.toThrow('Invalid or expired ADO Personal Access Token');
    });

    it('throws with server error on 400 (PAT not configured)', async () => {
      fetchSpy.mockResolvedValueOnce(
        jsonResponse(
          { error: 'ADO PAT not configured. Set via: dotnet user-secrets set "Ado:Pat" "<pat>"' },
          400,
          'Bad Request',
        ),
      );

      await expect(triggerSync()).rejects.toThrow('ADO PAT not configured');
    });

    it('throws with server error on 502 (ADO unreachable)', async () => {
      fetchSpy.mockResolvedValueOnce(
        jsonResponse(
          { error: 'Could not reach Azure DevOps. Check your network connection and VPN status.' },
          502,
          'Bad Gateway',
        ),
      );

      await expect(triggerSync()).rejects.toThrow('Could not reach Azure DevOps');
    });

    it('throws with server error on 500 (unexpected)', async () => {
      fetchSpy.mockResolvedValueOnce(
        jsonResponse(
          { error: 'Sync failed unexpectedly. Check logs at %LOCALAPPDATA%\\ReportingDashboard\\logs\\' },
          500,
          'Internal Server Error',
        ),
      );

      await expect(triggerSync()).rejects.toThrow('Sync failed unexpectedly');
    });
  });

  describe('network failures', () => {
    it('fetchRoadmap propagates network error', async () => {
      fetchSpy.mockRejectedValueOnce(new TypeError('Failed to fetch'));

      await expect(fetchRoadmap()).rejects.toThrow('Failed to fetch');
    });

    it('fetchWorkItems propagates network error', async () => {
      fetchSpy.mockRejectedValueOnce(new TypeError('Failed to fetch'));

      await expect(fetchWorkItems('Shipped', 'Mar')).rejects.toThrow('Failed to fetch');
    });

    it('triggerSync propagates network error', async () => {
      fetchSpy.mockRejectedValueOnce(new TypeError('Failed to fetch'));

      await expect(triggerSync()).rejects.toThrow('Failed to fetch');
    });
  });
});