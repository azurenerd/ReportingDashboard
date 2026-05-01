/**
 * PostProcessing — Bloom via @react-three/postprocessing EffectComposer.
 *
 * Uses luminance-based bloom: objects with emissive materials that exceed the
 * luminanceThreshold will glow. The camera has layer 1 enabled so bloom-tagged
 * objects (e.g. particles, accent meshes) are visible in the render pass.
 *
 * Settings tuned for the dark futuristic aesthetic:
 *   - luminanceThreshold 0.2: only bright emissive surfaces bloom
 *   - intensity 1.5: pronounced but not overpowering glow
 *   - radius 0.8: soft bloom spread
 *   - mipmapBlur: high-quality multi-resolution blur
 *
 * HTML overlays are unaffected because they live outside the Canvas and are
 * composited by the browser after the WebGL framebuffer.
 */
import { useThree } from '@react-three/fiber';
import { useEffect } from 'react';
import { EffectComposer, Bloom } from '@react-three/postprocessing';

export default function PostProcessing() {
  const { camera } = useThree();

  // Enable layer 1 on camera so bloom-tagged objects are visible in the render
  useEffect(() => {
    camera.layers.enable(1);
  }, [camera]);

  return (
    <EffectComposer>
      <Bloom
        intensity={1.5}
        luminanceThreshold={0.2}
        luminanceSmoothing={0.9}
        radius={0.8}
        mipmapBlur
      />
    </EffectComposer>
  );
}