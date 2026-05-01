import { describe, it, expect, vi, afterEach } from 'vitest';
import express from 'express';
import request from 'supertest';
import { errorHandler } from '../../../server/middleware/errorHandler.js';

describe('errorHandler Unit Tests', () => {
  const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

  afterEach(() => {
    consoleSpy.mockClear();
  });

  it('[Unit] returns 500 with INTERNAL_ERROR code for thrown errors', async () => {
    const app = express();
    app.get('/fail', (_req, _res, next) => {
      next(new Error('something broke'));
    });
    app.use(errorHandler);

    const res = await request(app).get('/fail');
    expect(res.status).toBe(500);
    expect(res.body.error.code).toBe('INTERNAL_ERROR');
    expect(res.body.error.message).toBe('something broke');
    expect(consoleSpy).toHaveBeenCalledWith('[Server Error]', 'something broke');
  });
});