export type QualityPreset = 'high' | 'low';

export const QUALITY_PRESET: QualityPreset = 'high';

export const QUALITY_CONFIG = {
  high: {
    particleCount: 1000,
    bloomEnabled: true,
    bloomIntensity: 1.2,
    bloomThreshold: 0.4,
    bloomRadius: 0.8,
    sphereSegments: 32,
  },
  low: {
    particleCount: 200,
    bloomEnabled: false,
    bloomIntensity: 0,
    bloomThreshold: 1,
    bloomRadius: 0,
    sphereSegments: 8,
  },
} as const;

export const LAYOUT_CONFIG = {
  hierarchy: [-4, 0, 0] as [number, number, number],
  riskRadar: [5, 0, 0] as [number, number, number],
  timeline: [0, -4, 0] as [number, number, number],
} as const;

export const STATUS_COLORS: Record<string, string> = {
  Done: '#00ff88',
  InProgress: '#00d4ff',
  Blocked: '#ff4444',
  NotStarted: '#666677',
  AtRisk: '#ffaa00',
};

export const SEVERITY_COLORS: Record<string, string> = {
  Critical: '#ff4444',
  High: '#ff8844',
  Medium: '#ffaa00',
  Low: '#00d4ff',
};