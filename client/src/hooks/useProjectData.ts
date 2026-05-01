import { useState, useEffect } from 'react';
import { get } from '../api/client';
import type { ProjectSummary, ProjectItem, SprintMetrics, Risk, TeamActivity, Roadmap } from '@shared/types';

export interface DashboardData {
  projectSummary: ProjectSummary | null;
  projectItems: ProjectItem[];
  sprintMetrics: SprintMetrics | null;
  risks: Risk[];
  teamActivity: TeamActivity | null;
  roadmap: Roadmap | null;
}

export function useProjectData(): { data: DashboardData; loading: boolean; error: string | null } {
  const [data, setData] = useState<DashboardData>({ projectSummary: null, projectItems: [], sprintMetrics: null, risks: [], teamActivity: null, roadmap: null });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([
      get<ProjectSummary>('/api/project-summary'),
      get<{ items: ProjectItem[] }>('/api/project-items'),
      get<SprintMetrics>('/api/sprint-metrics'),
      get<{ risks: Risk[] }>('/api/risks'),
      get<TeamActivity>('/api/team-activity'),
      get<Roadmap>('/api/roadmap'),
    ]).then(([summary, items, sprint, risks, activity, roadmap]) => {
      setData({ projectSummary: summary, projectItems: items.items, sprintMetrics: sprint, risks: risks.risks, teamActivity: activity, roadmap });
    }).catch((err) => {
      setError(err instanceof Error ? err.message : 'Failed to fetch data');
    }).finally(() => setLoading(false));
  }, []);

  return { data, loading, error };
}