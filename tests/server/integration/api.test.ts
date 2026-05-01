import { describe, it, expect, beforeAll, afterAll, vi } from 'vitest';
import express, { type Express } from 'express';
import request from 'supertest';
import { createProjectRoutes } from '../../../server/routes/projectRoutes.js';
import { createSprintRoutes } from '../../../server/routes/sprintRoutes.js';
import { createRiskRoutes } from '../../../server/routes/riskRoutes.js';
import { createTeamRoutes } from '../../../server/routes/teamRoutes.js';
import { createRoadmapRoutes } from '../../../server/routes/roadmapRoutes.js';
import { createReportRoutes } from '../../../server/routes/reportRoutes.js';
import { errorHandler } from '../../../server/middleware/errorHandler.js';
import { generateMockData } from '../../../server/data/mockData.js';

describe('API Integration Tests', () => {
  let app: Express;
  let consoleSpy: ReturnType<typeof vi.spyOn>;

  beforeAll(() => {
    consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    const data = generateMockData();
    app = express();
    app.use(express.json());
    app.use(createProjectRoutes(data));
    app.use(createSprintRoutes(data));
    app.use(createRiskRoutes(data));
    app.use(createTeamRoutes(data));
    app.use(createRoadmapRoutes(data));
    app.use(createReportRoutes(data));
    app.use(errorHandler);
  });

  afterAll(() => {
    consoleSpy.mockRestore();
  });

  it('[Integration] all 7 API endpoints return expected status codes', async () => {
    const endpoints = [
      { path: '/api/project-summary', status: 200 },
      { path: '/api/project-items', status: 200 },
      { path: '/api/sprint-metrics', status: 200 },
      { path: '/api/risks', status: 200 },
      { path: '/api/team-activity', status: 200 },
      { path: '/api/roadmap', status: 200 },
      { path: '/api/report/nonexistent', status: 404 },
    ];
    for (const { path, status } of endpoints) {
      const res = await request(app).get(path);
      expect(res.status, `${path} should return ${status}`).toBe(status);
      expect(res.headers['content-type']).toMatch(/json/);
    }
  });

  it('[Integration] project-summary returns full mock data structure', async () => {
    const res = await request(app).get('/api/project-summary');
    expect(res.body).toMatchObject({
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
    });
  });

  it('[Integration] risks endpoint wraps data in { risks } envelope', async () => {
    const res = await request(app).get('/api/risks');
    expect(res.body).toHaveProperty('risks');
    expect(Array.isArray(res.body.risks)).toBe(true);
  });

  it('[Integration] report 404 returns structured error JSON', async () => {
    const res = await request(app).get('/api/report/does-not-exist');
    expect(res.status).toBe(404);
    expect(res.body.error).toEqual({
      code: 'NOT_FOUND',
      message: "Item with id 'does-not-exist' not found",
    });
  });

  it('[Integration] error handler catches middleware errors', async () => {
    const errApp = express();
    errApp.get('/api/boom', () => { throw new Error('kaboom'); });
    errApp.use(errorHandler);

    const res = await request(errApp).get('/api/boom');
    expect(res.status).toBe(500);
    expect(res.body.error.code).toBe('INTERNAL_ERROR');
    expect(res.body.error.message).toBe('kaboom');
  });
});