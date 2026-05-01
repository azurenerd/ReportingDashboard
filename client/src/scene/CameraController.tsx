import { useRef, useEffect } from 'react';
import { useThree, useFrame } from '@react-three/fiber';
import { OrbitControls } from '@react-three/drei';
import gsap from 'gsap';
import * as THREE from 'three';
import { useCameraFocus } from '../hooks/useCameraFocus';

/**
 * Cinematic camera controller with GSAP-powered fly-in animation.
 * On page load, the camera sweeps from a distant elevated position to the
 * default viewing position over ~3 seconds with power2.inOut easing.
 * OrbitControls are disabled during the animation and re-enabled on completion.
 * Also handles focus-on-target animations triggered via the dashboard store.
 */

// Fly-in start/end positions
const FLY_IN_START = new THREE.Vector3(0, 20, 50);
const FLY_IN_END = new THREE.Vector3(0, 5, 15);
const FLY_IN_DURATION = 3; // seconds
const FLY_IN_LOOK_AT = new THREE.Vector3(0, 0, 0);

export default function CameraController() {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const controlsRef = useRef<any>(null);
  const hasAnimatedRef = useRef(false);
  const isAnimatingRef = useRef(true); // Start true to block OrbitControls on first frame
  const camera = useThree((state) => state.camera);

  // Bridge dashboardStore.focusTarget → camera animation
  useCameraFocus(controlsRef, isAnimatingRef);

  // Fly-in animation — runs once on mount
  useEffect(() => {
    if (hasAnimatedRef.current) return;
    hasAnimatedRef.current = true;

    // Position camera at fly-in start
    camera.position.copy(FLY_IN_START);
    camera.lookAt(FLY_IN_LOOK_AT);

    // Disable OrbitControls during fly-in
    if (controlsRef.current) {
      controlsRef.current.enabled = false;
    }

    // Animate camera position with GSAP
    const pos = { x: FLY_IN_START.x, y: FLY_IN_START.y, z: FLY_IN_START.z };

    gsap.to(pos, {
      x: FLY_IN_END.x,
      y: FLY_IN_END.y,
      z: FLY_IN_END.z,
      duration: FLY_IN_DURATION,
      ease: 'power2.inOut',
      onUpdate: () => {
        camera.position.set(pos.x, pos.y, pos.z);
        camera.lookAt(FLY_IN_LOOK_AT);
        // Keep OrbitControls target synced so handoff is seamless
        if (controlsRef.current) {
          controlsRef.current.target.copy(FLY_IN_LOOK_AT);
        }
      },
      onComplete: () => {
        isAnimatingRef.current = false;
        if (controlsRef.current) {
          controlsRef.current.enabled = true;
          controlsRef.current.update();
        }
      },
    });
  }, [camera]);

  // Suppress OrbitControls during any active animation by syncing each frame
  useFrame(() => {
    if (isAnimatingRef.current && controlsRef.current) {
      controlsRef.current.enabled = false;
    }
  });

  return (
    <OrbitControls
      ref={controlsRef}
      makeDefault
      enableDamping
      dampingFactor={0.05}
      minDistance={5}
      maxDistance={60}
      maxPolarAngle={Math.PI * 0.85}
      enabled={false} // Start disabled; enabled after fly-in
    />
  );
}