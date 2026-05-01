import { createContext, useContext, useState, useCallback, useMemo } from 'react';
import type { ReactNode } from 'react';

export interface FocusTarget {
  x: number;
  y: number;
  z: number;
}

/** Shape of the dashboard store exposed to consumers via useDashboardStore(). */
export interface DashboardState {
  selectedEntityId: string | null;
  focusTarget: FocusTarget | null;
  setSelectedEntity: (id: string | null) => void;
  setFocusTarget: (target: FocusTarget | null) => void;
  clearSelection: () => void;
}

const DashboardContext = createContext<DashboardState | null>(null);

/**
 * Provider that wraps the application tree and supplies shared dashboard state.
 * Place this near the root (e.g., in App.tsx) so both 3D scene components
 * and HTML overlay components can access selection/focus state.
 */
export function DashboardProvider({ children }: { children: ReactNode }) {
  const [selectedEntityId, setSelectedEntityId] = useState<string | null>(null);
  const [focusTarget, setFocusTargetState] = useState<FocusTarget | null>(null);

  const setSelectedEntity = useCallback((id: string | null) => {
    setSelectedEntityId(id);
  }, []);

  const setFocusTarget = useCallback((target: FocusTarget | null) => {
    setFocusTargetState(target);
  }, []);

  /** Clears both selectedEntityId and focusTarget, used by DetailPanel on close. */
  const clearSelection = useCallback(() => {
    setSelectedEntityId(null);
    setFocusTargetState(null);
  }, []);

  const value = useMemo<DashboardState>(
    () => ({
      selectedEntityId,
      focusTarget,
      setSelectedEntity,
      setFocusTarget,
      clearSelection,
    }),
    [selectedEntityId, focusTarget, setSelectedEntity, setFocusTarget, clearSelection]
  );

  return (
    <DashboardContext.Provider value={value}>
      {children}
    </DashboardContext.Provider>
  );
}

/**
 * Hook to access dashboard store state and actions.
 * Returns the full state object — destructure what you need:
 *   const { selectedEntityId, clearSelection } = useDashboardStore();
 */
export function useDashboardStore(): DashboardState {
  const ctx = useContext(DashboardContext);
  if (!ctx) {
    throw new Error('useDashboardStore must be used within a DashboardProvider');
  }
  return ctx;
}