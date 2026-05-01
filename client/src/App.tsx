import { Canvas } from '@react-three/fiber';
import { Suspense } from 'react';
import { OrbitControls } from '@react-three/drei';
import { DashboardStoreProvider } from './store/dashboardStore';
import SceneSetup from './scene/SceneSetup';
import CameraController from './scene/CameraController';
import ParticleBackground from './scene/ParticleBackground';
import ProjectHierarchy from './scene/ProjectHierarchy';
import RiskRadar from './scene/RiskRadar';
import TimelinePath from './scene/TimelinePath';
import PostProcessing from './scene/PostProcessing';
import DashboardCards from './components/DashboardCards';
import DetailPanel from './components/DetailPanel';
import SprintCharts from './components/SprintCharts';
import ActivityFeed from './components/ActivityFeed';
import LoadingScreen from './components/LoadingScreen';
import WebGLFallback, { isWebGL2Supported } from './components/WebGLFallback';

export default function App() {
  // Gate the entire 3D experience on WebGL2 support
  if (!isWebGL2Supported()) {
    return <WebGLFallback />;
  }

  return (
    <DashboardStoreProvider>
      <div className="relative w-full h-full">
        {/* R3F Canvas - full-screen 3D scene */}
        <Canvas
          className="absolute inset-0"
          camera={{ position: [0, 5, 20], fov: 60 }}
          gl={{ antialias: true, alpha: false, powerPreference: 'high-performance' }}
          dpr={[1, 1.5]}
          <Suspense fallback={null}>
            <SceneSetup />
            <CameraController />
            <ParticleBackground />
            <ProjectHierarchy />
            <RiskRadar />
            <TimelinePath />
            <PostProcessing />
            <OrbitControls
              enableDamping
              dampingFactor={0.05}
              minDistance={5}
              maxDistance={60}
              maxPolarAngle={Math.PI * 0.85}
            />
          </Suspense>
        </Canvas>

        {/* HTML overlay panels - rendered on top of canvas */}
        <DashboardCards />
        <SprintCharts />
        <ActivityFeed />
        <DetailPanel />
        <Suspense fallback={<LoadingScreen />}><></></Suspense>
      </div>
    </DashboardStoreProvider>
  );
}