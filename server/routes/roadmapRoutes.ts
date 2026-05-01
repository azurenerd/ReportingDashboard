import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

export function createRoadmapRoutes(data: AllMockData): Router {
  const router = Router();
  router.get('/api/roadmap', (_req, res) => { res.json(data.roadmap); });
  return router;
}