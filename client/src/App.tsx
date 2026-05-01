import { Canvas } from '@react-three/fiber';
import { Suspense } from 'react';
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

export default function App() {
  return (
    <DashboardStoreProvider>
      <div className="relative w-full h-full">
        <Canvas
          className="absolute inset-0"
          camera={{ position: [0, 5, 20], fov: 60 }}
          gl={{ antialias: true, alpha: false }}
          onCreated={({ gl }) => { gl.setClearColor('#030712'); }}
        >
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
        <DashboardCards />
        <SprintCharts />
        <ActivityFeed />
        <DetailPanel />
        <Suspense fallback={<LoadingScreen />}><></></Suspense>
      </div>
    </DashboardStoreProvider>
  );
}