/**
 * ParticleBackground - 2000 instanced-mesh particles with sine-wave floating
 * animation.
 *
 * Uses a single InstancedMesh draw call for performance (stays well under the
 * <100 draw-call budget). Particles are tiny emissive cyan spheres scattered
 * in a large volume, gently drifting via sine-wave Y/X/Z offsets. The mesh is
 * assigned to Three.js layer 1 so it participates in selective bloom.
 *
 * Initial positions and phase offsets are computed once via useMemo to avoid
 * GC pressure. A reusable Object3D dummy is used for per-frame matrix updates.
 */
import { useRef, useMemo, useEffect } from 'react';
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

  // Reusable Object3D for matrix computation - avoids allocation each frame
  const dummy = useMemo(() => new THREE.Object3D(), []);

  // Assign the instanced mesh to Three.js layer 1 so bloom targets it
  useEffect(() => {
    if (meshRef.current) {
      meshRef.current.layers.enable(1);
    }
  }, []);

  // Animate particles each frame - sine-wave vertical drift with slight sway
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
    <instancedMesh
      ref={meshRef}
      args={[undefined, undefined, PARTICLE_COUNT]}
      frustumCulled={false}
      <sphereGeometry args={[0.03, 6, 6]} />
      {/* Emissive material ensures particles pass the bloom luminance threshold */}
      <meshStandardMaterial
        color="#00f0ff"
        emissive="#00f0ff"
        emissiveIntensity={0.8}
        transparent
        opacity={0.6}
      />
    </instancedMesh>
  );
}