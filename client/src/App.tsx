import React, { Suspense } from 'react';
import { Canvas } from '@react-three/fiber';
import { OrbitControls, PerformanceMonitor } from '@react-three/drei';
import { DashboardProvider } from './store/dashboardStore';
import WebGLFallback from './components/WebGLFallback';
import DashboardCards from './components/DashboardCards';
import SprintCharts from './components/SprintCharts';
import ActivityFeed from './components/ActivityFeed';
import DetailPanel from './components/DetailPanel';
import SceneSetup from './scene/SceneSetup';
import ParticleBackground from './scene/ParticleBackground';
import PostProcessing from './scene/PostProcessing';
import './styles/globals.css';

/**
 * App: Root layout orchestrator.
 * Gates rendering on WebGL2 support, wraps in DashboardProvider context,
 * renders the 3D Canvas with Suspense fallback, and overlays HTML UI panels.
 * Consumes dashboardStore.focusTarget (read-only) - does not modify the store.
 */
export default function App() {
  return (
    <DashboardProvider>
      <WebGLFallback>
        {/* 3D scene wrapped in Suspense for async-loaded resources */}
        <Suspense fallback={null}>
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
            }}
            <PerformanceMonitor>
              {/* Base scene: lights, fog, background color #0a0a1a */}
              <SceneSetup />

              {/* OrbitControls for interactive camera navigation (AC #10) */}
              <OrbitControls
                enableDamping
                dampingFactor={0.05}
                minDistance={5}
                maxDistance={100}
                maxPolarAngle={Math.PI * 0.85}
                enablePan
              />

              {/* 2000 instanced-mesh particles with sine-wave animation (AC #4, #5) */}
              <ParticleBackground />

              {/* Selective bloom + vignette post-processing (AC #6) */}
              <PostProcessing />
            </PerformanceMonitor>
          </Canvas>
        </Suspense>

        {/* HTML overlay layer - pointer-events: none so 3D interactions pass through */}
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            width: '100vw',
            height: '100vh',
            zIndex: 10,
            pointerEvents: 'none',
          }}
          <DashboardCards />
          <SprintCharts />
          <ActivityFeed />
        </div>

        {/* Detail panel overlays everything when open */}
        <DetailPanel />
      </WebGLFallback>
    </DashboardProvider>
  );
}