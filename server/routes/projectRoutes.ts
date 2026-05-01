import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

/**
 * Factory function: receives the pre-generated mock data singleton
 * and returns a Router with project-related endpoints.
 */
export function createProjectRoutes(data: AllMockData): Router {
  const router = Router();

  // GET /api/project-summary — high-level project health indicators
  router.get('/project-summary', (_req, res) => {
    res.json(data.projectSummary);
  });

  // GET /api/project-items — full hierarchy for 3D node graph
  router.get('/project-items', (_req, res) => {
    res.json({ items: data.projectItems });
  });

  return router;
}