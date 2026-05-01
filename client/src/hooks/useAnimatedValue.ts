import { useRef, useEffect, useState } from 'react';

/**
 * Animates a numeric value from 0 to target using requestAnimationFrame.
 * Stub implementation — will be enhanced with GSAP in a later task.
 */
export function useAnimatedValue(target: number, duration = 1000): number {
  const [value, setValue] = useState(0);
  const startTime = useRef<number | null>(null);

  useEffect(() => {
    startTime.current = null;

    function animate(timestamp: number) {
      if (startTime.current === null) startTime.current = timestamp;
      const elapsed = timestamp - startTime.current;
      const progress = Math.min(elapsed / duration, 1);
      setValue(Math.round(target * progress));
      if (progress < 1) requestAnimationFrame(animate);
    }

    requestAnimationFrame(animate);
  }, [target, duration]);

  return value;
}