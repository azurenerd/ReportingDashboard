import { Router } from 'express';
import type { AllMockData } from '../data/types.js';

/**
 * Factory function: returns a Router for the detail report endpoint.
 * Performs a Map lookup on the universal itemIndex; returns 404
 * with a structured JSON error for unknown IDs.
 */
export function createReportRoutes(data: AllMockData): Router {
  const router = Router();

  // GET /api/report/:id — detail for any entity (epic, feature, story, risk)
  router.get('/report/:id', (req, res) => {
    const { id } = req.params;
    const detail = data.itemIndex.get(id);

    if (!detail) {
      res.status(404).json({
        error: {
          code: 'NOT_FOUND',
          message: `Item with id '${id}' not found`,
        },
      });
      return;
    }

    res.json(detail);
  });

  return router;
}