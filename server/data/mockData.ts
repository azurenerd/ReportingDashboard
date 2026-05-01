import type { AllMockData } from './types.js';

export function generateMockData(): AllMockData {
  return {
    projectSummary: {
      id: 'proj-001',
      name: 'Project Phoenix',
      status: 'In Progress',
      currentSprint: 'Sprint 14',
      completionPercent: 67,
      deliveryConfidence: 78,
      daysRemaining: 8,
      healthScore: 72,
      healthColor: 'yellow',
      totalEpics: 4,
      totalFeatures: 12,
      totalStories: 48,
    },
    projectItems: [],
    sprintMetrics: {
      sprintName: 'Sprint 14',
      sprintNumber: 14,
      startDate: '2026-04-20T00:00:00Z',
      endDate: '2026-05-03T00:00:00Z',
      velocity: { sprints: [], planned: [], completed: [] },
      burndown: { days: [], ideal: [], actual: [] },
      openBugs: 0,
      blockers: 0,
      carryoverItems: 0,
    },
    risks: [],
    teamActivity: { events: [], teamMembers: [] },
    roadmap: { milestones: [], sprintBoundaries: [] },
    itemIndex: new Map(),
  };
}