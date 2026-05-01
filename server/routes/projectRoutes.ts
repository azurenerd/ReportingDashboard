import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

export function createProjectRoutes(data: AllMockData): Router {
  const router = Router();
  router.get('/api/project-summary', (_req, res) => { res.json(data.projectSummary); });
  router.get('/api/project-items', (_req, res) => { res.json({ items: data.projectItems }); });
  return router;
}