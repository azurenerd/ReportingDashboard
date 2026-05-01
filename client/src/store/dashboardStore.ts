import { createContext, useContext, useState, useCallback, createElement, type ReactNode } from 'react';

interface FocusTarget {
  x: number;
  y: number;
  z: number;
}

interface DashboardStoreState {
  selectedEntityId: string | null;
  focusTarget: FocusTarget | null;
  setSelectedEntity: (id: string | null) => void;
  setFocusTarget: (target: FocusTarget | null) => void;
  clearSelection: () => void;
}

const DashboardStoreContext = createContext<DashboardStoreState | null>(null);

export function DashboardStoreProvider({ children }: { children: ReactNode }) {
  const [selectedEntityId, setSelectedEntityId] = useState<string | null>(null);
  const [focusTarget, setFocusTargetState] = useState<FocusTarget | null>(null);

  const setSelectedEntity = useCallback((id: string | null) => setSelectedEntityId(id), []);
  const setFocusTarget = useCallback((target: FocusTarget | null) => setFocusTargetState(target), []);
  const clearSelection = useCallback(() => {
    setSelectedEntityId(null);
    setFocusTargetState(null);
  }, []);

  // Use createElement instead of JSX so this file can remain .ts per architecture contract
  return createElement(
    DashboardStoreContext.Provider,
    { value: { selectedEntityId, focusTarget, setSelectedEntity, setFocusTarget, clearSelection } },
    children,
  );
}

export function useDashboardStore(): DashboardStoreState {
  const ctx = useContext(DashboardStoreContext);
  if (!ctx) throw new Error('useDashboardStore must be used within DashboardStoreProvider');
  return ctx;
}