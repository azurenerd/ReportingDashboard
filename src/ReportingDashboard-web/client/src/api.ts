import type {
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  ActivityEvent,
  RoadmapMilestone,
  ReportDetail,
} from './types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:3001';

async function apiFetch<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`);
  if (!res.ok) throw new Error(`API ${path}: ${res.status}`);
  return res.json() as Promise<T>;
}

export const fetchProjectSummary = (): Promise<ProjectSummary> =>
  apiFetch<ProjectSummary>('/api/project-summary');

export const fetchProjectItems = (): Promise<ProjectItem[]> =>
  apiFetch<ProjectItem[]>('/api/project-items');

export const fetchSprintMetrics = (): Promise<SprintMetrics> =>
  apiFetch<SprintMetrics>('/api/sprint-metrics');

export const fetchRisks = (): Promise<Risk[]> =>
  apiFetch<Risk[]>('/api/risks');

export const fetchTeamActivity = (): Promise<ActivityEvent[]> =>
  apiFetch<ActivityEvent[]>('/api/team-activity');

export const fetchRoadmap = (): Promise<RoadmapMilestone[]> =>
  apiFetch<RoadmapMilestone[]>('/api/roadmap');

export const fetchReportDetail = (id: string): Promise<ReportDetail> =>
  apiFetch<ReportDetail>(`/api/report/${id}`);