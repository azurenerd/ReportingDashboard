import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

export function createRiskRoutes(data: AllMockData): Router {
  const router = Router();

  router.get('/risks', (_req, res) => {
    res.json({ risks: data.risks });
  });

  return router;
}