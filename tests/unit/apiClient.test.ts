import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { get } from '../../client/src/api/client';

describe('api client get()', () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('calls fetch with correct URL for path starting with /', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ data: 'test' }),
    });
    const result = await get<{ data: string }>('/project-summary');
    expect(globalThis.fetch).toHaveBeenCalledWith('/api/project-summary');
    expect(result).toEqual({ data: 'test' });
  });

  it('prepends slash for path not starting with /', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve([]),
    });
    await get('risks');
    expect(globalThis.fetch).toHaveBeenCalledWith('/api/risks');
  });

  it('throws with server error message when response not ok', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
      statusText: 'Not Found',
      json: () => Promise.resolve({ error: { message: 'Item not found' } }),
    });
    await expect(get('/report/missing')).rejects.toThrow('Item not found');
  });

  it('throws with HTTP status when body parse fails', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      statusText: 'Internal Server Error',
      json: () => Promise.reject(new Error('parse error')),
    });
    await expect(get('/broken')).rejects.toThrow('HTTP 500: Internal Server Error');
  });

  it('returns typed response on success', async () => {
    const payload = { id: 'proj-001', name: 'Phoenix' };
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(payload),
    });
    const result = await get<{ id: string; name: string }>('/project-summary');
    expect(result.id).toBe('proj-001');
  });
});