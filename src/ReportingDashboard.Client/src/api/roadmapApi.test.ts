import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { fetchRoadmap } from './roadmapApi';
import type { RoadmapData } from '../models/types';

describe('roadmapApi', () => {
  const originalFetch = globalThis.fetch;

  beforeEach(() => {
    globalThis.fetch = vi.fn();
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  describe('fetchRoadmap', () => {
    it('returns RoadmapData on 200 with valid JSON', async () => {
      const mockData: RoadmapData = {
        workstreams: [
          { id: 'M1', name: 'Core Platform', color: '#0078D4' },
        ],
        milestones: [
          {
            id: 'ms-1',
            workstreamId: 'M1',
            title: 'PoC Complete',
            date: '2026-02-15',
            type: 'poc',
          },
        ],
        workItems: [
          {
            id: 1001,
            title: 'Implement auth module',
            status: 'Shipped',
            month: 'Feb',
            adoUrl: 'https://dev.azure.com/org/project/_workitems/edit/1001',
          },
        ],
        months: [
          { name: 'Feb', isCurrent: false },
          { name: 'Mar', isCurrent: false },
          { name: 'Apr', isCurrent: true },
          { name: 'May', isCurrent: false },
        ],
        dateRange: { start: '2026-01-01', end: '2026-06-30' },
        lastSyncUtc: '2026-04-10T14:30:00Z',
      };

      (globalThis.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => mockData,
      });

      const result = await fetchRoadmap();

      expect(globalThis.fetch).toHaveBeenCalledWith('/api/roadmap');
      expect(result).toEqual(mockData);
      expect(result.workstreams).toHaveLength(1);
      expect(result.milestones).toHaveLength(1);
      expect(result.workItems).toHaveLength(1);
      expect(result.months).toHaveLength(4);
      expect(result.dateRange.start).toBe('2026-01-01');
      expect(result.lastSyncUtc).toBe('2026-04-10T14:30:00Z');
    });
  });
});