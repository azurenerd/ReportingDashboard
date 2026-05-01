/**
 * Typed fetch wrapper for the ReportingDashboard API.
 * All API calls route through this module for consistent error handling.
 * In development, Vite's proxy forwards /api/* to the Express backend on :3001.
 */

const BASE_URL = '/api';

/** API error shape returned by the Express backend. */
interface ApiErrorBody {
  error?: {
    code?: string;
    message?: string;
  };
}

/**
 * Custom error class carrying HTTP status and server error code.
 * Consumers can inspect `status` to differentiate 404 from 500, etc.
 */
export class ApiError extends Error {
  public readonly status: number;
  public readonly code: string;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
  }
}

/**
 * Generic typed GET request with structured error handling.
 * Throws ApiError on non-2xx responses so hooks can surface the message.
 */
export async function get<T>(path: string): Promise<T> {
  const url = `${BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;
  const res = await fetch(url);

  if (!res.ok) {
    const body: ApiErrorBody | null = await res.json().catch(() => null);
    const code = body?.error?.code ?? 'UNKNOWN';
    const message = body?.error?.message ?? `HTTP ${res.status}: ${res.statusText}`;
    throw new ApiError(res.status, code, message);
  }

  return res.json() as Promise<T>;
}

// ── Typed endpoint functions ──
// Each function targets a specific API endpoint and returns the expected shape.

export function fetchProjectSummary() {
  return get<import('../types').ProjectSummary>('/project-summary');
}

export function fetchProjectItems() {
  return get<{ items: import('../types').ProjectItem[] }>('/project-items');
}

export function fetchSprintMetrics() {
  return get<import('../types').SprintMetrics>('/sprint-metrics');
}

export function fetchRisks() {
  return get<{ risks: import('../types').Risk[] }>('/risks');
}

export function fetchTeamActivity() {
  return get<import('../types').TeamActivity>('/team-activity');
}

export function fetchRoadmap() {
  return get<import('../types').Roadmap>('/roadmap');
}

export function fetchReportDetail(id: string) {
  return get<import('../types').ReportDetail>(`/report/${encodeURIComponent(id)}`);
}