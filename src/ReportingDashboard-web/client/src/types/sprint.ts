export interface BurndownPoint {
  day: number;
  ideal: number;
  actual: number;
}

export interface VelocitySprint {
  sprint: string;
  planned: number;
  completed: number;
}

export interface SprintMetrics {
  currentSprint: string;
  velocity: number;
  plannedPoints: number;
  completedPoints: number;
  burndown: BurndownPoint[];
  velocityHistory: VelocitySprint[];
  openBugs: number;
  blockers: number;
  carryoverItems: number;
}