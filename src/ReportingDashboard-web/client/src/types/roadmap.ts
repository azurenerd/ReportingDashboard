export type MilestoneStatus = 'Completed' | 'Active' | 'Upcoming';
export type MilestoneType = 'Release' | 'Milestone' | 'SprintBoundary';

export interface RoadmapMilestone {
  id: string;
  title: string;
  type: MilestoneType;
  status: MilestoneStatus;
  date: string;
  description: string;
  deliverables: string[];
}