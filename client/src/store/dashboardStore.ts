import React, { createContext, useContext, useState, useCallback, useMemo } from 'react';

export interface FocusTarget {
  x: number;
  y: number;
  z: number;
}

export interface DashboardStoreValue {
  selectedEntityId: string | null;
  focusTarget: FocusTarget | null;
  setSelectedEntity: (id: string | null) => void;
  setFocusTarget: (target: FocusTarget | null) => void;
  clearSelection: () => void;
}

const DashboardContext = createContext<DashboardStoreValue | null>(null);

export function DashboardStoreProvider({ children }: { children: React.ReactNode }): React.JSX.Element {
  const [selectedEntityId, setSelectedEntityId] = useState<string | null>(null);
  const [focusTarget, setFocusTargetState] = useState<FocusTarget | null>(null);

  const setSelectedEntity = useCallback((id: string | null) => {
    setSelectedEntityId(id);
  }, []);

  const setFocusTarget = useCallback((target: FocusTarget | null) => {
    setFocusTargetState(target);
  }, []);

  const clearSelection = useCallback(() => {
    setSelectedEntityId(null);
    setFocusTargetState(null);
  }, []);

  const value = useMemo<DashboardStoreValue>(() => ({
    selectedEntityId,
    focusTarget,
    setSelectedEntity,
    setFocusTarget,
    clearSelection,
  }), [selectedEntityId, focusTarget, setSelectedEntity, setFocusTarget, clearSelection]);

  return React.createElement(DashboardContext.Provider, { value }, children);
}

export function useDashboardStore(): DashboardStoreValue {
  const ctx = useContext(DashboardContext);
  if (!ctx) {
    throw new Error('useDashboardStore must be used within a DashboardStoreProvider');
  }
  return ctx;
}