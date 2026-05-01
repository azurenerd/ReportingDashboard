import { describe, it, expect, beforeEach } from 'vitest';
import express, { type Express } from 'express';
import request from 'supertest';
import { createProjectRoutes } from '../../../server/routes/projectRoutes.js';
import { createSprintRoutes } from '../../../server/routes/sprintRoutes.js';
import { createRiskRoutes } from '../../../server/routes/riskRoutes.js';
import { createTeamRoutes } from '../../../server/routes/teamRoutes.js';
import { createRoadmapRoutes } from '../../../server/routes/roadmapRoutes.js';
import { createReportRoutes } from '../../../server/routes/reportRoutes.js';
import { generateMockData } from '../../../server/data/mockData.js';
import type { AllMockData } from '../../../server/data/types.js';

describe('Route Unit Tests', () => {
  let app: Express;
  let data: AllMockData;

  beforeEach(() => {
    app = express();
    data = generateMockData();
  });

  it('[Unit] GET /api/project-summary returns projectSummary from mock data', async () => {
    app.use(createProjectRoutes(data));
    const res = await request(app).get('/api/project-summary');
    expect(res.status).toBe(200);
    expect(res.body.id).toBe('proj-001');
    expect(res.body.name).toBe('Project Phoenix');
    expect(res.body.completionPercent).toBe(67);
  });

  it('[Unit] GET /api/project-items wraps items in { items } envelope', async () => {
    app.use(createProjectRoutes(data));
    const res = await request(app).get('/api/project-items');
    expect(res.status).toBe(200);
    expect(res.body).toHaveProperty('items');
    expect(Array.isArray(res.body.items)).toBe(true);
  });

  it('[Unit] GET /api/report/:id returns 404 for unknown id', async () => {
    app.use(createReportRoutes(data));
    const res = await request(app).get('/api/report/nonexistent');
    expect(res.status).toBe(404);
    expect(res.body.error.code).toBe('NOT_FOUND');
    expect(res.body.error.message).toContain('nonexistent');
  });

  it('[Unit] GET /api/report/:id returns item when found in itemIndex', async () => {
    data.itemIndex.set('item-1', { id: 'item-1', title: 'Test Item' } as any);
    app.use(createReportRoutes(data));
    const res = await request(app).get('/api/report/item-1');
    expect(res.status).toBe(200);
    expect(res.body.id).toBe('item-1');
  });

  it('[Unit] GET /api/sprint-metrics returns sprintMetrics data', async () => {
    app.use(createSprintRoutes(data));
    const res = await request(app).get('/api/sprint-metrics');
    expect(res.status).toBe(200);
    expect(res.body.sprintName).toBe('Sprint 14');
    expect(res.body.sprintNumber).toBe(14);
  });

  it('[Unit] GET /api/team-activity returns teamActivity data', async () => {
    app.use(createTeamRoutes(data));
    const res = await request(app).get('/api/team-activity');
    expect(res.status).toBe(200);
    expect(res.body).toHaveProperty('events');
    expect(res.body).toHaveProperty('teamMembers');
  });

  it('[Unit] GET /api/roadmap returns roadmap data', async () => {
    app.use(createRoadmapRoutes(data));
    const res = await request(app).get('/api/roadmap');
    expect(res.status).toBe(200);
    expect(res.body).toHaveProperty('milestones');
    expect(res.body).toHaveProperty('sprintBoundaries');
  });

  it('[Unit] GET /api/risks wraps data in { risks } envelope', async () => {
    app.use(createRiskRoutes(data));
    const res = await request(app).get('/api/risks');
    expect(res.status).toBe(200);
    expect(res.body).toHaveProperty('risks');
    expect(Array.isArray(res.body.risks)).toBe(true);
  });
});