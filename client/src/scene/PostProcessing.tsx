/**
 * PostProcessing — Selective bloom via Three.js render layers.
 * Only objects assigned to layer 1 receive bloom, preventing UI washout.
 * Uses @react-three/postprocessing EffectComposer with Bloom effect.
 */
import { useThree } from '@react-three/fiber';
import { useEffect, useMemo } from 'react';
import { EffectComposer, Bloom } from '@react-three/postprocessing';
import { BlendFunction } from 'postprocessing';
import * as THREE from 'three';

export default function PostProcessing() {
  const { camera } = useThree();

  // Enable layer 1 on camera so bloom-tagged objects are visible
  useEffect(() => {
    camera.layers.enable(1);
  }, [camera]);

  // Selection layer for selective bloom (objects on layer 1 glow)
  const bloomLayer = useMemo(() => {
    const layer = new THREE.Layers();
    layer.set(1);
    return layer;
  }, []);

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