import type {
  AllMockData,
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  TeamActivity,
  ActivityEvent,
  TeamMember,
  Roadmap,
  RoadmapMilestone,
  ReportDetail,
  ItemStatus,
  ItemType,
} from './types.js';
import { stableId } from './seed.js';

// ── Team members ──

const teamMembers: TeamMember[] = [
  { id: 'tm-001', name: 'Sarah Chen', role: 'Senior Engineer', avatar: 'SC' },
  { id: 'tm-002', name: 'Mike Torres', role: 'Tech Lead', avatar: 'MT' },
  { id: 'tm-003', name: 'Alex Kim', role: 'Frontend Engineer', avatar: 'AK' },
  { id: 'tm-004', name: 'Priya Patel', role: 'Backend Engineer', avatar: 'PP' },
  { id: 'tm-005', name: 'Jordan Lee', role: 'DevOps Engineer', avatar: 'JL' },
  { id: 'tm-006', name: 'Emma Wilson', role: 'QA Engineer', avatar: 'EW' },
  { id: 'tm-007', name: 'David Park', role: 'Product Designer', avatar: 'DP' },
  { id: 'tm-008', name: 'Lisa Zhang', role: 'Senior Engineer', avatar: 'LZ' },
  { id: 'tm-009', name: 'Ryan Murphy', role: 'Junior Engineer', avatar: 'RM' },
  { id: 'tm-010', name: 'Nina Gupta', role: 'Engineering Manager', avatar: 'NG' },
];

// ── Project summary ──

const projectSummary: ProjectSummary = {
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
};

// ── Helper to build items ──

function makeItem(
  id: string,
  type: ItemType,
  title: string,
  status: ItemStatus,
  parentId: string | null,
  owner: string,
  points: number,
): ProjectItem {
  return {
    id,
    type,
    title,
    description: `Description for ${title}`,
    status,
    parentId,
    owner,
    priority: 'medium',
    storyPoints: points,
    remainingWork: status === 'done' ? 0 : Math.round(points * 1.5),
    dependencies: [],
    recentActivity: [],
  };
}

// ── Project items hierarchy ──

const epicDefs: { title: string; status: ItemStatus; owner: string }[] = [
  { title: 'User Authentication Platform', status: 'in-progress', owner: 'Sarah Chen' },
  { title: 'Payment Processing Engine', status: 'in-progress', owner: 'Mike Torres' },
  { title: 'Analytics Dashboard', status: 'not-started', owner: 'Alex Kim' },
  { title: 'Notification Service', status: 'done', owner: 'Priya Patel' },
];

const statuses: ItemStatus[] = ['done', 'in-progress', 'blocked', 'not-started', 'at-risk'];
const projectItems: ProjectItem[] = [];
let storyIdx = 1;

epicDefs.forEach((epic, ei) => {
  const epicId = stableId('epic', ei + 1);
  projectItems.push(makeItem(epicId, 'epic', epic.title, epic.status, null, epic.owner, 89));

  for (let fi = 0; fi < 3; fi++) {
    const featId = stableId('feat', ei * 3 + fi + 1);
    projectItems.push(
      makeItem(
        featId,
        'feature',
        `Feature ${ei * 3 + fi + 1}: ${epic.title} - Module ${fi + 1}`,
        statuses[(ei + fi) % statuses.length],
        epicId,
        teamMembers[(ei + fi) % teamMembers.length].name,
        21,
      ),
    );

    const storiesPerFeature = fi === 0 ? 4 : 3;
    for (let si = 0; si < storiesPerFeature; si++) {
      const storyId = stableId('story', storyIdx++);
      projectItems.push(
        makeItem(
          storyId,
          si % 4 === 3 ? 'task' : 'story',
          `Story ${storyIdx - 1}: Implementation task`,
          statuses[(ei + fi + si) % statuses.length],
          featId,
          teamMembers[(ei + fi + si) % teamMembers.length].name,
          5,
        ),
      );
    }
  }
});

// ── Sprint metrics ──

const sprintMetrics: SprintMetrics = {
  sprintName: 'Sprint 14',
  sprintNumber: 14,
  startDate: '2026-04-20T00:00:00Z',
  endDate: '2026-05-03T00:00:00Z',
  velocity: {
    sprints: ['S10', 'S11', 'S12', 'S13', 'S14'],
    planned: [34, 40, 38, 42, 40],
    completed: [31, 38, 35, 40, 28],
  },
  burndown: {
    days: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5', 'Day 6', 'Day 7', 'Day 8', 'Day 9', 'Day 10'],
    ideal: [40, 36, 32, 28, 24, 20, 16, 12, 8, 0],
    actual: [40, 38, 35, 33, 29, 25, 22, null, null, null],
  },
  openBugs: 7,
  blockers: 3,
  carryoverItems: 2,
};

// ── Risks ──

const risks: Risk[] = [
  { id: 'risk-001', title: 'Third-party API deprecation', description: 'Payment provider v2 API sunset in Q3', severity: 'critical', owner: 'Mike Torres', status: 'open', category: 'Technical', impactArea: 'Revenue Pipeline', mitigationPlan: 'Migration to v3 API scheduled for Sprint 16' },
  { id: 'risk-002', title: 'Key engineer availability', description: 'Lead architect on leave during Sprint 15', severity: 'high', owner: 'Nina Gupta', status: 'open', category: 'Resource', impactArea: 'Architecture', mitigationPlan: 'Cross-train backup engineer this sprint' },
  { id: 'risk-003', title: 'Performance regression', description: 'Dashboard load time increased 40% after last release', severity: 'high', owner: 'Sarah Chen', status: 'mitigated', category: 'Technical', impactArea: 'User Experience', mitigationPlan: 'Performance profiling and optimization PR merged' },
  { id: 'risk-004', title: 'Scope creep on notifications', description: 'Stakeholders requesting real-time push notifications', severity: 'medium', owner: 'David Park', status: 'open', category: 'Scope', impactArea: 'Timeline', mitigationPlan: 'Defer to Phase 2; document in backlog' },
  { id: 'risk-005', title: 'Security audit findings', description: 'Pending security review may surface blocking issues', severity: 'high', owner: 'Jordan Lee', status: 'open', category: 'Security', impactArea: 'Release', mitigationPlan: 'Pre-audit checklist completed; remediations budgeted' },
  { id: 'risk-006', title: 'CI pipeline instability', description: 'Flaky tests causing false build failures', severity: 'medium', owner: 'Emma Wilson', status: 'mitigated', category: 'Process', impactArea: 'Velocity', mitigationPlan: 'Quarantined flaky tests; retry logic added' },
  { id: 'risk-007', title: 'Database migration complexity', description: 'Schema change requires zero-downtime migration', severity: 'low', owner: 'Priya Patel', status: 'open', category: 'Technical', impactArea: 'Data Integrity', mitigationPlan: 'Blue-green deployment strategy documented' },
  { id: 'risk-008', title: 'Browser compatibility gaps', description: 'Safari WebGL2 rendering differences', severity: 'low', owner: 'Alex Kim', status: 'open', category: 'Technical', impactArea: 'User Experience', mitigationPlan: 'Graceful degradation for unsupported features' },
];

// ── Activity events ──

const activityEvents: ActivityEvent[] = Array.from({ length: 22 }, (_, i) => ({
  id: stableId('evt', i + 1),
  type: (['pr-completed', 'task-completed', 'comment', 'deployment', 'review'] as const)[i % 5],
  actor: teamMembers[i % teamMembers.length].name,
  actorAvatar: teamMembers[i % teamMembers.length].avatar,
  description: [
    'Merged PR #247: Implement OAuth2 refresh flow',
    'Completed task: Database schema migration script',
    'Commented on Story-012: Need API contract clarification',
    'Deployed v2.4.1 to staging environment',
    'Approved PR #251: Add input validation middleware',
  ][i % 5],
  timestamp: new Date(Date.now() - i * 3600000).toISOString(),
  relatedItemId: i % 3 === 0 ? stableId('story', (i % 40) + 1) : null,
}));

// ── Roadmap ──

const roadmap: Roadmap = {
  milestones: [
    { id: 'ms-001', title: 'Alpha Release', date: '2026-02-15T00:00:00Z', phase: 'completed', type: 'release', description: 'Internal alpha with core features' },
    { id: 'ms-002', title: 'Beta Release', date: '2026-03-15T00:00:00Z', phase: 'completed', type: 'release', description: 'External beta with partner integrations' },
    { id: 'ms-003', title: 'Security Audit', date: '2026-04-01T00:00:00Z', phase: 'completed', type: 'milestone', description: 'Third-party security assessment' },
    { id: 'ms-004', title: 'Performance Milestone', date: '2026-04-20T00:00:00Z', phase: 'active', type: 'milestone', description: 'Sub-second load time target' },
    { id: 'ms-005', title: 'RC1', date: '2026-05-10T00:00:00Z', phase: 'upcoming', type: 'release', description: 'Release candidate for GA' },
    { id: 'ms-006', title: 'GA Launch', date: '2026-06-01T00:00:00Z', phase: 'upcoming', type: 'release', description: 'General availability release' },
  ],
  sprintBoundaries: [
    { date: '2026-03-23T00:00:00Z', label: 'Sprint 12' },
    { date: '2026-04-06T00:00:00Z', label: 'Sprint 13' },
    { date: '2026-04-20T00:00:00Z', label: 'Sprint 14' },
    { date: '2026-05-04T00:00:00Z', label: 'Sprint 15' },
  ],
};

// ── Build item index for report detail lookups ──

function buildItemIndex(): Map<string, ReportDetail> {
  const index = new Map<string, ReportDetail>();

  for (const item of projectItems) {
    index.set(item.id, {
      id: item.id,
      type: item.type,
      title: item.title,
      description: item.description,
      owner: item.owner,
      status: item.status,
      priority: item.priority,
      estimate: item.storyPoints,
      remainingWork: item.remainingWork,
      dependencies: item.dependencies.map((depId) => {
        const dep = projectItems.find((p) => p.id === depId);
        return { id: depId, title: dep?.title ?? 'Unknown', status: dep?.status ?? 'unknown' };
      }),
      recentActivity: item.recentActivity,
      metadata: {},
    });
  }

  for (const risk of risks) {
    index.set(risk.id, {
      id: risk.id,
      type: 'risk',
      title: risk.title,
      description: risk.description,
      owner: risk.owner,
      status: risk.status,
      priority: risk.severity,
      estimate: null,
      remainingWork: null,
      dependencies: [],
      recentActivity: [],
      metadata: { category: risk.category, impactArea: risk.impactArea },
    });
  }

  return index;
}

// ── Public factory ──

export function generateMockData(): AllMockData {
  return {
    projectSummary,
    projectItems,
    sprintMetrics,
    risks,
    teamActivity: { events: activityEvents, teamMembers },
    roadmap,
    itemIndex: buildItemIndex(),
  };
}