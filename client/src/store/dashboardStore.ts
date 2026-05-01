import { create } from 'zustand';

export interface FocusTarget {
  x: number;
  y: number;
  z: number;
}

interface DashboardState {
  selectedEntityId: string | null;
  focusTarget: FocusTarget | null;
  setSelectedEntity: (id: string | null) => void;
  setFocusTarget: (target: FocusTarget | null) => void;
  clearSelection: () => void;
}

export const useDashboardStore = create<DashboardState>((set) => ({
  selectedEntityId: null,
  focusTarget: null,
  setSelectedEntity: (id) => set({ selectedEntityId: id }),
  setFocusTarget: (target) => set({ focusTarget: target }),
  clearSelection: () => set({ selectedEntityId: null, focusTarget: null }),
}));