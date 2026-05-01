import { useEffect, useCallback, useRef } from 'react';
import { useThree } from '@react-three/fiber';
import gsap from 'gsap';
import * as THREE from 'three';
import { useDashboard } from '../store/dashboardStore';

/**
 * useCameraFocus — Smooth camera focus transitions triggered by the dashboard store.
 *
 * When `focusTarget` in the store changes to a non-null position, this hook animates
 * the camera to an offset viewing position looking at the target. Used by T9 (ProjectHierarchy)
 * and T10 (RiskRadar) click handlers to focus the camera on clicked nodes.
 *
 * @param controlsRef - Ref to the OrbitControls instance (for updating the look-at target)
 * @param enabled - Whether focus animations are allowed (false during initial fly-in)
 * @returns Object with `focusOnPosition` function for imperative use
 */
export function useCameraFocus(
  controlsRef: React.RefObject<any>,
  enabled: boolean = true
): { focusOnPosition: (target: [number, number, number]) => void } {
  const { camera } = useThree();
  const { focusTarget } = useDashboard();
  const tweenRef = useRef<gsap.core.Tween | null>(null);

  /**
   * Smoothly animate camera to view a target position.
   * Camera moves to an offset (slightly above and behind the target) and
   * the OrbitControls target updates to the focus point.
   */
  const focusOnPosition = useCallback(
    (target: [number, number, number]) => {
      if (!enabled) return;

      // Kill any in-progress focus animation
      if (tweenRef.current) {
        tweenRef.current.kill();
      }

      const targetVec = new THREE.Vector3(...target);

      // Calculate camera offset: position the camera slightly above and behind the target
      const direction = new THREE.Vector3()
        .subVectors(camera.position, targetVec)
        .normalize();
      const offset = direction.multiplyScalar(8);
      const cameraTarget = targetVec.clone().add(offset);
      cameraTarget.y = Math.max(cameraTarget.y, target[1] + 3);

      // Animate camera position
      tweenRef.current = gsap.to(camera.position, {
        x: cameraTarget.x,
        y: cameraTarget.y,
        z: cameraTarget.z,
        duration: 1.5,
        ease: 'power2.inOut',
        onUpdate: () => {
          // Smoothly update the controls look-at target
          if (controlsRef.current) {
            controlsRef.current.target.lerp(targetVec, 0.1);
            controlsRef.current.update();
          }
        },
        onComplete: () => {
          // Snap controls target to final position
          if (controlsRef.current) {
            controlsRef.current.target.copy(targetVec);
            controlsRef.current.update();
          }
        },
      });
    },
    [camera, controlsRef, enabled]
  );

  // React to focusTarget changes from the dashboard store
  useEffect(() => {
    if (focusTarget && enabled) {
      focusOnPosition(focusTarget);
    }
  }, [focusTarget, focusOnPosition, enabled]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (tweenRef.current) {
        tweenRef.current.kill();
      }
    };
  }, []);

  return { focusOnPosition };
}