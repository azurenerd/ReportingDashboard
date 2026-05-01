/**
 * Deterministic seed utilities for consistent mock data generation.
 * No Math.random() is used anywhere — all randomness comes from
 * a seeded PRNG (mulberry32) so output is identical on every call.
 */

// ── Fixed base date anchoring all relative dates ──
// Using a fixed epoch so timestamps never drift between runs.
export const BASE_DATE = new Date('2026-04-30T12:00:00Z');

export const SEED = 42;

// ── Mulberry32 seeded PRNG ──
// Returns a function that produces deterministic floats in [0, 1).
export function createRng(seed: number): () => number {
  let s = seed | 0;
  return (): number => {
    s = (s + 0x6d2b79f5) | 0;
    let t = Math.imul(s ^ (s >>> 15), 1 | s);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

// ── Helper: generate stable IDs with zero-padded index ──
export function stableId(prefix: string, index: number): string {
  return `${prefix}-${String(index).padStart(3, '0')}`;
}

// ── Helper: pick deterministically from an array using an index ──
export function pickFrom<T>(arr: readonly T[], index: number): T {
  return arr[index % arr.length];
}

// ── Helper: generate an ISO date string offset from BASE_DATE ──
export function offsetDate(daysOffset: number, hoursOffset: number = 0): string {
  const ms = BASE_DATE.getTime() + daysOffset * 86400000 + hoursOffset * 3600000;
  return new Date(ms).toISOString();
}

// ── Helper: deterministic integer in [min, max] from RNG ──
export function randInt(rng: () => number, min: number, max: number): number {
  return Math.floor(rng() * (max - min + 1)) + min;
}

// ── Helper: deterministic pick from array using RNG ──
export function randPick<T>(rng: () => number, arr: readonly T[]): T {
  return arr[Math.floor(rng() * arr.length)];
}

// ── Helper: deterministic shuffle (Fisher-Yates) ──
export function shuffle<T>(rng: () => number, arr: T[]): T[] {
  const result = [...arr];
  for (let i = result.length - 1; i > 0; i--) {
    const j = Math.floor(rng() * (i + 1));
    [result[i], result[j]] = [result[j], result[i]];
  }
  return result;
}