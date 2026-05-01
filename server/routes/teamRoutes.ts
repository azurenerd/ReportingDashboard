import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

/**
 * Factory function: returns a Router for team activity endpoint.
 */
export function createTeamRoutes(data: AllMockData): Router {
  const router = Router();

  // GET /api/team-activity — recent events and team member list
  router.get('/team-activity', (_req, res) => {
    res.json(data.teamActivity);
  });

  return router;
}