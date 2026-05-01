/**
 * Data-fetching hooks for the ReportingDashboard.
 *
 * Each hook follows the same pattern:
 *   - Returns { data, loading, error }
 *   - Fetches on mount (and on dependency change for useReportDetail)
 *   - Cancels in-flight requests if the component unmounts
 *
 * Also exports a combined useProjectData hook that fetches all endpoints
 * in parallel for components that need the full dashboard dataset.
 */

import { useState, useEffect, useCallback } from 'react';
import {
  fetchProjectSummary,
  fetchProjectItems,
  fetchSprintMetrics,
  fetchRisks,
  fetchTeamActivity,
  fetchRoadmap,
  fetchReportDetail,
} from '../api/client';
import type {
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  TeamActivity,
  Roadmap,
  ReportDetail,
  AsyncState,
} from '../types';

// ── Individual endpoint hooks ──

/** Fetches high-level project health indicators from GET /api/project-summary. */
export function useProjectSummary(): AsyncState<ProjectSummary | null> {
  const [data, setData] = useState<ProjectSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    fetchProjectSummary()
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load project summary');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}

/** Fetches the project item hierarchy from GET /api/project-items. */
export function useProjectItems(): AsyncState<ProjectItem[]> {
  const [data, setData] = useState<ProjectItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    fetchProjectItems()
      .then((result) => {
        if (!cancelled) setData(result.items);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load project items');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}

/** Fetches sprint velocity, burndown, and blocker data from GET /api/sprint-metrics. */
export function useSprintMetrics(): AsyncState<SprintMetrics | null> {
  const [data, setData] = useState<SprintMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    fetchSprintMetrics()
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load sprint metrics');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}

/** Fetches project risks and blockers from GET /api/risks. */
export function useRisks(): AsyncState<Risk[]> {
  const [data, setData] = useState<Risk[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    fetchRisks()
      .then((result) => {
        if (!cancelled) setData(result.risks);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load risks');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}

/** Fetches team activity events and members from GET /api/team-activity. */
export function useTeamActivity(): AsyncState<TeamActivity | null> {
  const [data, setData] = useState<TeamActivity | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    fetchTeamActivity()
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load team activity');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}

/** Fetches roadmap milestones and sprint boundaries from GET /api/roadmap. */
export function useRoadmap(): AsyncState<Roadmap | null> {
  const [data, setData] = useState<Roadmap | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    fetchRoadmap()
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load roadmap');
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}

/**
 * Fetches detailed report for a specific item from GET /api/report/:id.
 * Refetches automatically when `id` changes.
 * Returns null data and skips fetch when id is null/undefined (no item selected).
 */
export function useReportDetail(id: string | null | undefined): AsyncState<ReportDetail | null> {
  const [data, setData] = useState<ReportDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Reset state when id is cleared
    if (!id) {
      setData(null);
      setLoading(false);
      setError(null);
      return;
    }

    let cancelled = false;
    setLoading(true);
    setError(null);

    fetchReportDetail(id)
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .catch((err) => {
        if (!cancelled) {
          setData(null);
          setError(err instanceof Error ? err.message : 'Failed to load report detail');
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, [id]);

  return { data, loading, error };
}

// ── Combined dashboard hook ──

export interface DashboardData {
  projectSummary: ProjectSummary | null;
  projectItems: ProjectItem[];
  sprintMetrics: SprintMetrics | null;
  risks: Risk[];
  teamActivity: TeamActivity | null;
  roadmap: Roadmap | null;
}

interface UseProjectDataResult {
  data: DashboardData;
  loading: boolean;
  error: string | null;
  refetch: () => void;
}

const emptyData: DashboardData = {
  projectSummary: null,
  projectItems: [],
  sprintMetrics: null,
  risks: [],
  teamActivity: null,
  roadmap: null,
};

/** Fetches all dashboard data in parallel on mount. Provides a refetch callback. */
export function useProjectData(): UseProjectDataResult {
  const [data, setData] = useState<DashboardData>(emptyData);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [fetchCount, setFetchCount] = useState(0);

  const refetch = useCallback(() => {
    setFetchCount((c) => c + 1);
  }, []);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    async function fetchAll() {
      try {
        const [summary, items, sprint, risksRes, team, roadmapRes] = await Promise.all([
          fetchProjectSummary(),
          fetchProjectItems(),
          fetchSprintMetrics(),
          fetchRisks(),
          fetchTeamActivity(),
          fetchRoadmap(),
        ]);

        if (!cancelled) {
          setData({
            projectSummary: summary,
            projectItems: items.items,
            sprintMetrics: sprint,
            risks: risksRes.risks,
            teamActivity: team,
            roadmap: roadmapRes,
          });
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Failed to load dashboard data');
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchAll();
    return () => { cancelled = true; };
  }, [fetchCount]);

  return { data, loading, error, refetch };
}