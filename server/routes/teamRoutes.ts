import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

export function createTeamRoutes(data: AllMockData): Router {
  const router = Router();

  router.get('/team-activity', (_req, res) => {
    res.json(data.teamActivity);
  });

  return router;
}