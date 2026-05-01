import { LAYOUT_CONFIG } from '../config';
import { HierarchyScene } from './HierarchyScene';
import { RiskRadar } from './RiskRadar';
import { Timeline3D } from './Timeline3D';

export function DashboardLayout() {
  return (
    <>
      <group position={LAYOUT_CONFIG.hierarchy}>
        <HierarchyScene />
      </group>
      <group position={LAYOUT_CONFIG.riskRadar}>
        <RiskRadar />
      </group>
      <group position={LAYOUT_CONFIG.timeline}>
        <Timeline3D />
      </group>
    </>
  );
}