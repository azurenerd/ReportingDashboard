import React, { Suspense } from 'react';
import { Canvas } from '@react-three/fiber';
import { OrbitControls, PerformanceMonitor } from '@react-three/drei';
import SceneSetup from './SceneSetup';
import ParticleBackground from './ParticleBackground';
import PostProcessing from './PostProcessing';
import CameraController from './CameraController';
import HierarchyGraph from './HierarchyGraph';
import RiskRadar from './RiskRadar';
import Timeline3D from './Timeline3D';

interface MainCanvasProps {
  onCreated?: () => void;
}

/**
 * MainCanvas: R3F Canvas container with all 3D scene components.
 * Configures WebGL renderer for performance, sets up OrbitControls,
 * and composes the scene graph from child components.
 */
export default function MainCanvas({ onCreated }: MainCanvasProps) {
  return (
    <Canvas
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100vw',
        height: '100vh',
        zIndex: 0,
      }}
      gl={{
        antialias: true,
        alpha: false,
        powerPreference: 'high-performance',
        stencil: false,
        depth: true,
      }}
      camera={{ position: [0, 10, 30], fov: 60, near: 0.1, far: 200 }}
      dpr={[1, 2]}
      onCreated={({ gl }) => {
        gl.toneMapping = 3; // ACESFilmicToneMapping
        gl.toneMappingExposure = 1.0;
        onCreated?.();
      }}
      <PerformanceMonitor>
        {/* Base scene: lights, fog, background */}
        <SceneSetup />

        {/* Camera controls: orbit interaction */}
        <CameraController />

        {/* Instanced particle field on bloom layer 1 */}
        <ParticleBackground />

        {/* Lazy-loaded visualization components (stubs until their tasks complete) */}
        <Suspense fallback={null}>
          <HierarchyGraph />
          <RiskRadar />
          <Timeline3D />
        </Suspense>

        {/* Post-processing: selective bloom + vignette */}
        <PostProcessing />
      </PerformanceMonitor>
    </Canvas>
  );
}