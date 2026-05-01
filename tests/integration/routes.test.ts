import { describe, it, expect, beforeAll } from 'vitest';
import express from 'express';
import request from 'supertest';
import { createProjectRoutes } from '../../server/routes/projectRoutes.js';
import { createSprintRoutes } from '../../server/routes/sprintRoutes.js';
import { createRiskRoutes } from '../../server/routes/riskRoutes.js';
import { createRoadmapRoutes } from '../../server/routes/roadmapRoutes.js';
import { createReportRoutes } from '../../server/routes/reportRoutes.js';
import { errorHandler } from '../../server/middleware/errorHandler.js';
import type { AllMockData } from '../../server/data/types.js';

const mockData: AllMockData = {
  projectSummary: { id: 'proj-001', name: 'Test Project', status: 'In Progress', currentSprint: 'Sprint 1', completionPercent: 50, deliveryConfidence: 80, daysRemaining: 5, healthScore: 75, healthColor: 'green', totalEpics: 2, totalFeatures: 4, totalStories: 10 } as any,
  projectItems: [{ id: 'item-1', title: 'Epic 1', type: 'epic' }] as any,
  sprintMetrics: { sprintName: 'Sprint 1', openBugs: 3, blockers: 1, carryoverItems: 0 } as any,
  risks: [{ id: 'risk-001', title: 'Test Risk', severity: 'high' }] as any,
  teamActivity: { members: [], events: [] } as any,
  roadmap: { milestones: [{ id: 'ms-001', title: 'Alpha' }] } as any,
  itemIndex: new Map([['item-1', { id: 'item-1', title: 'Epic 1' }]]) as any,
};

function buildApp() {
  const app = express();
  app.use('/api', createProjectRoutes(mockData));
  app.use('/api', createSprintRoutes(mockData));
  app.use('/api', createRiskRoutes(mockData));
  app.use('/api', createRoadmapRoutes(mockData));
  app.use('/api', createReportRoutes(mockData));
  app.use(errorHandler);
  return app;
}

describe('Express API routes', () => {
  const app = buildApp();

  it('GET /api/project-summary returns projectSummary', async () => {
    const res = await request(app).get('/api/project-summary');
    expect(res.status).toBe(200);
    expect(res.body.id).toBe('proj-001');
    expect(res.body.name).toBe('Test Project');
  });

  it('GET /api/project-items returns items wrapped in object', async () => {
    const res = await request(app).get('/api/project-items');
    expect(res.status).toBe(200);
    expect(res.body.items).toHaveLength(1);
    expect(res.body.items[0].id).toBe('item-1');
  });

  it('GET /api/risks returns risks array', async () => {
    const res = await request(app).get('/api/risks');
    expect(res.status).toBe(200);
    expect(res.body.risks).toHaveLength(1);
    expect(res.body.risks[0].severity).toBe('high');
  });

  it('GET /api/report/:id returns 404 for unknown id', async () => {
    const res = await request(app).get('/api/report/nonexistent');
    expect(res.status).toBe(404);
    expect(res.body.error.code).toBe('NOT_FOUND');
    expect(res.body.error.message).toContain('nonexistent');
  });

  it('GET /api/report/:id returns item when found', async () => {
    const res = await request(app).get('/api/report/item-1');
    expect(res.status).toBe(200);
    expect(res.body.id).toBe('item-1');
  });
});