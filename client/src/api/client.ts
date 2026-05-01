import type {
  ApiError,
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  TeamActivity,
  Roadmap,
  ReportDetail,
} from '../types';

const BASE_URL = '/api';

/** Custom error class carrying HTTP status and server error code */
export class FetchError extends Error {
  constructor(
    public status: number,
    public code: string,
    message: string,
  ) {
    super(message);
    this.name = 'FetchError';
  }
}

/**
 * Generic JSON fetcher with AbortSignal support.
 * Throws FetchError on non-2xx responses.
 */
async function fetchJson<T>(
  endpoint: string,
  options?: { signal?: AbortSignal },
): Promise<T> {
  const response = await fetch(`${BASE_URL}${endpoint}`, {
    signal: options?.signal,
  });

  if (!response.ok) {
    let code = 'UNKNOWN';
    let message = `Request failed with status ${response.status}`;
    try {
      const body: ApiError = await response.json();
      code = body.error.code;
      message = body.error.message;
    } catch {
      // Server returned non-JSON error body; use default message
    }
    throw new FetchError(response.status, code, message);
  }

  return response.json() as Promise<T>;
}

// Typed API client - one method per endpoint, all accept an optional AbortSignal
export const api = {
  getProjectSummary: (signal?: AbortSignal) =>
    fetchJson<ProjectSummary>('/project-summary', { signal }),

  getProjectItems: (signal?: AbortSignal) =>
    fetchJson<{ items: ProjectItem[] }>('/project-items', { signal }),

  getSprintMetrics: (signal?: AbortSignal) =>
    fetchJson<SprintMetrics>('/sprint-metrics', { signal }),

  getRisks: (signal?: AbortSignal) =>
    fetchJson<{ risks: Risk[] }>('/risks', { signal }),

  getTeamActivity: (signal?: AbortSignal) =>
    fetchJson<TeamActivity>('/team-activity', { signal }),

  getRoadmap: (signal?: AbortSignal) =>
    fetchJson<Roadmap>('/roadmap', { signal }),

  getReportDetail: (id: string, signal?: AbortSignal) =>
    fetchJson<ReportDetail>(`/report/${encodeURIComponent(id)}`, { signal }),
};

export default api;