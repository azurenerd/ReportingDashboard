import { useRef, useMemo } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';

const PARTICLE_COUNT = 2000;
const SPREAD = 60;

/**
 * Deterministic seeded PRNG (Mulberry32).
 * Ensures particle positions are identical across mounts and HMR reloads.
 */
function mulberry32(seed: number): () => number {
  return () => {
    seed |= 0;
    seed = (seed + 0x6d2b79f5) | 0;
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

/**
 * ParticleBackground: 2000 instanced-mesh particles with sine-wave floating animation.
 * Uses a single InstancedMesh for one draw call (performance budget: <100 draw calls total).
 * Particles are assigned to layer 1 for selective bloom (AC #5, #6).
 */
export default function ParticleBackground() {
  const meshRef = useRef<THREE.InstancedMesh>(null);
  const needsInit = useRef(true);
  const dummy = useMemo(() => new THREE.Object3D(), []);

  // Pre-compute deterministic initial positions and per-particle animation parameters
  const particleData = useMemo(() => {
    const rng = mulberry32(42); // Fixed seed for determinism
    const data = new Float32Array(PARTICLE_COUNT * 5); // x, y, z, speed, phase
    for (let i = 0; i < PARTICLE_COUNT; i++) {
      const idx = i * 5;
      data[idx] = (rng() - 0.5) * SPREAD * 2;     // x
      data[idx + 1] = (rng() - 0.5) * SPREAD * 2; // y
      data[idx + 2] = (rng() - 0.5) * SPREAD * 2; // z
      data[idx + 3] = 0.3 + rng() * 0.7;          // speed multiplier
      data[idx + 4] = rng() * Math.PI * 2;        // phase offset
    }
    return data;
  }, []);

  // Per-particle scale factors (deterministic, separate seed)
  const scaleData = useMemo(() => {
    const rng = mulberry32(123);
    const scales = new Float32Array(PARTICLE_COUNT);
    for (let i = 0; i < PARTICLE_COUNT; i++) {
      scales[i] = 0.8 + rng() * 0.4;
    }
    return scales;
  }, []);

  // Animate particles with sine-wave vertical drift each frame.
  // On first frame, initialize instance matrices (ref is available inside useFrame).
  useFrame(({ clock }) => {
    if (!meshRef.current) return;
    const time = clock.getElapsedTime();

    // First-frame initialization: set starting positions
    if (needsInit.current) {
      needsInit.current = false;
      for (let i = 0; i < PARTICLE_COUNT; i++) {
        const idx = i * 5;
        dummy.position.set(particleData[idx], particleData[idx + 1], particleData[idx + 2]);
        dummy.scale.setScalar(scaleData[i]);
        dummy.updateMatrix();
        meshRef.current!.setMatrixAt(i, dummy.matrix);
      }
      meshRef.current.instanceMatrix.needsUpdate = true;
      return;
    }

    for (let i = 0; i < PARTICLE_COUNT; i++) {
      const idx = i * 5;
      const baseX = particleData[idx];
      const baseY = particleData[idx + 1];
      const baseZ = particleData[idx + 2];
      const speed = particleData[idx + 3];
      const phase = particleData[idx + 4];

      // Sine-wave vertical drift with per-particle variation
      const yOffset = Math.sin(time * speed + phase) * 1.2;
      // Subtle horizontal sway
      const xOffset = Math.sin(time * speed * 0.3 + phase * 2) * 0.3;

      dummy.position.set(baseX + xOffset, baseY + yOffset, baseZ);
      dummy.scale.setScalar(scaleData[i] + Math.sin(time * 0.5 + phase) * 0.1);
      dummy.updateMatrix();
      meshRef.current!.setMatrixAt(i, dummy.matrix);
    }
    meshRef.current.instanceMatrix.needsUpdate = true;
  });

  return (
    <instancedMesh
      ref={meshRef}
      args={[undefined, undefined, PARTICLE_COUNT]}
      frustumCulled={false}
      layers={1}
      {/* Low-poly icosahedron for each particle - minimal vertex count */}
      <icosahedronGeometry args={[0.04, 0]} />
      {/* Emissive material ensures particles exceed bloom luminance threshold */}
      <meshBasicMaterial
        color="#00d4ff"
        transparent
        opacity={0.7}
        toneMapped={false}
      />
    </instancedMesh>
  );
}