import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

/**
 * Factory function: returns a Router for roadmap endpoint.
 */
export function createRoadmapRoutes(data: AllMockData): Router {
  const router = Router();

  // GET /api/roadmap — milestones and sprint boundaries for timeline
  router.get('/roadmap', (_req, res) => {
    res.json(data.roadmap);
  });

  return router;
}