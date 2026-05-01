import { useState, useEffect } from 'react';
import { get } from '../api/client';
import type {
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  TeamActivity,
  Roadmap,
} from '../types';

/**
 * Central data hook that fetches all dashboard data in parallel.
 * Returns a combined { data, loading, error } tuple consumed by App.tsx.
 */
export interface DashboardData {
  projectSummary: ProjectSummary | null;
  projectItems: ProjectItem[];
  sprintMetrics: SprintMetrics | null;
  risks: Risk[];
  teamActivity: TeamActivity | null;
  roadmap: Roadmap | null;
}

export function useProjectData() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchAll() {
      try {
        const [summary, items, sprint, risks, activity, roadmap] = await Promise.all([
          get<ProjectSummary>('/api/project-summary'),
          get<{ items: ProjectItem[] }>('/api/project-items'),
          get<SprintMetrics>('/api/sprint-metrics'),
          get<{ risks: Risk[] }>('/api/risks'),
          get<TeamActivity>('/api/team-activity'),
          get<Roadmap>('/api/roadmap'),
        ]);

        if (!cancelled) {
          setData({
            projectSummary: summary,
            projectItems: items.items,
            sprintMetrics: sprint,
            risks: risks.risks,
            teamActivity: activity,
            roadmap: roadmap,
          });
          setError(null);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err : new Error(String(err)));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchAll();
    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}