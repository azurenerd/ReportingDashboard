export type ItemStatus = 'Done' | 'InProgress' | 'Blocked' | 'NotStarted' | 'AtRisk';
export type ItemType = 'Epic' | 'Feature' | 'Story' | 'Task' | 'Bug';

export interface ProjectSummary {
  id: string;
  name: string;
  status: 'On Track' | 'At Risk' | 'Off Track';
  completionPercentage: number;
  deliveryConfidence: 'High' | 'Medium' | 'Low';
  currentSprint: string;
  sprintDaysRemaining: number;
  healthScore: number;
  totalEpics: number;
  totalFeatures: number;
  totalStories: number;
  totalBugs: number;
}

export interface ProjectItem {
  id: string;
  title: string;
  type: ItemType;
  status: ItemStatus;
  parentId: string | null;
  assignee: string;
  priority: 'Critical' | 'High' | 'Medium' | 'Low';
  storyPoints: number;
  remainingWork: number;
  tags: string[];
}