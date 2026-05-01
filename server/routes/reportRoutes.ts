import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

export function createReportRoutes(data: AllMockData): Router {
  const router = Router();

  router.get('/report/:id', (req, res) => {
    const detail = data.itemIndex.get(req.params.id);
    if (!detail) {
      res.status(404).json({
        error: {
          code: 'NOT_FOUND',
          message: `Item with id '${req.params.id}' not found`,
        },
      });
      return;
    }
    res.json(detail);
  });

  return router;
}