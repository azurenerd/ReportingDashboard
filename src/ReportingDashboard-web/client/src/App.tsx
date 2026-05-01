import { useEffect } from 'react';
import { Canvas } from '@react-three/fiber';
import { useDashboardStore } from './store';
import { DashboardLayout } from './scene/DashboardLayout';
import { CameraController } from './scene/CameraController';
import { ParticleField } from './scene/ParticleField';
import { PostEffects } from './scene/PostEffects';
import { LoadingState } from './components/LoadingState';
import { ProjectOverview } from './components/ProjectOverview';
import { SprintMetrics } from './components/SprintMetrics';
import { TeamActivity } from './components/TeamActivity';
import { DetailPanel } from './components/DetailPanel';

export function App() {
  const isLoading = useDashboardStore((s) => s.isLoading);
  const loadError = useDashboardStore((s) => s.loadError);
  const fetchAllData = useDashboardStore((s) => s.fetchAllData);

  useEffect(() => {
    fetchAllData();
  }, [fetchAllData]);

  return (
    <div style={{ width: '100vw', height: '100vh', background: '#0a0a0f' }}>
      {(isLoading || loadError) && <LoadingState />}
      <Canvas
        dpr={[1, 2]}
        gl={{ antialias: true, alpha: false }}
        camera={{ position: [0, 20, 40], fov: 50 }}
        style={{ position: 'absolute', top: 0, left: 0 }}
      >
        <color attach="background" args={['#0a0a0f']} />
        <ambientLight intensity={0.4} />
        <directionalLight position={[10, 10, 5]} intensity={0.6} />
        <CameraController />
        <ParticleField />
        <DashboardLayout />
        <PostEffects />
      </Canvas>
      {!isLoading && !loadError && (
        <>
          <ProjectOverview />
          <SprintMetrics />
          <TeamActivity />
        </>
      )}
      <DetailPanel />
    </div>
  );
}