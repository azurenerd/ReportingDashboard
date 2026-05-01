const projectSummary = {
  id: 'proj-001',
  name: 'Project Atlas',
  status: 'At Risk',
  completionPercentage: 67,
  deliveryConfidence: 'Medium',
  currentSprint: 'Sprint 14',
  sprintDaysRemaining: 4,
  healthScore: 72,
  totalEpics: 4,
  totalFeatures: 12,
  totalStories: 34,
  totalBugs: 8,
};

const projectItems = [
  { id: 'epic-001', title: 'User Authentication Platform', type: 'Epic', status: 'InProgress', parentId: null, assignee: 'Sarah Chen', priority: 'High', storyPoints: 89, remainingWork: 32, tags: ['security'] },
];

const sprintMetrics = {
  currentSprint: 'Sprint 14',
  velocity: 42,
  plannedPoints: 48,
  completedPoints: 31,
  burndown: [
    { day: 1, ideal: 48, actual: 48 },
    { day: 2, ideal: 43, actual: 45 },
  ],
  velocityHistory: [
    { sprint: 'Sprint 12', planned: 45, completed: 40 },
    { sprint: 'Sprint 13', planned: 50, completed: 44 },
  ],
  openBugs: 5,
  blockers: 2,
  carryoverItems: 3,
};

const risks = [
  { id: 'risk-001', title: 'API Performance Degradation', description: 'Response times increasing under load', severity: 'Critical', owner: 'Sarah Chen', status: 'Open', impact: 'User-facing latency exceeds SLA', mitigation: 'Implement caching layer', dateIdentified: '2026-04-15T00:00:00Z' },
];

const teamActivity = [
  { id: 'act-001', type: 'PR_Merged', description: 'Merged PR #142: Add caching middleware', member: { id: 'tm-001', name: 'Sarah Chen', role: 'Tech Lead', avatar: 'SC' }, timestamp: '2026-04-28T14:30:00Z', relatedItemId: 'epic-001' },
];

const roadmap = [
  { id: 'ms-001', title: 'v1.0 Beta Release', type: 'Release', status: 'Completed', date: '2026-03-15T00:00:00Z', description: 'Initial beta with core features', deliverables: ['Auth module', 'Dashboard MVP'] },
];

const reportDetails = {
  'epic-001': { id: 'epic-001', title: 'User Authentication Platform', type: 'Epic', description: 'Complete authentication and authorization platform.', owner: 'Sarah Chen', status: 'InProgress', priority: 'High', estimate: 89, remainingWork: 32, dependencies: ['Infrastructure Setup'], recentActivity: [{ date: '2026-04-28', action: 'Status changed to In Progress', actor: 'Sarah Chen' }], tags: ['security'], createdDate: '2026-01-10T00:00:00Z', updatedDate: '2026-04-28T00:00:00Z' },
  'risk-001': { id: 'risk-001', title: 'API Performance Degradation', type: 'Risk', description: 'Response times increasing under load.', owner: 'Sarah Chen', status: 'Open', priority: 'Critical', estimate: null, remainingWork: null, dependencies: [], recentActivity: [{ date: '2026-04-15', action: 'Risk identified', actor: 'Sarah Chen' }], tags: ['performance'], createdDate: '2026-04-15T00:00:00Z', updatedDate: '2026-04-15T00:00:00Z' },
};

module.exports = { projectSummary, projectItems, sprintMetrics, risks, teamActivity, roadmap, reportDetails };