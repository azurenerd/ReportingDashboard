import { useRef, useMemo } from 'react';
import { useFrame } from '@react-three/fiber';
import { Text, Float } from '@react-three/drei';
import * as THREE from 'three';
import { useRoadmap } from '../api/client';

/**
 * Local interfaces matching the GET /api/roadmap API contract.
 * Defined locally to decouple from potentially mismatched global types.
 */
interface TimelineMilestone {
  id: string;
  title: string;
  date: string;
  phase: 'completed' | 'active' | 'upcoming';
  type: string;
  description: string;
}

interface TimelineRoadmapData {
  milestones: TimelineMilestone[];
  sprintBoundaries: { date: string; label: string }[];
}

// Color constants for milestone phases
const COLORS = {
  completed: '#00d4ff', // Bright cyan
  active: '#ffffff',    // White (will pulse)
  upcoming: '#64748b',  // Dim slate
  tube: '#7b61ff',      // Purple glow for the timeline tube
  sprintRing: '#334155', // Dim ring for sprint boundaries
};

// Timeline layout constants
const TIMELINE_LENGTH = 24;
const TIMELINE_Y = -4;
const TIMELINE_Z = -6;
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

/** Pulsing active milestone marker with animated scale and emissive */
function ActiveMilestoneMarker({ position }: { position: [number, number, number] }) {
  const meshRef = useRef<THREE.Mesh>(null);

  useFrame(({ clock }) => {
    if (meshRef.current) {
      const pulse = 1 + 0.2 * Math.sin(clock.getElapsedTime() * 3);
      meshRef.current.scale.setScalar(pulse);
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

/** Static milestone marker (completed=cyan bright or upcoming=dim slate) */
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

/** Sprint boundary ring marker rendered as a ring geometry */
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

/** Milestone text label positioned above the marker */
function MilestoneLabel({
  position,
  text,
  phase,
}: {
  position: [number, number, number];
  text: string;
  phase: string;
}) {
  const color =
    phase === 'completed' ? COLORS.completed : phase === 'active' ? COLORS.active : COLORS.upcoming;
  return (
    <Text
      position={[position[0], position[1] + 0.7, position[2]]}
      fontSize={0.18}
      color={color}
      anchorX="center"
      anchorY="bottom"
      maxWidth={3}
      textAlign="center"
      {text}
    </Text>
  );
}

/**
 * TimelinePath - 3D horizontal timeline roadmap visualization (US-06)
 *
 * Renders a glowing tube along the X-axis using CatmullRomCurve3 + TubeGeometry,
 * with milestone markers (OctahedronGeometry) color-coded by phase, sprint boundary
 * ring markers, and drei Text labels at each milestone. Data is fetched from
 * GET /api/roadmap via the useRoadmap SWR hook.
 */
export default function TimelinePath() {
  const { data: rawData } = useRoadmap();

  // Cast to match actual API contract shape
  const roadmap = rawData as unknown as TimelineRoadmapData | undefined;

  // Compute date range, curve, and positions from roadmap data
  const { curve, milestonePositions, sprintPositions } = useMemo(() => {
    if (!roadmap || !roadmap.milestones || roadmap.milestones.length === 0) {
      // Default fallback curve when no data is available
      const points = [
        new THREE.Vector3(-TIMELINE_LENGTH / 2, TIMELINE_Y, TIMELINE_Z),
        new THREE.Vector3(0, TIMELINE_Y, TIMELINE_Z),
        new THREE.Vector3(TIMELINE_LENGTH / 2, TIMELINE_Y, TIMELINE_Z),
      ];
      return {
        curve: new THREE.CatmullRomCurve3(points, false, 'catmullrom', 0.5),
        milestonePositions: [] as { milestone: TimelineMilestone; x: number }[],
        sprintPositions: [] as { date: string; label: string; x: number }[],
      };
    }

    // Calculate the full time range from milestones and sprint boundaries
    const allDates: number[] = [];
    roadmap.milestones.forEach((m) => allDates.push(new Date(m.date).getTime()));
    const boundaries = roadmap.sprintBoundaries || [];
    boundaries.forEach((b) => allDates.push(new Date(b.date).getTime()));

    const min = Math.min(...allDates);
    const max = Math.max(...allDates);

    // Build curve control points with slight Y undulation for visual interest
    const numPoints = 12;
    const curvePoints: THREE.Vector3[] = [];
    for (let i = 0; i <= numPoints; i++) {
      const t = i / numPoints;
      const x = -TIMELINE_LENGTH / 2 + t * TIMELINE_LENGTH;
      const y = TIMELINE_Y + Math.sin(t * Math.PI * 2) * 0.15;
      curvePoints.push(new THREE.Vector3(x, y, TIMELINE_Z));
    }

    const catmullCurve = new THREE.CatmullRomCurve3(curvePoints, false, 'catmullrom', 0.5);

    // Calculate milestone X positions proportional to dates
    const mPositions = roadmap.milestones.map((milestone) => ({
      milestone,
      x: dateToPosition(milestone.date, min, max),
    }));

    // Calculate sprint boundary X positions
    const sPositions = boundaries.map((boundary) => ({
      ...boundary,
      x: dateToPosition(boundary.date, min, max),
    }));

    return {
      curve: catmullCurve,
      milestonePositions: mPositions,
      sprintPositions: sPositions,
    };
  }, [roadmap]);

  // Generate tube geometry from the CatmullRom curve
  const tubeGeometry = useMemo(() => {
    return new THREE.TubeGeometry(curve, TUBE_SEGMENTS, TUBE_RADIUS, TUBE_RADIAL_SEGMENTS, false);
  }, [curve]);

  // Render minimal placeholder tube if no data is loaded yet
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
      {/* Main timeline tube with purple glow */}
      <mesh geometry={tubeGeometry}>
        <meshStandardMaterial
          color={COLORS.tube}
          emissive={COLORS.tube}
          emissiveIntensity={0.4}
          transparent
          opacity={0.7}
        />
      </mesh>

      {/* Milestone markers: OctahedronGeometry at date-proportional positions */}
      {milestonePositions.map(({ milestone, x }) => {
        const t = (x + TIMELINE_LENGTH / 2) / TIMELINE_LENGTH;
        const pointOnCurve = curve.getPointAt(Math.max(0, Math.min(1, t)));
        const pos: [number, number, number] = [pointOnCurve.x, pointOnCurve.y, pointOnCurve.z];

        return (
          <group key={milestone.id}>
            {milestone.phase === 'active' ? (
              <ActiveMilestoneMarker position={pos} />
            ) : (
              <MilestoneMarker
                position={pos}
                color={milestone.phase === 'completed' ? COLORS.completed : COLORS.upcoming}
                emissiveIntensity={milestone.phase === 'completed' ? 0.6 : 0.15}
              />
            )}
            <MilestoneLabel position={pos} text={milestone.title} phase={milestone.phase} />
          </group>
        );
      })}

      {/* Sprint boundary ring markers */}
      {sprintPositions.map((boundary, idx) => {
        const t = (boundary.x + TIMELINE_LENGTH / 2) / TIMELINE_LENGTH;
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
              {boundary.label}
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