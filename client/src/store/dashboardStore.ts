import { createContext, useContext, useState, useCallback, createElement, type ReactNode } from 'react';

/**
 * Dashboard shared state - lightweight React context for cross-component communication.
 * Used for: selected item ID (detail panel), camera focus target, loading states.
 */
interface DashboardState {
  selectedItemId: string | null;
  focusTarget: [number, number, number] | null;
  setSelectedItemId: (id: string | null) => void;
  setFocusTarget: (target: [number, number, number] | null) => void;
}

const DashboardContext = createContext<DashboardState | null>(null);

export function DashboardProvider({ children }: { children: ReactNode }) {
  const [selectedItemId, setSelectedItemIdState] = useState<string | null>(null);
  const [focusTarget, setFocusTargetState] = useState<[number, number, number] | null>(null);

  const setSelectedItemId = useCallback((id: string | null) => {
    setSelectedItemIdState(id);
  }, []);

  const setFocusTarget = useCallback((target: [number, number, number] | null) => {
    setFocusTargetState(target);
  }, []);

  return createElement(
    DashboardContext.Provider,
    { value: { selectedItemId, focusTarget, setSelectedItemId, setFocusTarget } },
    children
  );
}

export function useDashboard(): DashboardState {
  const ctx = useContext(DashboardContext);
  if (!ctx) {
    throw new Error('useDashboard must be used within a DashboardProvider');
  }
  return ctx;
}