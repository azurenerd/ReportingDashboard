/**
 * Deterministic seed utilities for consistent mock data generation.
 */
export const SEED = 42;

/** Simple deterministic hash for generating stable IDs. */
export function stableId(prefix: string, index: number): string {
  return `${prefix}-${String(index).padStart(3, '0')}`;
}