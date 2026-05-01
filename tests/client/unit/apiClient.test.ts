import { describe, it, expect, vi, afterEach } from 'vitest';
import { get, ApiClientError } from '../../../client/src/api/client.js';

describe('API Client Unit Tests', () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('[Unit] get() returns parsed JSON on success', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ id: 1, name: 'test' }),
    });
    const result = await get<{ id: number; name: string }>('/api/test');
    expect(result).toEqual({ id: 1, name: 'test' });
    expect(globalThis.fetch).toHaveBeenCalledWith('/api/test');
  });

  it('[Unit] get() throws ApiClientError with parsed error body on failure', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
      json: () => Promise.resolve({ error: { code: 'NOT_FOUND', message: 'Not found' } }),
    });
    try {
      await get('/api/missing');
      expect.unreachable('should have thrown');
    } catch (e) {
      expect(e).toBeInstanceOf(ApiClientError);
      const err = e as ApiClientError;
      expect(err.status).toBe(404);
      expect(err.apiError?.code).toBe('NOT_FOUND');
      expect(err.message).toBe('Not found');
      expect(err.name).toBe('ApiClientError');
    }
  });

  it('[Unit] get() throws ApiClientError with generic message when body is not JSON', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      json: () => Promise.reject(new Error('not json')),
    });
    try {
      await get('/api/broken');
      expect.unreachable('should have thrown');
    } catch (e) {
      const err = e as ApiClientError;
      expect(err.status).toBe(500);
      expect(err.apiError).toBeUndefined();
      expect(err.message).toBe('API error: 500');
    }
  });

  it('[Unit] ApiClientError sets name property correctly', () => {
    const err = new ApiClientError(400, { code: 'BAD', message: 'bad request' });
    expect(err.name).toBe('ApiClientError');
    expect(err.status).toBe(400);
    expect(err.message).toBe('bad request');
    expect(err.apiError).toEqual({ code: 'BAD', message: 'bad request' });
  });

  it('[Unit] ApiClientError uses fallback message when no apiError provided', () => {
    const err = new ApiClientError(503);
    expect(err.message).toBe('API error: 503');
    expect(err.apiError).toBeUndefined();
  });
});