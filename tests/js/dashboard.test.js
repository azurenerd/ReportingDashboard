import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

// Mock dashboard module
const createDashboard = () => {
  const state = {
    isVisible: false,
    data: []
  };

  return {
    show() {
      state.isVisible = true;
    },
    hide() {
      state.isVisible = false;
    },
    isVisible() {
      return state.isVisible;
    },
    loadData(data) {
      state.data = data;
      return state.data;
    },
    getData() {
      return state.data;
    }
  };
};

describe('Dashboard Module', () => {
  let dashboard;

  beforeEach(() => {
    dashboard = createDashboard();
  });

  afterEach(() => {
    dashboard = null;
  });

  it('should initialize with hidden state', () => {
    expect(dashboard.isVisible()).toBe(false);
  });

  it('should show dashboard when called', () => {
    dashboard.show();
    expect(dashboard.isVisible()).toBe(true);
  });

  it('should hide dashboard when called', () => {
    dashboard.show();
    dashboard.hide();
    expect(dashboard.isVisible()).toBe(false);
  });

  it('should load and return data', () => {
    const testData = [{ id: 1, name: 'Test' }];
    const result = dashboard.loadData(testData);
    
    expect(result).toEqual(testData);
    expect(dashboard.getData()).toEqual(testData);
  });

  it('should handle empty data array', () => {
    dashboard.loadData([]);
    expect(dashboard.getData()).toEqual([]);
  });

  it('should replace existing data on load', () => {
    dashboard.loadData([{ id: 1 }]);
    dashboard.loadData([{ id: 2 }]);
    
    expect(dashboard.getData()).toEqual([{ id: 2 }]);
  });
});