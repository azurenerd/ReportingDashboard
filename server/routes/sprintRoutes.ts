import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

export function createSprintRoutes(data: AllMockData): Router {
  const router = Router();
  router.get('/api/sprint-metrics', (_req, res) => { res.json(data.sprintMetrics); });
  return router;
}