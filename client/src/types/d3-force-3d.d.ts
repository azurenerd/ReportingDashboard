/**
 * Minimal type declarations for d3-force-3d.
 * This package extends d3-force with 3D support (x, y, z coordinates)
 * but does not ship its own TypeScript definitions.
 */
declare module 'd3-force-3d' {
  export interface SimulationNode {
    index?: number;
    x: number;
    y: number;
    z: number;
    vx?: number;
    vy?: number;
    vz?: number;
    fx?: number | null;
    fy?: number | null;
    fz?: number | null;
    [key: string]: any;
  }

  export interface SimulationLink {
    source: any;
    target: any;
    index?: number;
    [key: string]: any;
  }

  export interface Simulation {
    tick(): void;
    stop(): Simulation;
    force(name: string, force?: any): any;
    nodes(): SimulationNode[];
    alpha(value?: number): any;
    alphaDecay(value?: number): any;
    alphaMin(value?: number): any;
  }

  export function forceSimulation(nodes?: any[], nDim?: number): Simulation;

  export interface ForceLink {
    id(fn: (d: any) => string): ForceLink;
    distance(value: number | ((link: any) => number)): ForceLink;
    strength(value: number | ((link: any) => number)): ForceLink;
    links(): any[];
  }
  export function forceLink(links?: any[]): ForceLink;

  export interface ForceManyBody {
    strength(value: number | ((d: any) => number)): ForceManyBody;
    distanceMin(value: number): ForceManyBody;
    distanceMax(value: number): ForceManyBody;
    theta(value: number): ForceManyBody;
  }
  export function forceManyBody(): ForceManyBody;

  export interface ForceCenter {
    x(value: number): ForceCenter;
    y(value: number): ForceCenter;
    z(value: number): ForceCenter;
    strength(value: number): ForceCenter;
  }
  export function forceCenter(x?: number, y?: number, z?: number): ForceCenter;

  export interface ForceCollide {
    radius(value: number | ((d: any) => number)): ForceCollide;
    strength(value: number): ForceCollide;
    iterations(value: number): ForceCollide;
  }
  export function forceCollide(radius?: number | ((d: any) => number)): ForceCollide;
}