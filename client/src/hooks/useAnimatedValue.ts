import { useRef, useEffect, useState } from 'react';
import gsap from 'gsap';

/**
 * GSAP-powered animated number interpolation hook.
 * Smoothly animates from the current displayed value to the target value.
 * Used for animated counters (completion %, health score, days remaining, etc.).
 *
 * @param target - The target numeric value to animate towards
 * @param duration - Animation duration in seconds (default: 1s)
 * @param ease - GSAP easing function string (default: 'power2.out')
 * @returns The current interpolated value (rounded to nearest integer)
 */
export function useAnimatedValue(
  target: number,
  duration = 1,
  ease = 'power2.out'
): number {
  const [value, setValue] = useState(0);
  const tweenRef = useRef<gsap.core.Tween | null>(null);
  const objRef = useRef({ val: 0 });

  useEffect(() => {
    // Kill any existing tween before starting a new one
    if (tweenRef.current) {
      tweenRef.current.kill();
    }

    tweenRef.current = gsap.to(objRef.current, {
      val: target,
      duration,
      ease,
      onUpdate: () => {
        setValue(Math.round(objRef.current.val));
      },
    });

    return () => {
      if (tweenRef.current) {
        tweenRef.current.kill();
        tweenRef.current = null;
      }
    };
  }, [target, duration, ease]);

  return value;
}