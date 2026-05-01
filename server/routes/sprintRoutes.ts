import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

/**
 * Factory function: returns a Router for sprint metrics endpoint.
 */
export function createSprintRoutes(data: AllMockData): Router {
  const router = Router();

  // GET /api/sprint-metrics — velocity, burndown, blocker counts
  router.get('/sprint-metrics', (_req, res) => {
    res.json(data.sprintMetrics);
  });

  return router;
}