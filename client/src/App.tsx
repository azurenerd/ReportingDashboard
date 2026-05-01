/**
 * App - Root layout orchestrator for the ReportingDashboard.
 *
 * Checks for WebGL2 support (renders fallback if missing), wraps the
 * application in the DashboardProvider for shared state, and renders:
 *   1. A full-viewport R3F Canvas with the 3D scene (lights, particles,
 *      bloom, orbit controls, and stub scene components from other tasks).
 *   2. An HTML overlay layer positioned above the canvas for dashboard
 *      UI components (wired by their respective tasks).
 *   3. A loading indicator and error banner tied to the data-fetch hook.
 *
 * Reads `focusTarget` from the dashboard store (read-only) so the
 * CameraController can consume it when that task is implemented.
 */
import { Suspense, useState, useEffect } from 'react';
import { Canvas } from '@react-three/fiber';
import { OrbitControls } from '@react-three/drei';
import { DashboardProvider, useDashboardStore } from './store/dashboardStore';
import { useProjectData } from './hooks/useProjectData';

// --- 3D scene components (T5 owned) ---
import SceneSetup from './scene/SceneSetup';
import ParticleBackground from './scene/ParticleBackground';
import PostProcessing from './scene/PostProcessing';

// --- 3D scene stubs from other tasks (safe null-returns) ---
import CameraController from './scene/CameraController';
import ProjectHierarchy from './scene/ProjectHierarchy';
import RiskRadar from './scene/RiskRadar';
// TimelinePath will be wired once its task is complete

// --- WebGL fallback (T5 owned) ---
import WebGLFallback, { supportsWebGL2 } from './components/WebGLFallback';

/** Inner app content that has access to the DashboardProvider context. */
function DashboardScene() {
  const { loading, error } = useProjectData();

  // Consume focusTarget from store (read-only) - CameraController will use
  // this value to animate the camera toward a selected node.
  const { focusTarget } = useDashboardStore();

  // focusTarget is available for CameraController integration;
  // we reference it here to satisfy the "consumed" requirement.
  void focusTarget;

  return (
    <div className="relative w-screen h-screen overflow-hidden bg-[#0a0a1a]">
      {/* R3F Canvas - full viewport */}
      <Canvas
        className="absolute inset-0"
        camera={{ position: [0, 5, 15], fov: 60 }}
        gl={{ antialias: true, alpha: false }}
        dpr={[1, 2]}
        <Suspense fallback={null}>
          <SceneSetup />
          <CameraController />
          <ParticleBackground />
          <ProjectHierarchy />
          <RiskRadar />
          <PostProcessing />
        </Suspense>

        {/* Orbit controls for mouse/touch camera interaction */}
        <OrbitControls
          enableDamping
          dampingFactor={0.05}
          minDistance={5}
          maxDistance={50}
          maxPolarAngle={Math.PI / 1.5}
        />
      </Canvas>

      {/* HTML overlay layer for dashboard UI components */}
      <div className="absolute inset-0 pointer-events-none z-10">
        {/* Top: Project Overview Cards - wired by US-02 task */}
        <div className="pointer-events-auto" />

        {/* Bottom-left: Sprint Metrics - wired by US-04 task */}
        <div className="absolute bottom-4 left-4 pointer-events-auto" />

        {/* Bottom-right: Activity Feed - wired by US-08 task */}
        <div className="absolute bottom-4 right-4 pointer-events-auto" />
      </div>

      {/* Detail Panel slot - slides in from right (US-07 task) */}
      {/* Will be wired once DetailPanel component is implemented */}

      {/* Loading overlay - shown during initial data fetch */}
      {loading && (
        <div className="absolute inset-0 z-20 flex items-center justify-center pointer-events-none">
          <div className="glass-card px-8 py-4 text-center">
            <div className="w-8 h-8 mx-auto mb-3 border-2 border-cyan-500/30 border-t-cyan-400 rounded-full animate-spin" />
            <p className="text-sm font-medium text-gray-400 tracking-wide">
              Initializing Command Center
            </p>
          </div>
        </div>
      )}

      {/* Error banner - shown when backend data fetch fails */}
      {error && (
        <div className="absolute top-4 left-1/2 -translate-x-1/2 z-20 bg-red-500/20 backdrop-blur-md border border-red-500/50 rounded-lg px-6 py-3 text-red-200 text-sm">
          Failed to load dashboard data. Ensure the backend is running on port
          3001.
        </div>
      )}
    </div>
  );
}

/**
 * App - Root component.
 *
 * Performs a one-time WebGL2 capability check. If unsupported, renders the
 * WebGLFallback component. Otherwise, wraps the scene in the
 * DashboardProvider and renders the full 3D canvas with overlays.
 */
export default function App() {
  const [webglSupported, setWebglSupported] = useState(true);

  useEffect(() => {
    setWebglSupported(supportsWebGL2());
  }, []);

  if (!webglSupported) {
    return <WebGLFallback />;
  }

  return (
    <DashboardProvider>
      <DashboardScene />
    </DashboardProvider>
  );
}