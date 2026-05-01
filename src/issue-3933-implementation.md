## Implementation

Now I have a comprehensive understanding of the codebase. The task requires replacing the `Timeline3D.tsx` stub (note: the issue says `TimelinePath.tsx` but in the scaffold it's `Timeline3D.tsx`). I'll implement it as `Timeline3D.tsx` to match the existing scaffold and imports. Let me produce the implementation.

FILE: client/src/scene/Timeline3D.tsx
```typescript
import { useRef, useMemo } from 'react';
import { useFrame } from '@react-three/fiber';
import { Text, Float } from '@react-three/drei';
import * as THREE from 'three';
import { useRoadmap } from '../api/client';
import type { Milestone, SprintBoundary } from '../types';

// Color constants for milestone phases
const COLORS = {
  completed: '#00d4ff',   // Bright cyan
  active: '#ffffff',       // White (will pulse)
  upcoming: '#64748b',    // Dim slate
  tube: '#7b61ff',        // Purple glow for the timeline tube
  sprintRing: '#334155',  // Dim ring for sprint boundaries
};

// Timeline layout constants
const TIMELINE_LENGTH = 24;        // Total X-axis span
const TIMELINE_Y = -4;             // Vertical position
const TIMELINE_Z = -6;             // Depth position
const TUBE_RADIUS = 0.06;
const TUBE_SEGMENTS = 64;
const TUBE_RADIAL_SEGMENTS = 12;
const MILESTONE_SIZE = 0.35;
const RING_INNER = 0.12;
const RING_OUTER = 0.22;

/**
 * Maps a date string to an X position along the timeline.
 * Positions are proportional to date range, spanning from -TIMELINE_LENGTH/2 to +TIMELINE_LENGTH/2.
 */
function dateToPosition(dateStr: string, minTime: number, maxTime: number): number {
  const time = new Date(dateStr).getTime();
  if (maxTime === minTime) return 0;
  const normalized = (time - minTime) / (maxTime - minTime);
  return -TIMELINE_LENGTH / 2 + normalized * TIMELINE_LENGTH;
}

/** Pulsing active milestone marker */
function ActiveMilestoneMarker({ position }: { position: [number, number, number] }) {
  const meshRef = useRef<THREE.Mesh>(null);

  useFrame(({ clock }) => {
    if (meshRef.current) {
      // Pulsing scale between 0.8 and 1.2
      const pulse = 1 + 0.2 * Math.sin(clock.getElapsedTime() * 3);
      meshRef.current.scale.setScalar(pulse);
      // Pulsing emissive intensity
      const mat = meshRef.current.material as THREE.MeshStandardMaterial;
      if (mat) {
        mat.emissiveIntensity = 0.5 + 0.5 * Math.sin(clock.getElapsedTime() * 3);
      }
    }
  });

  return (
    <mesh ref={meshRef} position={position}>
      <octahedronGeometry args={[MILESTONE_SIZE, 0]} />
      <meshStandardMaterial
        color={COLORS.active}
        emissive={COLORS.active}
        emissiveIntensity={0.8}
        transparent
        opacity={0.95}
      />
    </mesh>
  );
}

/** Static milestone marker (completed or upcoming) */
function MilestoneMarker({
  position,
  color,
  emissiveIntensity,
}: {
  position: [number, number, number];
  color: string;
  emissiveIntensity: number;
}) {
  return (
    <Float speed={1.5} rotationIntensity={0.3} floatIntensity={0.2}>
      <mesh position={position}>
        <octahedronGeometry args={[MILESTONE_SIZE, 0]} />
        <meshStandardMaterial
          color={color}
          emissive={color}
          emissiveIntensity={emissiveIntensity}
          transparent
          opacity={color === COLORS.upcoming ? 0.6 : 0.9}
        />
      </mesh>
    </Float>
  );
}

/** Sprint boundary ring marker */
function SprintRingMarker({ position }: { position: [number, number, number] }) {
  return (
    <mesh position={position} rotation={[0, 0, Math.PI / 2]}>
      <ringGeometry args={[RING_INNER, RING_OUTER, 16]} />
      <meshStandardMaterial
        color={COLORS.sprintRing}
        emissive={COLORS.sprintRing}
        emissiveIntensity={0.2}
        transparent
        opacity={0.5}
        side={THREE.DoubleSide}
      />
    </mesh>
  );
}

/** Milestone text label */
function MilestoneLabel({
  position,
  text,
  phase,
}: {
  position: [number, number, number];
  text: string;
  phase: string;
}) {
  const color = phase === 'completed' ? COLORS.completed : phase === 'active' ? COLORS.active : COLORS.upcoming;
  return (
    <Text
      position={[position[0], position[1] + 0.7, position[2]]}
      fontSize={0.18}
      color={color}
      anchorX="center"
      anchorY="bottom"
      maxWidth={3}
      textAlign="center"
      font={undefined}
      {text}
    </Text>
  );
}

/**
 * Timeline3D - 3D horizontal timeline roadmap visualization (US-06)
 *
 * Renders a glowing tube along the X-axis using CatmullRomCurve3 + TubeGeometry,
 * with milestone markers (OctahedronGeometry) color-coded by phase, sprint boundary
 * ring markers, and drei Text labels at each milestone.
 */
export default function Timeline3D() {
  const { data: roadmap } = useRoadmap();

  // Compute date range and positions
  const { curve, milestonePositions, sprintPositions, minTime, maxTime } = useMemo(() => {
    if (!roadmap || !roadmap.milestones || roadmap.milestones.length === 0) {
      // Default fallback curve if no data
      const points = [
        new THREE.Vector3(-TIMELINE_LENGTH / 2, TIMELINE_Y, TIMELINE_Z),
        new THREE.Vector3(0, TIMELINE_Y, TIMELINE_Z),
        new THREE.Vector3(TIMELINE_LENGTH / 2, TIMELINE_Y, TIMELINE_Z),
      ];
      return {
        curve: new THREE.CatmullRomCurve3(points, false, 'catmullrom', 0.5),
        milestonePositions: [] as { milestone: Milestone; x: number }[],
        sprintPositions: [] as { sprint: SprintBoundary; x: number }[],
        minTime: 0,
        maxTime: 1,
      };
    }

    // Calculate the full time range from milestones and sprint boundaries
    const allDates: number[] = [];
    roadmap.milestones.forEach((m) => allDates.push(new Date(m.date).getTime()));
    if (roadmap.sprints) {
      roadmap.sprints.forEach((s) => {
        allDates.push(new Date(s.startDate).getTime());
        allDates.push(new Date(s.endDate).getTime());
      });
    }

    const min = Math.min(...allDates);
    const max = Math.max(...allDates);

    // Build curve points with slight Y undulation for visual interest
    const numPoints = 12;
    const curvePoints: THREE.Vector3[] = [];
    for (let i = 0; i <= numPoints; i++) {
      const t = i / numPoints;
      const x = -TIMELINE_LENGTH / 2 + t * TIMELINE_LENGTH;
      const y = TIMELINE_Y + Math.sin(t * Math.PI * 2) * 0.15;
      curvePoints.push(new THREE.Vector3(x, y, TIMELINE_Z));
    }

    const catmullCurve = new THREE.CatmullRomCurve3(curvePoints, false, 'catmullrom', 0.5);

    // Calculate milestone positions
    const mPositions = roadmap.milestones.map((milestone) => ({
      milestone,
      x: dateToPosition(milestone.date, min, max),
    }));

    // Calculate sprint boundary positions
    const sPositions = (roadmap.sprints || []).map((sprint) => ({
      sprint,
      x: dateToPosition(sprint.startDate, min, max),
    }));

    return {
      curve: catmullCurve,
      milestonePositions: mPositions,
      sprintPositions: sPositions,
      minTime: min,
      maxTime: max,
    };
  }, [roadmap]);

  // Generate tube geometry from curve
  const tubeGeometry = useMemo(() => {
    return new THREE.TubeGeometry(curve, TUBE_SEGMENTS, TUBE_RADIUS, TUBE_RADIAL_SEGMENTS, false);
  }, [curve]);

  // If no data, render minimal placeholder tube
  if (!roadmap || !roadmap.milestones || roadmap.milestones.length === 0) {
    return (
      <group>
        <mesh geometry={tubeGeometry}>
          <meshStandardMaterial
            color={COLORS.tube}
            emissive={COLORS.tube}
            emissiveIntensity={0.3}
            transparent
            opacity={0.5}
          />
        </mesh>
      </group>
    );
  }

  return (
    <group>
      {/* Main timeline tube */}
      <mesh geometry={tubeGeometry}>
        <meshStandardMaterial
          color={COLORS.tube}
          emissive={COLORS.tube}
          emissiveIntensity={0.4}
          transparent
          opacity={0.7}
        />
      </mesh>

      {/* Milestone markers */}
      {milestonePositions.map(({ milestone, x }) => {
        // Get Y position from curve at this X
        const t = (x + TIMELINE_LENGTH / 2) / TIMELINE_LENGTH;
        const pointOnCurve = curve.getPointAt(Math.max(0, Math.min(1, t)));
        const pos: [number, number, number] = [pointOnCurve.x, pointOnCurve.y, pointOnCurve.z];

        return (
          <group key={milestone.id}>
            {/* Marker */}
            {milestone.status === 'active' ? (
              <ActiveMilestoneMarker position={pos} />
            ) : (
              <MilestoneMarker
                position={pos}
                color={milestone.status === 'completed' ? COLORS.completed : COLORS.upcoming}
                emissiveIntensity={milestone.status === 'completed' ? 0.6 : 0.15}
              />
            )}
            {/* Label */}
            <MilestoneLabel position={pos} text={milestone.name} phase={milestone.status} />
          </group>
        );
      })}

      {/* Sprint boundary ring markers */}
      {sprintPositions.map(({ sprint, x }, idx) => {
        const t = (x + TIMELINE_LENGTH / 2) / TIMELINE_LENGTH;
        const pointOnCurve = curve.getPointAt(Math.max(0, Math.min(1, t)));
        const pos: [number, number, number] = [pointOnCurve.x, pointOnCurve.y - 0.4, pointOnCurve.z];

        return (
          <group key={`sprint-${idx}`}>
            <SprintRingMarker position={pos} />
            <Text
              position={[pos[0], pos[1] - 0.35, pos[2]]}
              fontSize={0.12}
              color={COLORS.sprintRing}
              anchorX="center"
              anchorY="top"
              {sprint.name}
            </Text>
          </group>
        );
      })}

      {/* Glowing endpoint caps */}
      <mesh position={[curve.getPointAt(0).x, curve.getPointAt(0).y, curve.getPointAt(0).z]}>
        <sphereGeometry args={[TUBE_RADIUS * 2, 8, 8]} />
        <meshStandardMaterial color={COLORS.completed} emissive={COLORS.completed} emissiveIntensity={0.5} />
      </mesh>
      <mesh position={[curve.getPointAt(1).x, curve.getPointAt(1).y, curve.getPointAt(1).z]}>
        <sphereGeometry args={[TUBE_RADIUS * 2, 8, 8]} />
        <meshStandardMaterial color={COLORS.upcoming} emissive={COLORS.upcoming} emissiveIntensity={0.3} />
      </mesh>
    </group>
  );
}
```