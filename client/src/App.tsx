import { Suspense, useState, useEffect } from 'react';
import { Canvas } from '@react-three/fiber';
import { DashboardProvider, useDashboard } from './store/dashboardStore';
import { useProjectData } from './hooks/useProjectData';

// 3D Scene components
import SceneSetup from './scene/SceneSetup';
import CameraController from './scene/CameraController';
import ParticleBackground from './scene/ParticleBackground';
import PostProcessing from './scene/PostProcessing';
import ProjectHierarchy from './scene/ProjectHierarchy';
import RiskRadar from './scene/RiskRadar';
import TimelinePath from './scene/TimelinePath';

// HTML overlay components
import DashboardCards from './components/DashboardCards';
import SprintCharts from './components/SprintCharts';
import ActivityFeed from './components/ActivityFeed';
import DetailPanel from './components/DetailPanel';
import LoadingScreen from './components/LoadingScreen';
import WebGLFallback from './components/WebGLFallback';

/**
 * Detects WebGL2 support in the current browser.
 */
function isWebGL2Supported(): boolean {
  try {
    const canvas = document.createElement('canvas');
    return !!(canvas.getContext('webgl2'));
  } catch {
    return false;
  }
}

/** Inner app content with access to dashboard context */
function DashboardContent() {
  const { data, loading, error } = useProjectData();
  const { selectedItemId } = useDashboard();

  if (loading) {
    return <LoadingScreen />;
  }

  return (
    <div className="relative w-screen h-screen overflow-hidden bg-slate-950">
      {/* 3D Canvas - full viewport */}
      <Canvas
        className="absolute inset-0"
        camera={{ position: [0, 0, 30], fov: 60 }}
        gl={{ antialias: true, alpha: false }}
        dpr={[1, 2]}
        <Suspense fallback={null}>
          <SceneSetup />
          <CameraController />
          <ParticleBackground />
          <ProjectHierarchy />
          <RiskRadar />
          <TimelinePath />
          <PostProcessing />
        </Suspense>
      </Canvas>

      {/* HTML Overlay - positioned above the canvas */}
      <div className="absolute inset-0 pointer-events-none">
        {/* Top: Project Overview Cards */}
        <div className="pointer-events-auto">
          <DashboardCards />
        </div>

        {/* Bottom-left: Sprint Metrics */}
        <div className="absolute bottom-4 left-4 pointer-events-auto">
          <SprintCharts />
        </div>

        {/* Bottom-right: Activity Feed */}
        <div className="absolute bottom-4 right-4 pointer-events-auto">
          <ActivityFeed />
        </div>
      </div>

      {/* Detail Panel - slides in from right when an item is selected */}
      {selectedItemId && (
        <div className="absolute inset-y-0 right-0 pointer-events-auto">
          <DetailPanel />
        </div>
      )}

      {/* Error overlay */}
      {error && (
        <div className="absolute top-4 left-1/2 -translate-x-1/2 bg-red-500/20 backdrop-blur-md border border-red-500/50 rounded-lg px-6 py-3 text-red-200 text-sm">
          Failed to load dashboard data. Ensure the backend is running on port 3001.
        </div>
      )}
    </div>
  );
}

/**
 * App - Root layout orchestrator
 *
 * Wraps the dashboard in the state provider, checks for WebGL2 support,
 * and renders the Canvas + HTML overlay layout.
 */
export default function App() {
  const [webglSupported, setWebglSupported] = useState(true);

  useEffect(() => {
    setWebglSupported(isWebGL2Supported());
  }, []);

  if (!webglSupported) {
    return <WebGLFallback />;
  }

  return (
    <DashboardProvider>
      <DashboardContent />
    </DashboardProvider>
  );
}