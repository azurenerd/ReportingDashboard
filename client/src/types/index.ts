/**
 * Re-export all shared types from the server data layer.
 * This provides a single import point for client code.
 */
export type {
  ItemStatus,
  ItemType,
  RiskSeverity,
  ActivityEventType,
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  ActivityEvent,
  TeamMember,
  TeamActivity,
  RoadmapMilestone,
  Roadmap,
  ReportDetail,
  AllMockData,
} from '@shared/types';