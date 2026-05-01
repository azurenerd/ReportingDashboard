import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useAnimatedValue, useAnimatedRoundedValue } from '../../client/src/hooks/useAnimatedValue';

// Mock gsap
const mockKill = vi.fn();
let capturedOnUpdate: (() => void) | undefined;
let capturedTarget: Record<string, unknown> | undefined;
let capturedConfig: Record<string, unknown> | undefined;

vi.mock('gsap', () => ({
  default: {
    to: vi.fn((target: Record<string, unknown>, config: Record<string, unknown>) => {
      capturedTarget = target;
      capturedConfig = config;
      capturedOnUpdate = config.onUpdate as (() => void) | undefined;
      // Simulate immediate completion: set value on target and invoke onUpdate
      target.value = config.value;
      if (capturedOnUpdate) {
        capturedOnUpdate();
      }
      return { kill: mockKill };
    }),
  },
}));

describe('useAnimatedValue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    capturedOnUpdate = undefined;
    capturedTarget = undefined;
    capturedConfig = undefined;
  });

  it('should initialize with value 0 before animation completes', async () => {
    // Override mock to NOT call onUpdate immediately (simulates pending animation)
    const gsap = (await import('gsap')).default;
    (gsap.to as ReturnType<typeof vi.fn>).mockImplementationOnce(
      (target: Record<string, unknown>, config: Record<string, unknown>) => {
        capturedTarget = target;
        capturedConfig = config;
        capturedOnUpdate = config.onUpdate as () => void;
        // Do not invoke onUpdate — animation is still in progress
        return { kill: mockKill };
      }
    );

    const { result } = renderHook(() => useAnimatedValue(100));
    expect(result.current).toBe(0);
  });

  it('should animate to the target value when gsap completes', () => {
    const { result } = renderHook(() => useAnimatedValue(75, 2, 'power2.inOut'));

    // The mock immediately sets value and calls onUpdate
    expect(result.current).toBe(75);
  });

  it('should pass correct duration and ease to gsap.to', async () => {
    const gsap = (await import('gsap')).default;

    renderHook(() => useAnimatedValue(50, 3, 'linear'));

    expect(gsap.to).toHaveBeenCalledWith(
      expect.objectContaining({ value: 50 }),
      expect.objectContaining({
        value: 50,
        duration: 3,
        ease: 'linear',
        onUpdate: expect.any(Function),
      })
    );
  });

  it('should kill previous tween when target changes', async () => {
    const gsap = (await import('gsap')).default;

    const { rerender } = renderHook(
      ({ target }) => useAnimatedValue(target),
      { initialProps: { target: 10 } }
    );

    // First render creates a tween
    expect(gsap.to).toHaveBeenCalledTimes(1);

    // Rerender with new target — should kill old tween and create new
    rerender({ target: 50 });

    expect(mockKill).toHaveBeenCalled();
    expect(gsap.to).toHaveBeenCalledTimes(2);
  });

  it('should kill tween on unmount (cleanup)', () => {
    const { unmount } = renderHook(() => useAnimatedValue(42));

    unmount();

    // kill() is called in cleanup
    expect(mockKill).toHaveBeenCalled();
  });
});

describe('useAnimatedRoundedValue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should return a rounded integer of the animated value', async () => {
    // Override mock to set a fractional value
    const gsap = (await import('gsap')).default;
    (gsap.to as ReturnType<typeof vi.fn>).mockImplementationOnce(
      (target: Record<string, unknown>, config: Record<string, unknown>) => {
        target.value = 73.7;
        const onUpdate = config.onUpdate as () => void;
        if (onUpdate) onUpdate();
        return { kill: mockKill };
      }
    );

    const { result } = renderHook(() => useAnimatedRoundedValue(74));

    expect(result.current).toBe(74); // Math.round(73.7) = 74
    expect(Number.isInteger(result.current)).toBe(true);
  });
});