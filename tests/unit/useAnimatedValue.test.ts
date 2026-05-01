import { describe, it, expect, vi, beforeEach, type Mock } from 'vitest';
import { renderHook, act } from '@testing-library/react';

// Track kill calls manually
let killFn: Mock;
let toFn: Mock;

// Mock gsap before importing the hook
vi.mock('gsap', () => {
  killFn = vi.fn();
  toFn = vi.fn(() => ({ kill: killFn }));
  return {
    default: { to: toFn },
  };
});

import { useAnimatedValue } from '../../client/src/hooks/useAnimatedValue';

describe('useAnimatedValue', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('returns 0 as the initial value before animation starts', () => {
    const { result } = renderHook(() => useAnimatedValue(100));
    expect(result.current).toBe(0);
  });

  it('calls gsap.to with correct target, duration, and ease defaults', () => {
    renderHook(() => useAnimatedValue(75));

    expect(toFn).toHaveBeenCalledTimes(1);
    const [targetObj, config] = toFn.mock.calls[0];
    expect(targetObj).toEqual({ val: 0 });
    expect(config.val).toBe(75);
    expect(config.duration).toBe(1);
    expect(config.ease).toBe('power2.out');
    expect(typeof config.onUpdate).toBe('function');
  });

  it('passes custom duration and ease to gsap.to', () => {
    renderHook(() => useAnimatedValue(50, 2.5, 'power3.inOut'));

    expect(toFn).toHaveBeenCalledTimes(1);
    const config = toFn.mock.calls[0][1];
    expect(config.val).toBe(50);
    expect(config.duration).toBe(2.5);
    expect(config.ease).toBe('power3.inOut');
  });

  it('updates the returned value (rounded) when gsap onUpdate fires', () => {
    const { result } = renderHook(() => useAnimatedValue(100));

    const targetObj = toFn.mock.calls[0][0];
    const onUpdate = toFn.mock.calls[0][1].onUpdate;

    act(() => {
      targetObj.val = 42.7;
      onUpdate();
    });

    expect(result.current).toBe(43);

    act(() => {
      targetObj.val = 99.4;
      onUpdate();
    });

    expect(result.current).toBe(99);
  });

  it('kills the previous tween and creates a new one when target changes', () => {
    const { rerender } = renderHook(
      ({ target }) => useAnimatedValue(target),
      { initialProps: { target: 50 } }
    );

    expect(toFn).toHaveBeenCalledTimes(1);

    rerender({ target: 80 });

    expect(killFn).toHaveBeenCalled();
    expect(toFn).toHaveBeenCalledTimes(2);
    expect(toFn.mock.calls[1][1].val).toBe(80);
  });
});