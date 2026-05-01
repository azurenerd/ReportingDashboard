/**
 * ParticleBackground — 2000 instanced-mesh particles with sine-wave floating animation.
 * Uses a single InstancedMesh draw call for performance (<100 draw calls budget).
 * Particles are tiny cyan/white spheres scattered in a large volume, gently drifting.
 */
import { useRef, useMemo } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';

const PARTICLE_COUNT = 2000;
const SPREAD = 60; // spatial spread on each axis

export default function ParticleBackground() {
  const meshRef = useRef<THREE.InstancedMesh>(null);

  // Pre-compute random seed positions and phase offsets (deterministic per mount)
  const particles = useMemo(() => {
    const data = new Float32Array(PARTICLE_COUNT * 4); // x, y, z, phase
    for (let i = 0; i < PARTICLE_COUNT; i++) {
      const idx = i * 4;
      data[idx] = (Math.random() - 0.5) * SPREAD;
      data[idx + 1] = (Math.random() - 0.5) * SPREAD;
      data[idx + 2] = (Math.random() - 0.5) * SPREAD;
      data[idx + 3] = Math.random() * Math.PI * 2; // phase offset
    }
    return data;
  }, []);

  const dummy = useMemo(() => new THREE.Object3D(), []);

  // Animate particles each frame — sine-wave vertical drift
  useFrame(({ clock }) => {
    const mesh = meshRef.current;
    if (!mesh) return;
    const t = clock.getElapsedTime();

    for (let i = 0; i < PARTICLE_COUNT; i++) {
      const idx = i * 4;
      const baseX = particles[idx];
      const baseY = particles[idx + 1];
      const baseZ = particles[idx + 2];
      const phase = particles[idx + 3];

      // Gentle sine-wave motion on Y with slight X/Z sway
      dummy.position.set(
        baseX + Math.sin(t * 0.3 + phase) * 0.4,
        baseY + Math.sin(t * 0.5 + phase) * 0.8,
        baseZ + Math.cos(t * 0.4 + phase) * 0.3,
      );
      dummy.updateMatrix();
      mesh.setMatrixAt(i, dummy.matrix);
    }
    mesh.instanceMatrix.needsUpdate = true;
  });

  return (
    <instancedMesh ref={meshRef} args={[undefined, undefined, PARTICLE_COUNT]} frustumCulled={false}>
      <sphereGeometry args={[0.02, 6, 6]} />
      <meshBasicMaterial color="#00f0ff" transparent opacity={0.6} />
    </instancedMesh>
  );
}