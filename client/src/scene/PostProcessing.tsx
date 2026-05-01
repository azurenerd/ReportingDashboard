/**
 * PostProcessing — Bloom via luminance thresholds.
 * Uses @react-three/postprocessing EffectComposer with Bloom effect.
 * Objects on layer 1 are visible to the camera (layer enabled below);
 * bloom intensity is controlled by luminance threshold so only bright
 * emissive materials (assigned by other scene components) glow.
 */
import { useThree } from '@react-three/fiber';
import { useEffect } from 'react';
import { EffectComposer, Bloom } from '@react-three/postprocessing';
import { BlendFunction } from 'postprocessing';

export default function PostProcessing() {
  const { camera } = useThree();

  // Enable layer 1 on camera so bloom-tagged objects are visible in the render
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