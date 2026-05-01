import { Canvas } from '@react-three/fiber';
import { Suspense } from 'react';
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
import { useProjectData } from './hooks/useProjectData';

export default function App() {
  const { loading } = useProjectData();

  if (loading) return <LoadingScreen />;

  return (
    <div className="relative w-full h-screen bg-gray-950">
      {/* 3D Canvas — fills the entire viewport */}
      <Canvas
        className="absolute inset-0"
        camera={{ position: [0, 5, 20], fov: 60 }}
        gl={{ antialias: true, alpha: false }}
        onCreated={({ gl }) => {
          gl.setClearColor('#0a0a1a');
        }}
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

      {/* HTML overlay components — rendered on top of the canvas */}
      <div className="absolute inset-0 pointer-events-none z-10">
        <div className="pointer-events-auto">
          <DashboardCards />
          <SprintCharts />
          <ActivityFeed />
          <DetailPanel />
        </div>
      </div>
    </div>
  );
}