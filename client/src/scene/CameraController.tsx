import { useRef, useEffect, useState } from 'react';
import { useThree, useFrame } from '@react-three/fiber';
import { OrbitControls } from '@react-three/drei';
import gsap from 'gsap';
import * as THREE from 'three';
import { useCameraFocus } from '../hooks/useCameraFocus';

/**
 * CameraController — Cinematic fly-in animation + OrbitControls + click-to-focus.
 *
 * On mount, the camera starts at (0, 20, 50) and GSAP animates it to (0, 5, 15)
 * over 3 seconds with power2.inOut easing. OrbitControls are disabled during the
 * fly-in and re-enabled once the animation completes. The useCameraFocus hook
 * handles subsequent focus-on-click animations triggered via the dashboard store.
 */
export default function CameraController() {
  const { camera } = useThree();
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const controlsRef = useRef<any>(null);
  const [controlsEnabled, setControlsEnabled] = useState(false);
  const [flyInComplete, setFlyInComplete] = useState(false);

  // Wire up the focus-on-click behavior (reads focusTarget from store)
  useCameraFocus(controlsRef, flyInComplete);

  useEffect(() => {
    // Set camera to starting position for the cinematic fly-in
    camera.position.set(0, 20, 50);
    camera.lookAt(0, 0, 0);

    // Animate camera position from start → end over 3 seconds
    const tl = gsap.timeline({
      onComplete: () => {
        setControlsEnabled(true);
        setFlyInComplete(true);
      },
    });

    tl.to(camera.position, {
      x: 0,
      y: 5,
      z: 15,
      duration: 3,
      ease: 'power2.inOut',
      onUpdate: () => {
        // Keep camera looking at origin during fly-in
        camera.lookAt(0, 0, 0);
        // Sync orbit controls target if available
        if (controlsRef.current) {
          controlsRef.current.target.set(0, 0, 0);
          controlsRef.current.update();
        }
      },
    });

    return () => {
      tl.kill();
    };
  }, [camera]);

  return (
    <OrbitControls
      ref={controlsRef}
      enabled={controlsEnabled}
      enableDamping
      dampingFactor={0.05}
      minDistance={5}
      maxDistance={60}
      maxPolarAngle={Math.PI * 0.85}
    />
  );
}