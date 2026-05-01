import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import React from 'react';
import {
  DashboardStoreProvider,
  useDashboardStore,
} from '../../client/src/store/dashboardStore';

// Wrapper that provides the DashboardContext
const wrapper = ({ children }: { children: React.ReactNode }) =>
  React.createElement(DashboardStoreProvider, null, children);

describe('dashboardStore', () => {
  it('has correct initial state', () => {
    const { result } = renderHook(() => useDashboardStore(), { wrapper });
    expect(result.current.selectedEntityId).toBeNull();
    expect(result.current.focusTarget).toBeNull();
  });

  it('setSelectedEntity updates selectedEntityId', () => {
    const { result } = renderHook(() => useDashboardStore(), { wrapper });
    act(() => {
      result.current.setSelectedEntity('epic-001');
    });
    expect(result.current.selectedEntityId).toBe('epic-001');
  });

  it('setFocusTarget updates focusTarget', () => {
    const { result } = renderHook(() => useDashboardStore(), { wrapper });
    const target = { x: 1, y: 2, z: 3 };
    act(() => {
      result.current.setFocusTarget(target);
    });
    expect(result.current.focusTarget).toEqual(target);
  });

  it('clearSelection resets both selectedEntityId and focusTarget', () => {
    const { result } = renderHook(() => useDashboardStore(), { wrapper });
    act(() => {
      result.current.setSelectedEntity('feat-005');
      result.current.setFocusTarget({ x: 10, y: 20, z: 30 });
    });
    act(() => {
      result.current.clearSelection();
    });
    expect(result.current.selectedEntityId).toBeNull();
    expect(result.current.focusTarget).toBeNull();
  });

  it('throws when used outside DashboardStoreProvider', () => {
    expect(() => {
      renderHook(() => useDashboardStore());
    }).toThrow('useDashboardStore must be used within a DashboardStoreProvider');
  });
});