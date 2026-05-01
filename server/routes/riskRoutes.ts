import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

/**
 * Factory function: returns a Router for risks endpoint.
 */
export function createRiskRoutes(data: AllMockData): Router {
  const router = Router();

  // GET /api/risks — all project risks/blockers for radar visualization
  router.get('/risks', (_req, res) => {
    res.json({ risks: data.risks });
  });

  return router;
}