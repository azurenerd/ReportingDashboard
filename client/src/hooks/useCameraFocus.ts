import { useEffect, useRef } from 'react';
import { useThree } from '@react-three/fiber';
import gsap from 'gsap';
import * as THREE from 'three';
import { useDashboardStore, type FocusTarget } from '../store/dashboardStore';

const FOCUS_DURATION = 1.5; // seconds for focus animation
const FOCUS_EASE = 'power2.inOut';
const CAMERA_OFFSET = new THREE.Vector3(0, 3, 8); // Offset from target when focusing

/**
 * Hook that bridges the dashboardStore's focusTarget to the CameraController.
 * When focusTarget changes in the store, this hook animates the camera
 * smoothly to focus on that 3D position.
 *
 * Must be called inside an R3F Canvas context (uses useThree).
 */
export function useCameraFocus(
  controlsRef: React.RefObject<any>,
  isAnimatingRef: React.MutableRefObject<boolean>
): void {
  const camera = useThree((state) => state.camera);
  const focusTarget = useDashboardStore((s) => s.focusTarget);
  const tweenRef = useRef<gsap.core.Tween | null>(null);

  useEffect(() => {
    if (!focusTarget || !controlsRef.current) return;

    // Kill any existing focus animation
    if (tweenRef.current) {
      tweenRef.current.kill();
    }

    // Mark animating to disable orbit controls
    isAnimatingRef.current = true;
    controlsRef.current.enabled = false;

    // Compute destination: offset from the target position
    const destination = new THREE.Vector3(
      focusTarget.x + CAMERA_OFFSET.x,
      focusTarget.y + CAMERA_OFFSET.y,
      focusTarget.z + CAMERA_OFFSET.z
    );

    const targetLookAt = new THREE.Vector3(focusTarget.x, focusTarget.y, focusTarget.z);

    // Animate camera position to the new focus point
    const pos = { x: camera.position.x, y: camera.position.y, z: camera.position.z };
    const lookAt = {
      x: controlsRef.current.target.x,
      y: controlsRef.current.target.y,
      z: controlsRef.current.target.z,
    };

    tweenRef.current = gsap.to(pos, {
      x: destination.x,
      y: destination.y,
      z: destination.z,
      duration: FOCUS_DURATION,
      ease: FOCUS_EASE,
      onUpdate: () => {
        camera.position.set(pos.x, pos.y, pos.z);

        // Interpolate lookAt target in sync (GSAP handles this via the shared timeline)
        const progress = tweenRef.current ? tweenRef.current.progress() : 1;
        const lx = lookAt.x + (targetLookAt.x - lookAt.x) * progress;
        const ly = lookAt.y + (targetLookAt.y - lookAt.y) * progress;
        const lz = lookAt.z + (targetLookAt.z - lookAt.z) * progress;

        camera.lookAt(lx, ly, lz);
        if (controlsRef.current) {
          controlsRef.current.target.set(lx, ly, lz);
        }
      },
      onComplete: () => {
        isAnimatingRef.current = false;
        if (controlsRef.current) {
          controlsRef.current.target.copy(targetLookAt);
          controlsRef.current.enabled = true;
          controlsRef.current.update();
        }
      },
    });

    return () => {
      if (tweenRef.current) {
        tweenRef.current.kill();
      }
    };
  }, [focusTarget, camera, controlsRef, isAnimatingRef]);
}

/**
 * Imperatively trigger a camera focus animation to a target position.
 * Call this from click handlers (e.g., T9 hierarchy nodes, T10 risk nodes).
 * The CameraController will pick up the change via useCameraFocus.
 */
export function focusOnPosition(target: FocusTarget): void {
  useDashboardStore.getState().setFocusTarget(target);
}