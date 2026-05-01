import { useThree } from '@react-three/fiber';
import { useEffect } from 'react';
import * as THREE from 'three';

/**
 * SceneSetup: Configures the base 3D environment.
 * - Dark background (#0a0a1a) per AC #1
 * - Exponential fog matching background for depth fade (AC #3)
 * - Ambient light with dark-blue tint (AC #2)
 * - Three point lights: cyan, magenta, and warm white (AC #2)
 * - Camera layers.enable(1) so bloom-tagged objects are visible (AC #7)
 */
export default function SceneSetup() {
  const { scene, camera } = useThree();

  useEffect(() => {
    // Set dark background matching the futuristic theme
    scene.background = new THREE.Color('#0a0a1a');

    // Exponential fog for depth fade - objects distant from camera fade into darkness
    scene.fog = new THREE.FogExp2('#0a0a1a', 0.015);

    // Enable layer 1 on camera so bloom-tagged objects (layer 1) remain visible
    camera.layers.enable(1);
  }, [scene, camera]);

  return (
    <>
      {/* Ambient light with dark-blue tint for subtle fill */}
      <ambientLight color="#1a1a2e" intensity={0.4} />

      {/* Cyan point light - primary accent, positioned upper-right */}
      <pointLight
        color="#00d4ff"
        intensity={2.5}
        position={[15, 12, 10]}
        distance={60}
        decay={2}
      />

      {/* Magenta point light - secondary accent, positioned lower-left */}
      <pointLight
        color="#ff00ff"
        intensity={1.8}
        position={[-12, -8, -10]}
        distance={50}
        decay={2}
      />

      {/* Warm white point light - overhead key light */}
      <pointLight
        color="#ffffff"
        intensity={1.2}
        position={[0, 20, 5]}
        distance={80}
        decay={2}
      />
    </>
  );
}