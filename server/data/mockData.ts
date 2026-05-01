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
  ActivityEventType,
} from './types.js';
import { stableId, pickFrom, offsetDate, createRng, randInt, randPick, SEED } from './seed.js';

// ── Team members (10 total) ──

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

// ── Epic definitions ──

const epicDefs: { title: string; status: ItemStatus; owner: string; description: string }[] = [
  {
    title: 'User Authentication Platform',
    status: 'in-progress',
    owner: 'Sarah Chen',
    description: 'Complete identity and access management system with OAuth2, SSO, and MFA capabilities',
  },
  {
    title: 'Payment Processing Engine',
    status: 'in-progress',
    owner: 'Mike Torres',
    description: 'Scalable payment gateway integration supporting multiple providers and currencies',
  },
  {
    title: 'Analytics Dashboard',
    status: 'at-risk',
    owner: 'Alex Kim',
    description: 'Real-time analytics and reporting platform with customizable widget framework',
  },
  {
    title: 'Notification Service',
    status: 'done',
    owner: 'Priya Patel',
    description: 'Multi-channel notification delivery service supporting email, SMS, and push notifications',
  },
];

// ── Feature definitions per epic ──

const featureDefs: { title: string; description: string; status: ItemStatus; priority: 'critical' | 'high' | 'medium' | 'low' }[][] = [
  // Epic 1: Auth
  [
    { title: 'OAuth2 Integration', description: 'Implement OAuth2 authorization code flow with PKCE', status: 'done', priority: 'critical' },
    { title: 'SSO Integration', description: 'Implement SAML-based SSO for enterprise customers', status: 'in-progress', priority: 'high' },
    { title: 'Multi-Factor Authentication', description: 'Add TOTP and SMS-based second factor authentication', status: 'not-started', priority: 'high' },
  ],
  // Epic 2: Payments
  [
    { title: 'Stripe Integration', description: 'Core payment processing via Stripe Connect', status: 'done', priority: 'critical' },
    { title: 'Invoice Generation', description: 'Automated invoice creation and PDF rendering', status: 'in-progress', priority: 'medium' },
    { title: 'Subscription Billing', description: 'Recurring billing engine with proration and plan changes', status: 'blocked', priority: 'high' },
  ],
  // Epic 3: Analytics
  [
    { title: 'Data Pipeline', description: 'Event ingestion and processing pipeline for analytics', status: 'in-progress', priority: 'high' },
    { title: 'Widget Framework', description: 'Configurable dashboard widget system with drag-and-drop', status: 'not-started', priority: 'medium' },
    { title: 'Export & Reporting', description: 'CSV/PDF export and scheduled report delivery', status: 'not-started', priority: 'low' },
  ],
  // Epic 4: Notifications
  [
    { title: 'Email Delivery', description: 'Transactional email service with template engine', status: 'done', priority: 'high' },
    { title: 'Push Notifications', description: 'Web and mobile push notification delivery', status: 'done', priority: 'medium' },
    { title: 'Notification Preferences', description: 'User-configurable notification channels and frequency', status: 'done', priority: 'medium' },
  ],
];

// ── Story title templates per feature area ──

const storyTemplates: string[][] = [
  // Auth stories
  ['Implement token refresh logic', 'Add PKCE challenge generation', 'Create login page UI', 'Handle token expiry gracefully', 'Add logout flow with revocation'],
  ['Configure SAML metadata parser', 'Implement IdP discovery service', 'Build SSO redirect handler', 'Add session mapping for SSO users', 'Write SSO integration tests'],
  ['Design MFA enrollment flow', 'Implement TOTP secret generation', 'Build SMS OTP delivery', 'Create MFA verification API', 'Add backup codes generation'],
  // Payment stories
  ['Set up Stripe SDK integration', 'Implement charge creation flow', 'Add payment method storage', 'Handle webhook events', 'Build payment confirmation UI'],
  ['Design invoice data model', 'Implement PDF renderer', 'Add line item calculation engine', 'Create invoice email template', 'Build invoice history view'],
  ['Implement plan change logic', 'Add proration calculator', 'Build subscription lifecycle hooks', 'Create dunning retry flow', 'Handle payment method updates'],
  // Analytics stories
  ['Set up event schema registry', 'Implement batch event ingestion', 'Build real-time aggregation worker', 'Add data retention policies', 'Create pipeline monitoring'],
  ['Design widget component API', 'Implement grid layout engine', 'Build chart widget types', 'Add widget configuration panel', 'Create widget template library'],
  ['Implement CSV export endpoint', 'Build PDF report generator', 'Add scheduled report cron job', 'Create report template editor', 'Build export history log'],
  // Notification stories
  ['Integrate SendGrid provider', 'Build template rendering engine', 'Add bounce handling', 'Implement unsubscribe flow', 'Create email preview tool'],
  ['Set up FCM integration', 'Build notification payload builder', 'Add device token management', 'Implement notification grouping', 'Create delivery receipts'],
  ['Build preferences UI', 'Implement channel routing logic', 'Add frequency capping', 'Create digest aggregation', 'Build preference migration'],
];

// ── Statuses and priorities for cycling ──

const statuses: ItemStatus[] = ['done', 'in-progress', 'blocked', 'not-started', 'at-risk'];
const priorities: ('critical' | 'high' | 'medium' | 'low')[] = ['critical', 'high', 'medium', 'low'];

// ── Build project items hierarchy ──

function buildProjectItems(rng: () => number): ProjectItem[] {
  const items: ProjectItem[] = [];
  let storyGlobalIdx = 1;

  epicDefs.forEach((epic, ei) => {
    const epicId = stableId('epic', ei + 1);

    items.push({
      id: epicId,
      type: 'epic',
      title: epic.title,
      description: epic.description,
      status: epic.status,
      parentId: null,
      owner: epic.owner,
      priority: 'high',
      storyPoints: 89,
      remainingWork: epic.status === 'done' ? 0 : randInt(rng, 20, 60),
      dependencies: [],
      recentActivity: [],
    });

    const features = featureDefs[ei];
    features.forEach((feat, fi) => {
      const featId = stableId('feat', ei * 3 + fi + 1);
      const featOwner = pickFrom(teamMembers, ei * 3 + fi + 1).name;

      items.push({
        id: featId,
        type: 'feature',
        title: feat.title,
        description: feat.description,
        status: feat.status,
        parentId: epicId,
        owner: featOwner,
        priority: feat.priority,
        storyPoints: 21,
        remainingWork: feat.status === 'done' ? 0 : randInt(rng, 4, 16),
        dependencies: fi > 0 ? [stableId('feat', ei * 3 + fi)] : [],
        recentActivity: [],
      });

      // 4 stories for first feature in each epic, 3-4 for the rest (ensures 40+ total)
      const storyCount = fi === 0 ? 4 : fi === 1 ? 4 : 3;
      const templates = storyTemplates[ei * 3 + fi];

      for (let si = 0; si < storyCount; si++) {
        const storyId = stableId('story', storyGlobalIdx);
        const storyStatus = pickFrom(statuses, (ei * 7 + fi * 3 + si * 2) % statuses.length);
        const storyOwner = pickFrom(teamMembers, (ei + fi + si) % teamMembers.length).name;
        const storyType: ItemType = si % 5 === 4 ? 'task' : 'story';
        const points = randInt(rng, 2, 8);

        items.push({
          id: storyId,
          type: storyType,
          title: templates[si % templates.length],
          description: `${templates[si % templates.length]} for ${feat.title}`,
          status: storyStatus,
          parentId: featId,
          owner: storyOwner,
          priority: pickFrom(priorities, (ei + fi + si) % priorities.length),
          storyPoints: points,
          remainingWork: storyStatus === 'done' ? 0 : Math.round(points * 1.5),
          dependencies: si > 0 ? [stableId('story', storyGlobalIdx - 1)] : [],
          recentActivity: [],
        });

        storyGlobalIdx++;
      }
    });
  });

  return items;
}

// ── Risks (8 total across severity levels) ──

const risks: Risk[] = [
  {
    id: 'risk-001',
    title: 'Third-party API deprecation',
    description: 'Payment provider v2 API sunset in Q3 — must migrate before June 30',
    severity: 'critical',
    owner: 'Mike Torres',
    status: 'open',
    category: 'Technical',
    impactArea: 'Revenue Pipeline',
    mitigationPlan: 'Migration to v3 API scheduled for Sprint 16',
  },
  {
    id: 'risk-002',
    title: 'Key engineer availability',
    description: 'Lead architect on planned leave during Sprint 15-16',
    severity: 'high',
    owner: 'Nina Gupta',
    status: 'open',
    category: 'Resource',
    impactArea: 'Architecture Decisions',
    mitigationPlan: 'Cross-train backup engineer this sprint; document all pending decisions',
  },
  {
    id: 'risk-003',
    title: 'Performance regression',
    description: 'Dashboard load time increased 40% after analytics instrumentation release',
    severity: 'high',
    owner: 'Sarah Chen',
    status: 'mitigated',
    category: 'Technical',
    impactArea: 'User Experience',
    mitigationPlan: 'Performance profiling complete; lazy-loading PR merged, monitoring in place',
  },
  {
    id: 'risk-004',
    title: 'Scope creep on notifications',
    description: 'Stakeholders requesting real-time push notifications with read receipts',
    severity: 'medium',
    owner: 'David Park',
    status: 'open',
    category: 'Scope',
    impactArea: 'Timeline',
    mitigationPlan: 'Defer to Phase 2; document in backlog with clear acceptance criteria',
  },
  {
    id: 'risk-005',
    title: 'Security audit findings',
    description: 'Pending third-party security review may surface critical blocking issues',
    severity: 'high',
    owner: 'Jordan Lee',
    status: 'open',
    category: 'Security',
    impactArea: 'Release Schedule',
    mitigationPlan: 'Pre-audit checklist completed; 2-sprint remediation buffer reserved',
  },
  {
    id: 'risk-006',
    title: 'CI pipeline instability',
    description: 'Flaky end-to-end tests causing 30% false-positive build failures',
    severity: 'medium',
    owner: 'Emma Wilson',
    status: 'mitigated',
    category: 'Process',
    impactArea: 'Developer Velocity',
    mitigationPlan: 'Quarantined flaky tests; added retry logic and parallel execution',
  },
  {
    id: 'risk-007',
    title: 'Database migration complexity',
    description: 'Schema change requires zero-downtime migration across 3 regions',
    severity: 'low',
    owner: 'Priya Patel',
    status: 'open',
    category: 'Technical',
    impactArea: 'Data Integrity',
    mitigationPlan: 'Blue-green deployment strategy documented; dry-run scheduled Sprint 15',
  },
  {
    id: 'risk-008',
    title: 'Browser compatibility gaps',
    description: 'Safari WebGL2 rendering differences affecting 12% of user base',
    severity: 'low',
    owner: 'Alex Kim',
    status: 'open',
    category: 'Technical',
    impactArea: 'User Experience',
    mitigationPlan: 'Graceful degradation fallback implemented; tracking affected user reports',
  },
];

// ── Activity events (22 total, diverse types) ──

function buildActivityEvents(projectItems: ProjectItem[]): ActivityEvent[] {
  const eventDescriptions: { type: ActivityEventType; template: string }[] = [
    { type: 'pr-completed', template: 'Merged PR #247: Implement OAuth2 refresh flow' },
    { type: 'task-completed', template: 'Completed: Database schema migration script' },
    { type: 'comment', template: 'Commented on SSO Integration: Need IdP metadata format clarification' },
    { type: 'deployment', template: 'Deployed v2.4.1 to staging environment' },
    { type: 'review', template: 'Approved PR #251: Add input validation middleware' },
    { type: 'pr-completed', template: 'Merged PR #253: Stripe webhook handler' },
    { type: 'task-completed', template: 'Completed: Push notification payload builder' },
    { type: 'comment', template: 'Flagged blocker on subscription billing: missing proration logic' },
    { type: 'deployment', template: 'Deployed notification-service v1.2.0 to production' },
    { type: 'review', template: 'Requested changes on PR #260: Analytics event schema' },
    { type: 'pr-completed', template: 'Merged PR #262: Email template rendering engine' },
    { type: 'task-completed', template: 'Completed: Widget grid layout system' },
    { type: 'comment', template: 'Updated risk assessment: CI pipeline now stable after retry fix' },
    { type: 'deployment', template: 'Deployed auth-service v3.1.0 to canary (10% traffic)' },
    { type: 'review', template: 'Approved PR #265: MFA enrollment UI components' },
    { type: 'pr-completed', template: 'Merged PR #268: Invoice PDF renderer' },
    { type: 'task-completed', template: 'Completed: Notification preference migration script' },
    { type: 'pr-completed', template: 'Merged PR #270: Data pipeline batch processor' },
    { type: 'review', template: 'Approved PR #272: Payment confirmation page redesign' },
    { type: 'task-completed', template: 'Completed: Export history logging service' },
    { type: 'deployment', template: 'Deployed analytics-dashboard v0.9.0 to dev environment' },
    { type: 'comment', template: 'Sprint 14 retrospective: velocity improving, carry-over reducing' },
  ];

  // Get story IDs for relatedItemId references
  const storyIds = projectItems.filter((i) => i.type === 'story' || i.type === 'task').map((i) => i.id);

  return eventDescriptions.map((evt, i) => ({
    id: stableId('evt', i + 1),
    type: evt.type,
    actor: pickFrom(teamMembers, i).name,
    actorAvatar: pickFrom(teamMembers, i).avatar,
    description: evt.template,
    // Spread events across last 3 days, each ~3 hours apart (deterministic)
    timestamp: offsetDate(-Math.floor(i / 8), -(i % 8) * 3),
    relatedItemId: i % 3 === 0 ? pickFrom(storyIds, i) : null,
  }));
}

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

// ── Roadmap ──

const roadmap: Roadmap = {
  milestones: [
    { id: 'ms-001', title: 'Alpha Release', date: '2026-02-15T00:00:00Z', phase: 'completed', type: 'release', description: 'Internal alpha with core authentication and payment features' },
    { id: 'ms-002', title: 'Beta Release', date: '2026-03-15T00:00:00Z', phase: 'completed', type: 'release', description: 'External beta with partner integrations and notification service' },
    { id: 'ms-003', title: 'Security Audit Complete', date: '2026-04-01T00:00:00Z', phase: 'completed', type: 'milestone', description: 'Third-party penetration testing and security assessment' },
    { id: 'ms-004', title: 'Performance Milestone', date: '2026-04-20T00:00:00Z', phase: 'active', type: 'milestone', description: 'Sub-second page load time and 60fps animation targets met' },
    { id: 'ms-005', title: 'Release Candidate 1', date: '2026-05-10T00:00:00Z', phase: 'upcoming', type: 'release', description: 'Feature-complete release candidate for stakeholder sign-off' },
    { id: 'ms-006', title: 'General Availability', date: '2026-06-01T00:00:00Z', phase: 'upcoming', type: 'release', description: 'Public launch with full documentation and support' },
    { id: 'ms-007', title: 'Post-Launch Review', date: '2026-06-15T00:00:00Z', phase: 'upcoming', type: 'milestone', description: 'Performance review and Phase 2 planning kickoff' },
  ],
  sprintBoundaries: [
    { date: '2026-03-09T00:00:00Z', label: 'Sprint 11' },
    { date: '2026-03-23T00:00:00Z', label: 'Sprint 12' },
    { date: '2026-04-06T00:00:00Z', label: 'Sprint 13' },
    { date: '2026-04-20T00:00:00Z', label: 'Sprint 14' },
    { date: '2026-05-04T00:00:00Z', label: 'Sprint 15' },
    { date: '2026-05-18T00:00:00Z', label: 'Sprint 16' },
  ],
};

// ── Build the universal item index for /api/report/:id lookups ──

function buildItemIndex(
  projectItems: ProjectItem[],
  activityEvents: ActivityEvent[],
): Map<string, ReportDetail> {
  const index = new Map<string, ReportDetail>();

  // Map project items into the index
  for (const item of projectItems) {
    // Resolve dependency references to titles
    const resolvedDeps = item.dependencies.map((depId) => {
      const dep = projectItems.find((p) => p.id === depId);
      return { id: depId, title: dep?.title ?? 'Unknown', status: dep?.status ?? 'unknown' };
    });

    // Attach 1-3 recent activity events relevant to this item or its owner
    const relevantActivity = activityEvents
      .filter((e) => e.relatedItemId === item.id || e.actor === item.owner)
      .slice(0, 3);

    // Determine which epic this item belongs to for metadata
    let epicTitle = '';
    if (item.type === 'epic') {
      epicTitle = item.title;
    } else if (item.type === 'feature') {
      const parent = projectItems.find((p) => p.id === item.parentId);
      epicTitle = parent?.title ?? '';
    } else {
      const feat = projectItems.find((p) => p.id === item.parentId);
      const epic = feat ? projectItems.find((p) => p.id === feat.parentId) : undefined;
      epicTitle = epic?.title ?? '';
    }

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
      dependencies: resolvedDeps,
      recentActivity: relevantActivity,
      metadata: {
        sprint: 'Sprint 14',
        epicTitle,
      },
    });
  }

  // Map risks into the index
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
      metadata: {
        category: risk.category,
        impactArea: risk.impactArea,
        mitigationPlan: risk.mitigationPlan,
      },
    });
  }

  return index;
}

// ── Public factory — deterministic, same output every call ──

export function generateMockData(): AllMockData {
  const rng = createRng(SEED);
  const projectItems = buildProjectItems(rng);
  const activityEvents = buildActivityEvents(projectItems);

  // Populate recentActivity on items (post-hoc, now that events exist)
  for (const item of projectItems) {
    item.recentActivity = activityEvents
      .filter((e) => e.relatedItemId === item.id)
      .slice(0, 3);
  }

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
      totalStories: projectItems.filter((i) => i.type === 'story' || i.type === 'task').length,
    },
    projectItems,
    sprintMetrics,
    risks,
    teamActivity: { events: activityEvents, teamMembers },
    roadmap,
    itemIndex: buildItemIndex(projectItems, activityEvents),
  };
}