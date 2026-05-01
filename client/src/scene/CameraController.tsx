import { useEffect } from 'react';
import { useThree } from '@react-three/fiber';
import { OrbitControls } from '@react-three/drei';

/**
 * CameraController: Provides OrbitControls for interactive scene navigation.
 * T5 scope: OrbitControls only. Camera fly-in animation is a separate task (T6).
 */
export default function CameraController() {
  const { camera } = useThree();

  useEffect(() => {
    // Enable layer 1 on camera so bloom-layer objects are visible
    camera.layers.enable(1);
  }, [camera]);

  return (
    <OrbitControls
      enableDamping
      dampingFactor={0.05}
      minDistance={5}
      maxDistance={100}
      maxPolarAngle={Math.PI * 0.85}
      enablePan
    />
  );
}