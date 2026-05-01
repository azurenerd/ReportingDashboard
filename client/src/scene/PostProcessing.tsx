/**
 * PostProcessing - Selective bloom via Three.js render layers.
 * Only objects assigned to layer 1 receive bloom, preventing UI washout.
 * Uses @react-three/postprocessing EffectComposer with Bloom effect.
 */
import { useThree } from '@react-three/fiber';
import { useEffect } from 'react';
import { EffectComposer, Bloom } from '@react-three/postprocessing';
import { BlendFunction } from 'postprocessing';

export default function PostProcessing() {
  const { camera } = useThree();

  // Enable layer 1 on camera so bloom-tagged objects are visible
  useEffect(() => {
    camera.layers.enable(1);
  }, [camera]);

  return (
    <EffectComposer>
      <Bloom
        intensity={0.8}
        luminanceThreshold={0.2}
        luminanceSmoothing={0.9}
        mipmapBlur
        blendFunction={BlendFunction.ADD}
      />
    </EffectComposer>
  );
}