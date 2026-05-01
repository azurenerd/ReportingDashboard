import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { FetchError, api } from '../../client/src/api/client';

/**
 * Unit tests for client/src/api/client.ts
 * Tests the FetchError class and api object methods (fetchJson behavior).
 */

function mockFetchResponse(overrides: Partial<Response> & { json?: () => Promise<unknown> }) {
  const headers = new Headers();
  if (!overrides.headers) {
    headers.set('content-type', 'application/json');
  }
  return {
    ok: true,
    status: 200,
    headers: overrides.headers ?? headers,
    json: overrides.json ?? (() => Promise.resolve({})),
    ...overrides,
  } as unknown as Response;
}

describe('FetchError', () => {
  it('stores status, code, and message properties', () => {
    const err = new FetchError(404, 'NOT_FOUND', 'Resource not found');

    expect(err).toBeInstanceOf(Error);
    expect(err.name).toBe('FetchError');
    expect(err.status).toBe(404);
    expect(err.code).toBe('NOT_FOUND');
    expect(err.message).toBe('Resource not found');
  });
});

describe('api client (fetchJson)', () => {
  const originalFetch = globalThis.fetch;

  beforeEach(() => {
    vi.restoreAllMocks();
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('[Trait("Category", "Unit")] fetches correct URL and returns parsed JSON', async () => {
    const payload = { name: 'Project Phoenix', status: 'on-track' };
    globalThis.fetch = vi.fn().mockResolvedValue(mockFetchResponse({
      json: () => Promise.resolve(payload),
    }));

    const result = await api.getProjectSummary();

    expect(globalThis.fetch).toHaveBeenCalledWith('/api/project-summary', { signal: undefined });
    expect(result).toEqual(payload);
  });

  it('[Trait("Category", "Unit")] throws FetchError with server error body on non-2xx', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue(mockFetchResponse({
      ok: false,
      status: 422,
      json: () => Promise.resolve({ error: { code: 'VALIDATION', message: 'Invalid ID' } }),
    }));

    await expect(api.getReportDetail('bad-id')).rejects.toMatchObject({
      status: 422,
      code: 'VALIDATION',
      message: 'Invalid ID',
    });
  });

  it('[Trait("Category", "Unit")] throws FetchError with default message when error body is not JSON', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue(mockFetchResponse({
      ok: false,
      status: 500,
      json: () => Promise.reject(new SyntaxError('Unexpected token')),
    }));

    await expect(api.getSprintMetrics()).rejects.toMatchObject({
      status: 500,
      code: 'UNKNOWN',
      message: 'Request failed with status 500',
    });
  });

  it('[Trait("Category", "Unit")] encodes id in getReportDetail URL', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue(mockFetchResponse({
      json: () => Promise.resolve({ id: 'special/id' }),
    }));

    await api.getReportDetail('special/id');

    expect(globalThis.fetch).toHaveBeenCalledWith(
      '/api/report/special%2Fid',
      expect.objectContaining({}),
    );
  });
});