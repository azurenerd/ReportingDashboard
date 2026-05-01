import type { RoadmapData, WorkItemDto, SyncResult } from '../models/types';

async function extractErrorMessage(response: Response): Promise<string> {
    try {
        const body = await response.json();
        if (typeof body.error === 'string') {
            return body.error;
        }
        if (body.errors && typeof body.errors === 'object') {
            const messages = Object.values(body.errors).flat();
            if (messages.length > 0) {
                return messages.join('; ');
            }
        }
    } catch {
        // JSON parsing failed, fall through
    }
    return response.statusText || `HTTP ${response.status}`;
}

export async function fetchRoadmap(): Promise<RoadmapData> {
    const response = await fetch('/api/roadmap');
    if (!response.ok) {
        const message = await extractErrorMessage(response);
        throw new Error(message);
    }
    return (await response.json()) as RoadmapData;
}

export async function fetchWorkItems(status: string, month: string): Promise<WorkItemDto[]> {
    const params = new URLSearchParams({ status, month });
    const response = await fetch(`/api/workitems?${params.toString()}`);
    if (!response.ok) {
        const message = await extractErrorMessage(response);
        throw new Error(message);
    }
    return (await response.json()) as WorkItemDto[];
}

export async function triggerSync(): Promise<SyncResult> {
    const response = await fetch('/api/sync', { method: 'POST' });
    if (!response.ok) {
        const message = await extractErrorMessage(response);
        throw new Error(message);
    }
    return (await response.json()) as SyncResult;
}