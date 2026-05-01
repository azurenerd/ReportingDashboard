export type ItemStatus = 'done' | 'in-progress' | 'blocked' | 'not-started' | 'at-risk';
export type ItemType = 'epic' | 'feature' | 'story' | 'task' | 'bug';
export type RiskSeverity = 'critical' | 'high' | 'medium' | 'low';
export type ActivityEventType = 'pr-completed' | 'task-completed' | 'comment' | 'deployment' | 'review';

export interface ProjectSummary {
  id: string;
  name: string;
  status: string;
  currentSprint: string;
  completionPercent: number;
  deliveryConfidence: number;
  daysRemaining: number;
  healthScore: number;
  healthColor: 'green' | 'yellow' | 'red';
  totalEpics: number;
  totalFeatures: number;
  totalStories: number;
}

export interface ProjectItem {
  id: string;
  type: ItemType;
  title: string;
  description: string;
  status: ItemStatus;
  parentId: string | null;
  owner: string;
  priority: 'critical' | 'high' | 'medium' | 'low';
  storyPoints: number;
  remainingWork: number;
  dependencies: string[];
  recentActivity: ActivityEvent[];
}

export interface SprintMetrics {
  sprintName: string;
  sprintNumber: number;
  startDate: string;
  endDate: string;
  velocity: {
    sprints: string[];
    planned: number[];
    completed: number[];
  };
  burndown: {
    days: string[];
    ideal: number[];
    actual: (number | null)[];
  };
  openBugs: number;
  blockers: number;
  carryoverItems: number;
}

export interface Risk {
  id: string;
  title: string;
  description: string;
  severity: RiskSeverity;
  owner: string;
  status: 'open' | 'mitigated' | 'closed';
  category: string;
  impactArea: string;
  mitigationPlan: string;
}

export interface ActivityEvent {
  id: string;
  type: ActivityEventType;
  actor: string;
  actorAvatar: string;
  description: string;
  timestamp: string;
  relatedItemId: string | null;
}

export interface TeamMember {
  id: string;
  name: string;
  role: string;
  avatar: string;
}

export interface TeamActivity {
  events: ActivityEvent[];
  teamMembers: TeamMember[];
}

export interface RoadmapMilestone {
  id: string;
  title: string;
  date: string;
  phase: 'completed' | 'active' | 'upcoming';
  type: 'release' | 'milestone' | 'sprint-boundary';
  description: string;
}

export interface Roadmap {
  milestones: RoadmapMilestone[];
  sprintBoundaries: { date: string; label: string }[];
}

export interface ReportDetail {
  id: string;
  type: ItemType | 'risk';
  title: string;
  description: string;
  owner: string;
  status: string;
  priority: string;
  estimate: number | null;
  remainingWork: number | null;
  dependencies: { id: string; title: string; status: string }[];
  recentActivity: ActivityEvent[];
  metadata: Record<string, string>;
}

// API error response shape (matches server error envelope)
export interface ApiError {
  error: {
    code: string;
    message: string;
  };
}

// Generic async state returned by all data hooks
export interface AsyncState<T> {
  data: T | null;
  loading: boolean;
  error: Error | null;
}