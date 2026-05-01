import { EffectComposer, Bloom, Vignette } from '@react-three/postprocessing';
import { BlendFunction } from 'postprocessing';

/**
 * PostProcessing: Selective bloom on layer 1 via EffectComposer (AC #6).
 * Bloom uses luminanceThreshold to only affect bright/emissive objects (particles, glow accents).
 * Non-emissive scene objects remain unaffected - achieving selective bloom without custom multi-pass.
 * Vignette adds cinematic edge darkening.
 */
export default function PostProcessing() {
  return (
    <EffectComposer>
      <Bloom
        intensity={1.2}
        luminanceThreshold={0.6}
        luminanceSmoothing={0.4}
        mipmapBlur
        radius={0.8}
      />
      <Vignette
        offset={0.3}
        darkness={0.7}
        blendFunction={BlendFunction.NORMAL}
      />
    </EffectComposer>
  );
}