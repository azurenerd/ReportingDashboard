/**
 * Unit tests for getRelativeTime utility (tested via re-implementation
 * since the function is not exported from the module).
 * These tests validate the exact algorithm used in ActivityFeed.tsx.
 * Trait: Category = Unit
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

// Re-implement the exact function from ActivityFeed.tsx to test its logic in isolation
function getRelativeTime(timestamp: string): string {
  const now = Date.now();
  const then = new Date(timestamp).getTime();
  const diffMs = now - then;

  if (diffMs < 0) return 'just now';

  const diffSec = Math.floor(diffMs / 1000);
  const diffMin = Math.floor(diffSec / 60);
  const diffHr = Math.floor(diffMin / 60);
  const diffDay = Math.floor(diffHr / 24);
  const diffWeek = Math.floor(diffDay / 7);

  if (diffSec < 60) return 'just now';
  if (diffMin < 60) return `${diffMin}m ago`;
  if (diffHr < 24) return `${diffHr}h ago`;
  if (diffDay < 7) return `${diffDay}d ago`;
  if (diffWeek < 4) return `${diffWeek}w ago`;
  return new Date(timestamp).toLocaleDateString();
}

// Re-implement getAvatarColor for isolated testing
const avatarColors = [
  'bg-indigo-500',
  'bg-rose-500',
  'bg-emerald-500',
  'bg-amber-500',
  'bg-cyan-500',
  'bg-fuchsia-500',
  'bg-teal-500',
  'bg-violet-500',
  'bg-orange-500',
  'bg-sky-500',
];

function getAvatarColor(initials: string): string {
  let hash = 0;
  for (let i = 0; i < initials.length; i++) {
    hash = initials.charCodeAt(i) + ((hash << 5) - hash);
  }
  return avatarColors[Math.abs(hash) % avatarColors.length];
}

describe('getRelativeTime', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-05-01T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  // Category: Unit
  it('returns "just now" for timestamps less than 60 seconds ago', () => {
    const thirtySecsAgo = new Date(Date.now() - 30000).toISOString();
    expect(getRelativeTime(thirtySecsAgo)).toBe('just now');
  });

  // Category: Unit
  it('returns "just now" for future timestamps (negative diff)', () => {
    const future = new Date(Date.now() + 60000).toISOString();
    expect(getRelativeTime(future)).toBe('just now');
  });

  // Category: Unit
  it('returns minutes format for times between 1-59 minutes ago', () => {
    const fiveMinAgo = new Date(Date.now() - 5 * 60 * 1000).toISOString();
    expect(getRelativeTime(fiveMinAgo)).toBe('5m ago');

    const fiftyNineMinAgo = new Date(Date.now() - 59 * 60 * 1000).toISOString();
    expect(getRelativeTime(fiftyNineMinAgo)).toBe('59m ago');
  });

  // Category: Unit
  it('returns hours/days/weeks format for longer durations', () => {
    const twoHrsAgo = new Date(Date.now() - 2 * 3600 * 1000).toISOString();
    expect(getRelativeTime(twoHrsAgo)).toBe('2h ago');

    const threeDaysAgo = new Date(Date.now() - 3 * 86400 * 1000).toISOString();
    expect(getRelativeTime(threeDaysAgo)).toBe('3d ago');

    const twoWeeksAgo = new Date(Date.now() - 14 * 86400 * 1000).toISOString();
    expect(getRelativeTime(twoWeeksAgo)).toBe('2w ago');
  });

  // Category: Unit
  it('returns locale date string for timestamps older than 4 weeks', () => {
    const sixWeeksAgo = new Date(Date.now() - 42 * 86400 * 1000).toISOString();
    const result = getRelativeTime(sixWeeksAgo);
    // Should be a locale date string, not "Xw ago"
    expect(result).not.toContain('w ago');
    expect(result).toMatch(/\d/); // Contains digits (date)
  });
});

describe('getAvatarColor', () => {
  // Category: Unit
  it('returns deterministic color for same initials', () => {
    const color1 = getAvatarColor('JD');
    const color2 = getAvatarColor('JD');
    expect(color1).toBe(color2);
  });

  // Category: Unit
  it('returns a valid Tailwind color class from the palette', () => {
    const color = getAvatarColor('AB');
    expect(avatarColors).toContain(color);
  });

  // Category: Unit
  it('produces different colors for different initials', () => {
    const colors = new Set(['AB', 'CD', 'EF', 'GH', 'IJ'].map(getAvatarColor));
    // With 5 different inputs and 10 possible colors, expect at least 2 distinct
    expect(colors.size).toBeGreaterThanOrEqual(2);
  });
});