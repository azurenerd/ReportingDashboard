import type { RoadmapData, WorkItemDto, SyncResult } from '../models/types';

async function extractErrorMessage(response: Response): Promise<string> {
  try {
    const body: unknown = await response.json();
    if (
      typeof body === 'object' &&
      body !== null &&
      'error' in body &&
      typeof (body as Record<string, unknown>).error === 'string'
    ) {
      return (body as Record<string, unknown>).error as string;
    }
  } catch {
    // JSON parsing failed — fall through to statusText
  }
  return response.statusText || `HTTP ${response.status}`;
}

export async function fetchRoadmap(): Promise<RoadmapData> {
  const response = await fetch('/api/roadmap');
  if (!response.ok) {
    const msg = await extractErrorMessage(response);
    throw new Error(msg);
  }
  return (await response.json()) as RoadmapData;
}

export async function fetchWorkItems(
  status: string,
  month: string,
): Promise<WorkItemDto[]> {
  const params = new URLSearchParams({ status, month });
  const response = await fetch(`/api/workitems?${params.toString()}`);
  if (!response.ok) {
    const msg = await extractErrorMessage(response);
    throw new Error(msg);
  }
  return (await response.json()) as WorkItemDto[];
}

export async function triggerSync(): Promise<SyncResult> {
  const response = await fetch('/api/sync', { method: 'POST' });
  if (!response.ok) {
    const msg = await extractErrorMessage(response);
    throw new Error(msg);
  }
  return (await response.json()) as SyncResult;
}