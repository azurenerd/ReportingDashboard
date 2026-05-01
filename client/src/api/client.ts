const BASE_URL = '';

export interface ApiError { code: string; message: string; }

export class ApiClientError extends Error {
  constructor(public status: number, public apiError?: ApiError) {
    super(apiError?.message || `API error: ${status}`);
    this.name = 'ApiClientError';
  }
}

export async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`);
  if (!res.ok) {
    let apiError: ApiError | undefined;
    try { const body = await res.json(); apiError = body.error; } catch { /* not JSON */ }
    throw new ApiClientError(res.status, apiError);
  }
  return res.json() as Promise<T>;
}