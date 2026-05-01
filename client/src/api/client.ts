const BASE_URL = '/api';

/** Generic typed GET request with error handling. */
export async function get<T>(path: string): Promise<T> {
  const url = `${BASE_URL}${path.startsWith('/') ? path : `/${path}`}`;
  const res = await fetch(url);

  if (!res.ok) {
    const body = await res.json().catch(() => null);
    const message = body?.error?.message || `HTTP ${res.status}: ${res.statusText}`;
    throw new Error(message);
  }

  return res.json() as Promise<T>;
}