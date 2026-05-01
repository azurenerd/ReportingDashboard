import { useRef, useEffect, useState, useCallback } from 'react';
import gsap from 'gsap';

/**
 * useAnimatedValue — GSAP-powered number interpolation hook.
 *
 * Smoothly animates a numeric value from its current state to the target value
 * using GSAP's tweening engine. Ideal for animated counters, progress indicators,
 * and any UI element that needs to smoothly transition between numeric states.
 *
 * @param target - The target number to animate toward
 * @param duration - Animation duration in seconds (default: 1.5)
 * @param ease - GSAP ease string (default: 'power2.out')
 * @returns The current animated numeric value
 */
export function useAnimatedValue(
  target: number,
  duration: number = 1.5,
  ease: string = 'power2.out'
): number {
  const [value, setValue] = useState(0);
  const tweenRef = useRef<gsap.core.Tween | null>(null);
  const objRef = useRef({ value: 0 });

  useEffect(() => {
    // Kill any existing tween before starting a new one
    if (tweenRef.current) {
      tweenRef.current.kill();
    }

    tweenRef.current = gsap.to(objRef.current, {
      value: target,
      duration,
      ease,
      onUpdate: () => {
        setValue(objRef.current.value);
      },
    });

    return () => {
      if (tweenRef.current) {
        tweenRef.current.kill();
      }
    };
  }, [target, duration, ease]);

  return value;
}

/**
 * useAnimatedRoundedValue — Same as useAnimatedValue but returns a rounded integer.
 * Useful for percentage counters and discrete numeric displays.
 *
 * @param target - The target integer to animate toward
 * @param duration - Animation duration in seconds (default: 1.5)
 * @param ease - GSAP ease string (default: 'power2.out')
 * @returns The current animated value, rounded to nearest integer
 */
export function useAnimatedRoundedValue(
  target: number,
  duration: number = 1.5,
  ease: string = 'power2.out'
): number {
  const animatedValue = useAnimatedValue(target, duration, ease);
  return Math.round(animatedValue);
}