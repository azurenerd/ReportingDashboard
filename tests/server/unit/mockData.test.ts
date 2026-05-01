import { describe, it, expect } from 'vitest';
import { generateMockData } from '../../../server/data/mockData.js';

describe('generateMockData Unit Tests', () => {
  it('[Unit] returns all required top-level keys with correct types', () => {
    const data = generateMockData();
    expect(data.projectSummary).toBeDefined();
    expect(data.projectItems).toBeInstanceOf(Array);
    expect(data.risks).toBeInstanceOf(Array);
    expect(data.teamActivity).toHaveProperty('events');
    expect(data.teamActivity).toHaveProperty('teamMembers');
    expect(data.roadmap).toHaveProperty('milestones');
    expect(data.itemIndex).toBeInstanceOf(Map);
  });

  it('[Unit] projectSummary contains expected hardcoded values', () => {
    const { projectSummary } = generateMockData();
    expect(projectSummary.id).toBe('proj-001');
    expect(projectSummary.name).toBe('Project Phoenix');
    expect(projectSummary.status).toBe('In Progress');
    expect(projectSummary.currentSprint).toBe('Sprint 14');
    expect(projectSummary.completionPercent).toBe(67);
    expect(projectSummary.deliveryConfidence).toBe(78);
    expect(projectSummary.daysRemaining).toBe(8);
    expect(projectSummary.healthScore).toBe(72);
    expect(projectSummary.healthColor).toBe('yellow');
    expect(projectSummary.totalEpics).toBe(4);
    expect(projectSummary.totalFeatures).toBe(12);
    expect(projectSummary.totalStories).toBe(48);
  });

  it('[Unit] sprintMetrics contains expected hardcoded values', () => {
    const { sprintMetrics } = generateMockData();
    expect(sprintMetrics.sprintName).toBe('Sprint 14');
    expect(sprintMetrics.sprintNumber).toBe(14);
    expect(sprintMetrics.startDate).toBe('2026-04-20T00:00:00Z');
    expect(sprintMetrics.endDate).toBe('2026-05-03T00:00:00Z');
    expect(sprintMetrics.openBugs).toBe(0);
    expect(sprintMetrics.blockers).toBe(0);
    expect(sprintMetrics.carryoverItems).toBe(0);
  });
});