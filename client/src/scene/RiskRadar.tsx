/**
 * RiskRadar — Orbital risk & blocker visualization (US-05)
 *
 * Concentric TorusGeometry rings represent severity levels (critical innermost,
 * low outermost). Risk nodes orbit their severity ring as glowing spheres,
 * color-coded by severity. Critical/high nodes are assigned to bloom layer 1
 * for selective glow. Hover shows a drei Html tooltip with title/owner.
 * Click triggers dashboardStore selection and camera focus.
 *
 * Performance: Uses individual meshes (8-12 nodes) — well within draw call budget.
 */
import { useRef, useMemo, useState } from 'react';
import { useFrame } from '@react-three/fiber';
import { Html, Float } from '@react-three/drei';
import * as THREE from 'three';
import { useRisks } from '../api/client';
import { useDashboard } from '../store/dashboardStore';
import type { Risk, RiskSeverity } from '../types';

// Severity ring radii (concentric orbits)
const SEVERITY_CONFIG: Record<RiskSeverity, { radius: number; color: string; bloomLayer: boolean }> = {
  critical: { radius: 2, color: '#ff0000', bloomLayer: true },
  high: { radius: 3.5, color: '#ff6600', bloomLayer: true },
  medium: { radius: 5, color: '#ffaa00', bloomLayer: false },
  low: { radius: 6.5, color: '#888888', bloomLayer: false },
};

// Ring visual properties
const RING_TUBE_RADIUS = 0.02;
const RING_SEGMENTS = 64;
const RING_TUBE_SEGMENTS = 16;
const NODE_RADIUS = 0.25;
const NODE_SEGMENTS = 16;

// Position offset for the entire radar group in the scene
const RADAR_POSITION: [number, number, number] = [10, 0, -5];

/**
 * Central pulsing core — a sphere that pulses with emissive glow,
 * acting as the visual anchor for the radar.
 */
function PulsingCore() {
  const meshRef = useRef<THREE.Mesh>(null);

  useFrame(({ clock }) => {
    if (!meshRef.current) return;
    const t = clock.getElapsedTime();
    const scale = 1 + 0.15 * Math.sin(t * 2);
    meshRef.current.scale.setScalar(scale);
    const mat = meshRef.current.material as THREE.MeshStandardMaterial;
    if (mat) {
      mat.emissiveIntensity = 0.6 + 0.4 * Math.sin(t * 2.5);
    }
  });

  return (
    <mesh ref={meshRef} layers={1}>
      <sphereGeometry args={[0.4, 24, 24]} />
      <meshStandardMaterial
        color="#ff4444"
        emissive="#ff2200"
        emissiveIntensity={0.8}
        transparent
        opacity={0.9}
      />
    </mesh>
  );
}

/**
 * Severity ring — a TorusGeometry rendered as a translucent orbit path.
 */
function SeverityRing({ radius, color }: { radius: number; color: string }) {
  return (
    <mesh rotation={[Math.PI / 2, 0, 0]}>
      <torusGeometry args={[radius, RING_TUBE_RADIUS, RING_TUBE_SEGMENTS, RING_SEGMENTS]} />
      <meshStandardMaterial
        color={color}
        emissive={color}
        emissiveIntensity={0.3}
        transparent
        opacity={0.4}
      />
    </mesh>
  );
}

/**
 * Individual orbiting risk node sphere with hover tooltip and click interaction.
 */
function RiskNode({
  risk,
  orbitRadius,
  color,
  bloomLayer,
  orbitSpeed,
  initialAngle,
  orbitTilt,
}: {
  risk: Risk;
  orbitRadius: number;
  color: string;
  bloomLayer: boolean;
  orbitSpeed: number;
  initialAngle: number;
  orbitTilt: number;
}) {
  const meshRef = useRef<THREE.Mesh>(null);
  const groupRef = useRef<THREE.Group>(null);
  const [hovered, setHovered] = useState(false);
  const { setSelectedItemId, setFocusTarget } = useDashboard();

  // Animate orbital motion each frame
  useFrame(({ clock }) => {
    if (!groupRef.current) return;
    const t = clock.getElapsedTime();
    const angle = initialAngle + t * orbitSpeed;
    const x = Math.cos(angle) * orbitRadius;
    const z = Math.sin(angle) * orbitRadius;
    // Apply slight Y oscillation based on tilt
    const y = Math.sin(angle + orbitTilt) * 0.3;
    groupRef.current.position.set(x, y, z);

    // Hover glow pulse
    if (meshRef.current) {
      const mat = meshRef.current.material as THREE.MeshStandardMaterial;
      if (mat) {
        mat.emissiveIntensity = hovered
          ? 1.2 + 0.3 * Math.sin(t * 5)
          : 0.5 + 0.2 * Math.sin(t * 2 + initialAngle);
      }
    }
  });

  const handleClick = (e: THREE.Event & { stopPropagation?: () => void }) => {
    if (e.stopPropagation) e.stopPropagation();
    // Get world position for camera focus
    if (groupRef.current) {
      const worldPos = new THREE.Vector3();
      groupRef.current.getWorldPosition(worldPos);
      setFocusTarget([worldPos.x, worldPos.y, worldPos.z]);
    }
    setSelectedItemId(risk.id);
  };

  const handlePointerOver = (e: THREE.Event & { stopPropagation?: () => void }) => {
    if (e.stopPropagation) e.stopPropagation();
    setHovered(true);
    document.body.style.cursor = 'pointer';
  };

  const handlePointerOut = () => {
    setHovered(false);
    document.body.style.cursor = 'auto';
  };

  return (
    <group ref={groupRef}>
      <mesh
        ref={meshRef}
        onClick={handleClick}
        onPointerOver={handlePointerOver}
        onPointerOut={handlePointerOut}
        layers={bloomLayer ? 1 : 0}
        <sphereGeometry args={[hovered ? NODE_RADIUS * 1.3 : NODE_RADIUS, NODE_SEGMENTS, NODE_SEGMENTS]} />
        <meshStandardMaterial
          color={color}
          emissive={color}
          emissiveIntensity={0.6}
          transparent
          opacity={0.9}
        />
      </mesh>

      {/* Hover tooltip with title and owner */}
      {hovered && (
        <Html
          distanceFactor={12}
          position={[0, 0.6, 0]}
          center
          style={{ pointerEvents: 'none' }}
          <div className="bg-slate-900/90 backdrop-blur-md border border-slate-600/50 rounded-lg px-3 py-2 text-xs whitespace-nowrap shadow-xl">
            <div className="text-white font-semibold">{risk.title}</div>
            <div className="text-slate-400 mt-0.5">
              Owner: {risk.owner} • {risk.severity.toUpperCase()}
            </div>
          </div>
        </Html>
      )}
    </group>
  );
}

/**
 * RiskRadar — Main exported component
 *
 * Fetches risks from GET /api/risks, groups them by severity, and renders
 * concentric orbital rings with animated risk node spheres.
 */
export default function RiskRadar() {
  const { data } = useRisks();

  // Group risks by severity for orbital placement
  const groupedRisks = useMemo(() => {
    const risks = data?.risks ?? [];
    const grouped: Record<RiskSeverity, Risk[]> = {
      critical: [],
      high: [],
      medium: [],
      low: [],
    };
    risks.forEach((risk) => {
      if (grouped[risk.severity]) {
        grouped[risk.severity].push(risk);
      }
    });
    return grouped;
  }, [data]);

  // Calculate orbital parameters for each risk node (deterministic per position)
  const riskNodes = useMemo(() => {
    const nodes: Array<{
      risk: Risk;
      orbitRadius: number;
      color: string;
      bloomLayer: boolean;
      orbitSpeed: number;
      initialAngle: number;
      orbitTilt: number;
    }> = [];

    (Object.keys(groupedRisks) as RiskSeverity[]).forEach((severity) => {
      const config = SEVERITY_CONFIG[severity];
      const risksInGroup = groupedRisks[severity];
      risksInGroup.forEach((risk, idx) => {
        // Distribute nodes evenly around the orbit, with slight speed variation
        const angleSpacing = (Math.PI * 2) / Math.max(risksInGroup.length, 1);
        const baseSpeed = severity === 'critical' ? 0.8 : severity === 'high' ? 0.5 : severity === 'medium' ? 0.35 : 0.2;
        nodes.push({
          risk,
          orbitRadius: config.radius,
          color: config.color,
          bloomLayer: config.bloomLayer,
          orbitSpeed: baseSpeed + idx * 0.05,
          initialAngle: idx * angleSpacing,
          orbitTilt: idx * 0.7,
        });
      });
    });

    return nodes;
  }, [groupedRisks]);

  // Don't render anything until data arrives
  if (!data || !data.risks || data.risks.length === 0) {
    return null;
  }

  return (
    <group position={RADAR_POSITION}>
      {/* Central pulsing core */}
      <PulsingCore />

      {/* Concentric severity rings */}
      {(Object.keys(SEVERITY_CONFIG) as RiskSeverity[]).map((severity) => (
        <SeverityRing
          key={severity}
          radius={SEVERITY_CONFIG[severity].radius}
          color={SEVERITY_CONFIG[severity].color}
        />
      ))}

      {/* Orbiting risk nodes */}
      {riskNodes.map((node) => (
        <RiskNode
          key={node.risk.id}
          risk={node.risk}
          orbitRadius={node.orbitRadius}
          color={node.color}
          bloomLayer={node.bloomLayer}
          orbitSpeed={node.orbitSpeed}
          initialAngle={node.initialAngle}
          orbitTilt={node.orbitTilt}
        />
      ))}
    </group>
  );
}