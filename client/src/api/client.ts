/**
 * Typed API client for fetching dashboard data from the Express backend.
 * All endpoints are proxied through Vite dev server (/api/* → localhost:3001).
 */

import { useState, useEffect } from 'react';
import type {
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  TeamActivity,
  Roadmap,
  ReportDetail,
} from '../types';

/** Generic fetch wrapper with error handling */
async function get<T>(url: string): Promise<T> {
  const response = await fetch(url);
  if (!response.ok) {
    const errorData = await response.json().catch(() => null);
    throw new Error(
      errorData?.error?.message || `API error: ${response.status} ${response.statusText}`
    );
  }
  return response.json() as Promise<T>;
}

/** Generic SWR-like hook for data fetching */
function useApi<T>(url: string) {
  const [data, setData] = useState<T | undefined>(undefined);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    get<T>(url)
      .then((result) => {
        if (!cancelled) {
          setData(result);
          setError(null);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof Error ? err : new Error(String(err)));
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [url]);

  return { data, loading, error };
}

// Typed endpoint hooks
export function useProjectSummary() {
  return useApi<ProjectSummary>('/api/project-summary');
}

export function useProjectItems() {
  return useApi<{ items: ProjectItem[] }>('/api/project-items');
}

export function useSprintMetrics() {
  return useApi<SprintMetrics>('/api/sprint-metrics');
}

export function useRisks() {
  return useApi<{ risks: Risk[] }>('/api/risks');
}

export function useTeamActivity() {
  return useApi<TeamActivity>('/api/team-activity');
}

export function useRoadmap() {
  return useApi<Roadmap>('/api/roadmap');
}

export function useReportDetail(id: string | null) {
  const [data, setData] = useState<ReportDetail | undefined>(undefined);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!id) {
      setData(undefined);
      setLoading(false);
      return;
    }
    let cancelled = false;
    setLoading(true);
    get<ReportDetail>(`/api/report/${id}`)
      .then((result) => {
        if (!cancelled) {
          setData(result);
          setError(null);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof Error ? err : new Error(String(err)));
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => { cancelled = true; };
  }, [id]);

  return { data, loading, error };
}

export { get };