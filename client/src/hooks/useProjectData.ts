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
}

const emptyData: DashboardData = {
  projectSummary: null,
  projectItems: [],
  sprintMetrics: null,
  risks: [],
  teamActivity: null,
  roadmap: null,
};

/** Fetches all dashboard data in parallel on mount. */
export function useProjectData(): UseProjectDataResult {
  const [data, setData] = useState<DashboardData>(emptyData);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchAll() {
      try {
        const [summary, items, sprint, risksRes, team, roadmapRes] = await Promise.all([
          get<ProjectSummary>('/project-summary'),
          get<{ items: ProjectItem[] }>('/project-items'),
          get<SprintMetrics>('/sprint-metrics'),
          get<{ risks: Risk[] }>('/risks'),
          get<TeamActivity>('/team-activity'),
          get<Roadmap>('/roadmap'),
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
          setError(err instanceof Error ? err.message : 'Failed to load data');
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