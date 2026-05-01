export interface RoadmapData {
    workstreams: Workstream[];
    milestones: Milestone[];
    workItems: WorkItem[];
    months: MonthColumn[];
    dateRange: { start: string; end: string };
    lastSyncUtc: string | null;
}

export interface Workstream {
    id: string;
    name: string;
    color: string;
    sortOrder: number;
}

export interface Milestone {
    id: string;
    workstreamId: string;
    name: string;
    date: string;
    type: 'PoC' | 'Production' | 'Checkpoint';
    subType: 'Major' | 'Minor';
}

export interface WorkItem {
    id: string;
    title: string;
    status: 'Shipped' | 'InProgress' | 'Carryover' | 'Blocked';
    month: string;
    workstreamId: string;
    adoUrl: string;
}

export interface MonthColumn {
    name: string;
    isCurrent: boolean;
}

export interface SyncResult {
    itemCount: number;
    syncedAtUtc: string;
}

export interface WorkItemDto {
    id: string;
    title: string;
    status: string;
    month: string;
    adoUrl: string;
}