// Integration tests for Express API server
// Category: Integration
const request = require('supertest');
const express = require('express');
const cors = require('cors');

// Build app without calling listen() so supertest can bind it
function createApp() {
  const app = express();
  app.use(cors({ origin: 'http://localhost:5173' }));
  app.use(express.json());

  app.get('/api/health', (_req, res) => {
    res.json({ status: 'ok' });
  });

  // Load routes from source
  const projectRoutes = require('../../../src/ReportingDashboard-web/server/routes/project');
  app.use('/api', projectRoutes);

  app.use((err, _req, res, _next) => {
    res.status(err.status || 500).json({
      error: 'Internal Server Error',
      message: err.message || 'An unexpected error occurred',
    });
  });

  return app;
}

describe('Express API Server', () => {
  let app;

  beforeAll(() => {
    app = createApp();
  });

  test('GET /api/health returns ok status', async () => {
    const res = await request(app).get('/api/health');
    expect(res.status).toBe(200);
    expect(res.body).toEqual({ status: 'ok' });
  });

  test('GET /api/project-summary returns project summary data', async () => {
    const res = await request(app).get('/api/project-summary');
    expect(res.status).toBe(200);
    expect(res.body).toBeDefined();
    expect(typeof res.body).toBe('object');
  });

  test('GET /api/risks returns an array', async () => {
    const res = await request(app).get('/api/risks');
    expect(res.status).toBe(200);
    expect(Array.isArray(res.body)).toBe(true);
  });

  test('GET /api/report/:id returns 404 for unknown id', async () => {
    const res = await request(app).get('/api/report/nonexistent-id-xyz');
    expect(res.status).toBe(404);
    expect(res.body).toEqual({ error: 'Not Found' });
  });

  test('GET /api/roadmap returns an array', async () => {
    const res = await request(app).get('/api/roadmap');
    expect(res.status).toBe(200);
    expect(Array.isArray(res.body)).toBe(true);
  });
});