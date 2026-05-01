/**
 * HierarchyGraph.tsx - 3D Project Hierarchy Interactive Node Graph (US-03 / T9)
 *
 * Renders project items (epics, features, stories/tasks) as a force-directed
 * 3D node graph using d3-force-3d for layout and React Three Fiber for rendering.
 *
 * Performance strategy:
 * - InstancedMesh for story-level nodes (40+): single draw call
 * - Individual <mesh> for epic/feature nodes (16): native R3F pointer events
 * - Single <lineSegments> for all connections: single draw call
 * - Force layout computed once synchronously (~300 ticks), not per-frame
 */
import { useRef, useMemo, useCallback, useState, useEffect } from 'react';
import { useFrame } from '@react-three/fiber';
import {
  InstancedMesh,
  Object3D,
  Color,
  Vector3,
  BufferGeometry,
  Float32BufferAttribute,
  MeshStandardMaterial,
  Mesh,
  LineSegments as ThreeLineSegments,
  LineBasicMaterial,
  InstancedBufferAttribute,
} from 'three';
import {
  forceSimulation,
  forceLink,
  forceManyBody,
  forceCenter,
} from 'd3-force-3d';
import { useProjectItems } from '../api/client';
import { useDashboard } from '../context/DashboardContext';
import type { ProjectItem, ItemStatus } from '../types';

// ── Status color mapping per acceptance criteria ──
const STATUS_COLORS: Record<ItemStatus, string> = {
  done: '#00ff88',
  'in-progress': '#00aaff',
  blocked: '#ff4444',
  'not-started': '#666666',
  'at-risk': '#ff8800',
};

// ── Node radii by hierarchy level per acceptance criteria ──
const RADIUS: Record<string, number> = {
  epic: 0.8,
  feature: 0.5,
  story: 0.3,
  task: 0.3,
  bug: 0.3,
};

// ── Animation constants ──
const FLOAT_AMP = 0.15;
const FLOAT_SPEED = 0.8;
const LERP_DURATION = 1.0;
const PULSE_SPEED = 2.0;
const EMISSIVE_BASE = 0.4;
const EMISSIVE_HOVER = 1.2;

// Reusable origin vector for lerp calculations (avoids per-frame allocation)
const ORIGIN = new Vector3(0, 0, 0);

// ── Layout types ──
interface SimNode {
  id: string;
  index?: number;
  x: number;
  y: number;
  z: number;
  item: ProjectItem;
}

interface SimLink {
  source: string | SimNode;
  target: string | SimNode;
}

interface NodePos {
  final: Vector3;
  current: Vector3;
}

// ── Helpers ──

function statusColor(status: ItemStatus): Color {
  return new Color(STATUS_COLORS[status] ?? '#666666');
}

function nodeRadius(type: string): number {
  return RADIUS[type] ?? 0.3;
}

function isStoryLevel(type: string): boolean {
  return type === 'story' || type === 'task' || type === 'bug';
}

function linkNodeId(endpoint: string | SimNode): string {
  return typeof endpoint === 'object' ? endpoint.id : endpoint;
}

/**
 * Compute a 3D force-directed layout synchronously to convergence.
 * Runs ~300 simulation ticks so nodes reach stable positions before rendering.
 */
function computeLayout(
  items: ProjectItem[],
  rawLinks: { source: string; target: string }[]
): { nodes: SimNode[]; links: SimLink[] } {
  const nodes: SimNode[] = items.map((item) => ({
    id: item.id,
    x: 0,
    y: 0,
    z: 0,
    item,
  }));

  const links: SimLink[] = rawLinks.map((l) => ({
    source: l.source,
    target: l.target,
  }));

  // 3D force simulation: dimension parameter = 3
  const sim = forceSimulation(nodes, 3)
    .force(
      'link',
      forceLink(links)
        .id((d: SimNode) => d.id)
        .distance((link: { source: SimNode | string }) => {
          const src = typeof link.source === 'object' ? link.source : null;
          return src?.item?.type === 'epic' ? 6 : 3.5;
        })
        .strength(0.7)
    )
    .force('charge', forceManyBody().strength(-35).distanceMax(25))
    .force('center', forceCenter(0, 0, 0).strength(0.05))
    .stop();

  // Run 300 ticks to convergence
  for (let i = 0; i < 300; i++) {
    sim.tick();
  }

  return { nodes, links };
}

// ── Epic/Feature Node (individual mesh with hover glow + click) ──

function InteractiveNode({
  item,
  posRef,
  floatSeed,
}: {
  item: ProjectItem;
  posRef: NodePos;
  floatSeed: number;
}) {
  const meshRef = useRef<Mesh>(null);
  const [hovered, setHovered] = useState(false);
  const { dispatch } = useDashboard();
  const color = useMemo(() => statusColor(item.status), [item.status]);
  const r = nodeRadius(item.type);

  const handleClick = useCallback(
    (e: { stopPropagation: () => void }) => {
      e.stopPropagation();
      // Opens detail panel via SELECT_ITEM action
      dispatch({ type: 'SELECT_ITEM', id: item.id });
    },
    [dispatch, item.id]
  );

  // Per-frame: update position with sine-wave float + adjust hover emissive
  useFrame(({ clock }) => {
    if (!meshRef.current) return;
    const t = clock.getElapsedTime();
    meshRef.current.position.set(
      posRef.current.x,
      posRef.current.y + Math.sin(t * FLOAT_SPEED + floatSeed) * FLOAT_AMP,
      posRef.current.z
    );
    const mat = meshRef.current.material as MeshStandardMaterial;
    mat.emissiveIntensity = hovered ? EMISSIVE_HOVER : EMISSIVE_BASE;
  });

  return (
    <mesh
      ref={meshRef}
      onPointerOver={(e) => {
        e.stopPropagation();
        setHovered(true);
        document.body.style.cursor = 'pointer';
      }}
      onPointerOut={(e) => {
        e.stopPropagation();
        setHovered(false);
        document.body.style.cursor = 'auto';
      }}
      onClick={handleClick}
      <sphereGeometry args={[r, 24, 24]} />
      <meshStandardMaterial
        color={color}
        emissive={color}
        emissiveIntensity={EMISSIVE_BASE}
        transparent={true}
        opacity={0.9}
        roughness={0.3}
        metalness={0.1}
      />
    </mesh>
  );
}

// ── Story/Task/Bug Instances (InstancedMesh for 40+ nodes) ──

function StoryInstances({
  nodes,
  posMap,
}: {
  nodes: SimNode[];
  posMap: Map<string, NodePos>;
}) {
  const meshRef = useRef<InstancedMesh>(null);
  const { dispatch } = useDashboard();
  const dummy = useMemo(() => new Object3D(), []);

  // Per-instance color buffer based on status
  const colors = useMemo(() => {
    const arr = new Float32Array(nodes.length * 3);
    nodes.forEach((n, i) => {
      statusColor(n.item.status).toArray(arr, i * 3);
    });
    return arr;
  }, [nodes]);

  // Apply per-instance colors when data changes
  useEffect(() => {
    const mesh = meshRef.current;
    if (!mesh || nodes.length === 0) return;
    mesh.instanceColor = new InstancedBufferAttribute(colors, 3);
    mesh.instanceColor.needsUpdate = true;
  }, [colors, nodes.length]);

  // Per-frame: update instance transforms with floating motion
  useFrame(({ clock }) => {
    const mesh = meshRef.current;
    if (!mesh) return;
    const t = clock.getElapsedTime();
    nodes.forEach((node, i) => {
      const pos = posMap.get(node.id);
      if (!pos) return;
      dummy.position.set(
        pos.current.x,
        pos.current.y + Math.sin(t * FLOAT_SPEED + i * 0.5) * FLOAT_AMP,
        pos.current.z
      );
      dummy.scale.setScalar(1);
      dummy.updateMatrix();
      mesh.setMatrixAt(i, dummy.matrix);
    });
    mesh.instanceMatrix.needsUpdate = true;
  });

  const handleClick = useCallback(
    (e: { stopPropagation: () => void; instanceId?: number }) => {
      e.stopPropagation();
      if (e.instanceId !== undefined && e.instanceId < nodes.length) {
        dispatch({ type: 'SELECT_ITEM', id: nodes[e.instanceId].id });
      }
    },
    [dispatch, nodes]
  );

  if (nodes.length === 0) return null;

  return (
    <instancedMesh
      ref={meshRef}
      args={[undefined, undefined, nodes.length]}
      onClick={handleClick}
      onPointerOver={() => {
        document.body.style.cursor = 'pointer';
      }}
      onPointerOut={() => {
        document.body.style.cursor = 'auto';
      }}
      <sphereGeometry args={[0.3, 12, 12]} />
      <meshStandardMaterial
        color="#ffffff"
        emissive="#888888"
        emissiveIntensity={0.3}
        transparent={true}
        opacity={0.85}
        roughness={0.4}
        metalness={0.1}
      />
    </instancedMesh>
  );
}

// ── Connection Lines (single LineSegments draw call for all edges) ──

function Connections({
  links,
  posMap,
}: {
  links: SimLink[];
  posMap: Map<string, NodePos>;
}) {
  const ref = useRef<ThreeLineSegments>(null);
  const matRef = useRef<LineBasicMaterial>(null);

  // Pre-allocate geometry buffer for all line segments (2 vertices per link)
  const geometry = useMemo(() => {
    const geo = new BufferGeometry();
    const positions = new Float32Array(links.length * 6);
    geo.setAttribute('position', new Float32BufferAttribute(positions, 3));
    return geo;
  }, [links.length]);

  // Per-frame: update line endpoints + pulse opacity
  useFrame(({ clock }) => {
    const attr = geometry.attributes.position as Float32BufferAttribute;
    const arr = attr.array as Float32Array;

    links.forEach((link, i) => {
      const src = posMap.get(linkNodeId(link.source));
      const tgt = posMap.get(linkNodeId(link.target));
      if (!src || !tgt) return;
      const off = i * 6;
      arr[off] = src.current.x;
      arr[off + 1] = src.current.y;
      arr[off + 2] = src.current.z;
      arr[off + 3] = tgt.current.x;
      arr[off + 4] = tgt.current.y;
      arr[off + 5] = tgt.current.z;
    });
    attr.needsUpdate = true;

    // Pulsing opacity effect on connection lines
    if (matRef.current) {
      const t = clock.getElapsedTime();
      matRef.current.opacity = 0.2 + 0.15 * Math.sin(t * PULSE_SPEED);
    }
  });

  return (
    <lineSegments ref={ref} geometry={geometry}>
      <lineBasicMaterial
        ref={matRef}
        color="#4488aa"
        transparent={true}
        opacity={0.3}
        depthWrite={false}
      />
    </lineSegments>
  );
}

// ── Main Component ──

/**
 * 3D Project Hierarchy - force-directed node graph.
 *
 * Fetches project items from GET /api/project-items, computes a d3-force-3d
 * layout synchronously to convergence (~300 ticks), then renders:
 * - Epics/Features as individual meshes (hover glow + click-to-select)
 * - Stories/Tasks/Bugs as InstancedMesh (40+ nodes, single draw call)
 * - Parent-child connections as LineSegments with pulsing opacity
 *
 * Nodes lerp from origin to final positions over ~1s with cubic ease-out,
 * then float with continuous sine-wave motion.
 */
export default function HierarchyGraph() {
  const { data, error } = useProjectItems();
  const groupRef = useRef<any>(null);
  const animStart = useRef(-1);

  // Compute force layout once when data arrives
  const layout = useMemo(() => {
    if (!data?.nodes?.length) return null;
    return computeLayout(data.nodes, data.links);
  }, [data]);

  // Split nodes: interactive (epic/feature) vs instanced (story/task/bug)
  const { epicFeatureNodes, storyNodes } = useMemo(() => {
    if (!layout) {
      return { epicFeatureNodes: [] as SimNode[], storyNodes: [] as SimNode[] };
    }
    const ef: SimNode[] = [];
    const st: SimNode[] = [];
    layout.nodes.forEach((n) => {
      if (isStoryLevel(n.item.type)) {
        st.push(n);
      } else {
        ef.push(n);
      }
    });
    return { epicFeatureNodes: ef, storyNodes: st };
  }, [layout]);

  // Build position map for lerp animation (shared by reference with children)
  const posMap = useMemo(() => {
    const map = new Map<string, NodePos>();
    if (!layout) return map;
    layout.nodes.forEach((n) => {
      map.set(n.id, {
        final: new Vector3(n.x, n.y, n.z),
        current: new Vector3(0, 0, 0),
      });
    });
    return map;
  }, [layout]);

  // Reset animation timer when layout changes
  useEffect(() => {
    animStart.current = -1;
  }, [layout]);

  // Animate nodes from origin to final positions over ~1s with cubic ease-out
  useFrame(({ clock }) => {
    if (!layout || posMap.size === 0) return;
    if (animStart.current < 0) {
      animStart.current = clock.getElapsedTime();
    }
    const elapsed = clock.getElapsedTime() - animStart.current;
    const raw = Math.min(elapsed / LERP_DURATION, 1);
    // Cubic ease-out for smooth deceleration
    const t = 1 - Math.pow(1 - raw, 3);

    posMap.forEach((pos) => {
      pos.current.lerpVectors(ORIGIN, pos.final, t);
    });
  });

  // Graceful fallback while loading or on API error
  if (error || !data || !layout) return null;

  return (
    <group ref={groupRef}>
      {/* Connection lines between parent-child nodes */}
      <Connections links={layout.links} posMap={posMap} />

      {/* Epic & Feature nodes - individual meshes for hover/click events */}
      {epicFeatureNodes.map((node, i) => {
        const pos = posMap.get(node.id);
        if (!pos) return null;
        return (
          <InteractiveNode
            key={node.id}
            item={node.item}
            posRef={pos}
            floatSeed={i * 1.1}
          />
        );
      })}

      {/* Story/Task/Bug nodes - InstancedMesh for performance */}
      <StoryInstances nodes={storyNodes} posMap={posMap} />
    </group>
  );
}