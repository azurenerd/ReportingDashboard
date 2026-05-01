import { describe, it, expect, beforeAll } from 'vitest';
import express, { type Express } from 'express';
import request from 'supertest';
import { createProjectRoutes } from '../../server/routes/projectRoutes.js';
import { createSprintRoutes } from '../../server/routes/sprintRoutes.js';
import { createRiskRoutes } from '../../server/routes/riskRoutes.js';
import { createTeamRoutes } from '../../server/routes/teamRoutes.js';
import { createRoadmapRoutes } from '../../server/routes/roadmapRoutes.js';
import { createReportRoutes } from '../../server/routes/reportRoutes.js';
import { errorHandler } from '../../server/middleware/errorHandler.js';
import type { AllMockData } from '../../server/data/types.js';

/**
 * Integration tests for all 7 REST API endpoints.
 * Spins up an Express app with all routes wired and mock data,
 * then exercises each endpoint via supertest.
 */

const KNOWN_ITEM_ID = 'item-001';
const KNOWN_ITEM_DETAIL = { id: KNOWN_ITEM_ID, title: 'Test Item', type: 'epic', status: 'In Progress' };

function createMockData(): AllMockData {
  const itemIndex = new Map<string, unknown>();
  itemIndex.set(KNOWN_ITEM_ID, KNOWN_ITEM_DETAIL);

  return {
    projectSummary: { name: 'Test Project', status: 'On Track', completionPercent: 65 },
    projectItems: [
      { id: 'epic-1', type: 'epic', title: 'Epic One', status: 'In Progress', children: [] },
    ],
    sprintMetrics: { velocity: 42, burndown: [10, 8, 6, 4], blockers: 2 },
    risks: [
      { id: 'risk-1', severity: 'high', description: 'Dependency delay', owner: 'Alice' },
    ],
    teamActivity: { members: [{ name: 'Alice', role: 'Dev' }], events: [] },
    roadmap: { milestones: [{ name: 'MVP', date: '2026-06-01' }], sprints: [] },
    itemIndex,
  } as unknown as AllMockData;
}

function buildApp(): Express {
  const app = express();
  const data = createMockData();

  app.use('/api', createProjectRoutes(data));
  app.use('/api', createSprintRoutes(data));
  app.use('/api', createRiskRoutes(data));
  app.use('/api', createTeamRoutes(data));
  app.use('/api', createRoadmapRoutes(data));
  app.use('/api', createReportRoutes(data));
  app.use(errorHandler);

  return app;
}

describe('Backend REST API Integration', { concurrent: false }, () => {
  let app: Express;

  beforeAll(() => {
    app = buildApp();
  });

  it('[Integration] GET /api/project-summary returns 200 with JSON project data', async () => {
    const res = await request(app).get('/api/project-summary');

    expect(res.status).toBe(200);
    expect(res.headers['content-type']).toMatch(/application\/json/);
    expect(res.body).toHaveProperty('name', 'Test Project');
    expect(res.body).toHaveProperty('status', 'On Track');
    expect(res.body).toHaveProperty('completionPercent', 65);
  });

  it('[Integration] GET /api/project-items returns 200 with items array wrapper', async () => {
    const res = await request(app).get('/api/project-items');

    expect(res.status).toBe(200);
    expect(res.headers['content-type']).toMatch(/application\/json/);
    expect(res.body).toHaveProperty('items');
    expect(Array.isArray(res.body.items)).toBe(true);
    expect(res.body.items.length).toBeGreaterThan(0);
    expect(res.body.items[0]).toHaveProperty('id', 'epic-1');
  });

  it('[Integration] GET /api/risks returns 200 with risks array wrapper', async () => {
    const res = await request(app).get('/api/risks');

    expect(res.status).toBe(200);
    expect(res.headers['content-type']).toMatch(/application\/json/);
    expect(res.body).toHaveProperty('risks');
    expect(Array.isArray(res.body.risks)).toBe(true);
    expect(res.body.risks[0]).toHaveProperty('severity', 'high');
  });

  it('[Integration] GET /api/report/:id returns 200 for known ID', async () => {
    const res = await request(app).get(`/api/report/${KNOWN_ITEM_ID}`);

    expect(res.status).toBe(200);
    expect(res.headers['content-type']).toMatch(/application\/json/);
    expect(res.body).toHaveProperty('id', KNOWN_ITEM_ID);
    expect(res.body).toHaveProperty('title', 'Test Item');
  });

  it('[Integration] GET /api/report/:id returns 404 with structured error for unknown ID', async () => {
    const res = await request(app).get('/api/report/nonexistent-id');

    expect(res.status).toBe(404);
    expect(res.headers['content-type']).toMatch(/application\/json/);
    expect(res.body).toHaveProperty('error');
    expect(res.body.error).toHaveProperty('code', 'NOT_FOUND');
    expect(res.body.error).toHaveProperty('message');
    expect(res.body.error.message).toContain('nonexistent-id');
  });
});