/**
 * SceneSetup — Lights, fog, and environment for the dark futuristic scene.
 * Ambient light tinted dark blue (#1a1a2e), cyan and magenta point lights
 * for neon accent illumination, and exponential fog for depth fade.
 */
import { useThree } from '@react-three/fiber';
import { useEffect } from 'react';
import * as THREE from 'three';

export default function SceneSetup() {
  const { scene } = useThree();

  // Set dark background and fog once on mount
  useEffect(() => {
    scene.background = new THREE.Color('#0a0a1a');
    scene.fog = new THREE.FogExp2('#0a0a1a', 0.015);
  }, [scene]);

  return (
    <>
      {/* Low-intensity ambient with dark-blue tint for base fill */}
      <ambientLight color="#1a1a2e" intensity={0.4} />

      {/* Cyan accent light — right side, elevated */}
      <pointLight position={[15, 10, 10]} color="#00f0ff" intensity={80} distance={60} decay={2} />

      {/* Magenta accent light — left side, lower */}
      <pointLight position={[-15, -5, -10]} color="#ff00ff" intensity={60} distance={50} decay={2} />

      {/* Soft white fill from above to prevent pure-black shadows */}
      <pointLight position={[0, 20, 0]} color="#ffffff" intensity={20} distance={80} decay={2} />
    </>
  );
}