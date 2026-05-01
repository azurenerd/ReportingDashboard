import { describe, it, expect, beforeEach } from 'vitest';
import { useDashboardStore } from '../../client/src/store/dashboardStore';

describe('dashboardStore', () => {
  beforeEach(() => {
    useDashboardStore.setState({ selectedEntityId: null, focusTarget: null });
  });

  it('has correct initial state', () => {
    const state = useDashboardStore.getState();
    expect(state.selectedEntityId).toBeNull();
    expect(state.focusTarget).toBeNull();
  });

  it('setSelectedEntity updates selectedEntityId', () => {
    useDashboardStore.getState().setSelectedEntity('epic-001');
    expect(useDashboardStore.getState().selectedEntityId).toBe('epic-001');
  });

  it('setFocusTarget updates focusTarget', () => {
    const target = { x: 1, y: 2, z: 3 };
    useDashboardStore.getState().setFocusTarget(target);
    expect(useDashboardStore.getState().focusTarget).toEqual(target);
  });

  it('clearSelection resets both selectedEntityId and focusTarget', () => {
    useDashboardStore.getState().setSelectedEntity('feat-005');
    useDashboardStore.getState().setFocusTarget({ x: 10, y: 20, z: 30 });
    useDashboardStore.getState().clearSelection();
    const state = useDashboardStore.getState();
    expect(state.selectedEntityId).toBeNull();
    expect(state.focusTarget).toBeNull();
  });

  it('setSelectedEntity accepts null', () => {
    useDashboardStore.getState().setSelectedEntity('story-001');
    useDashboardStore.getState().setSelectedEntity(null);
    expect(useDashboardStore.getState().selectedEntityId).toBeNull();
  });
});