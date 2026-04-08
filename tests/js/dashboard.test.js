import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

describe('Dashboard Module', () => {
  let Dashboard;
  let mockChart;

  beforeEach(() => {
    // Mock Chart.js
    mockChart = vi.fn(function() {
      this.destroy = vi.fn();
      this.resize = vi.fn();
    });
    window.Chart = mockChart;

    // Reset DOM
    document.body.innerHTML = '<div id="timelineContainer"><canvas></canvas></div>';
    
    // Import dashboard after mocking
    Dashboard = require('../wwwroot/js/dashboard.js').Dashboard;
  });

  afterEach(() => {
    vi.clearAllMocks();
    document.body.innerHTML = '';
  });

  describe('Initialization', () => {
    it('should initialize dashboard once', () => {
      Dashboard.init();
      Dashboard.init();
      expect(Dashboard.state.isInitialized).toBe(true);
    });

    it('should attach event listeners on init', () => {
      const attachSpy = vi.spyOn(Dashboard, 'attachEventListeners');
      Dashboard.init();
      expect(attachSpy).toHaveBeenCalled();
    });
  });

  describe('Timeline Initialization', () => {
    it('should initialize timeline with canvas element', () => {
      Dashboard.initializeTimeline();
      expect(Dashboard.state.timelineChart).toBeDefined();
    });

    it('should warn when timeline container not found', () => {
      document.body.innerHTML = '';
      const warnSpy = vi.spyOn(console, 'warn');
      Dashboard.initializeTimeline();
      expect(warnSpy).toHaveBeenCalled();
    });

    it('should warn when canvas element not found', () => {
      document.body.innerHTML = '<div id="timelineContainer"></div>';
      const warnSpy = vi.spyOn(console, 'warn');
      Dashboard.initializeTimeline();
      expect(warnSpy).toHaveBeenCalledWith('Timeline canvas element not found');
    });

    it('should warn when no milestone data available', () => {
      Dashboard.getMilestoneData = vi.fn(() => []);
      const warnSpy = vi.spyOn(console, 'warn');
      Dashboard.initializeTimeline();
      expect(warnSpy).toHaveBeenCalledWith('No milestone data available');
    });
  });

  describe('Milestone Data Retrieval', () => {
    it('should get data from window.dashboardData', () => {
      const testData = [{ name: 'Phase 1', status: 'Completed' }];
      window.dashboardData = { milestones: testData };
      
      const data = Dashboard.getMilestoneData();
      expect(data).toEqual(testData);
    });

    it('should fallback to data attribute parsing', () => {
      const testData = [{ name: 'Phase 2', status: 'InProgress' }];
      const container = document.getElementById('timelineContainer');
      container.dataset.milestones = JSON.stringify(testData);
      delete window.dashboardData;
      
      const data = Dashboard.getMilestoneData();
      expect(data).toEqual(testData);
    });

    it('should return empty array when no data available', () => {
      const data = Dashboard.getMilestoneData();
      expect(data).toEqual([]);
    });

    it('should handle invalid JSON in data attribute', () => {
      const container = document.getElementById('timelineContainer');
      container.dataset.milestones = '{invalid json}';
      const errorSpy = vi.spyOn(console, 'error');
      
      const data = Dashboard.getMilestoneData();
      expect(data).toEqual([]);
      expect(errorSpy).toHaveBeenCalled();
    });
  });

  describe('Timeline Rendering', () => {
    it('should create Chart.js instance with correct config', () => {
      const mockCtx = { canvas: {} };
      const milestones = [
        { name: 'M1', status: 'Completed' },
        { name: 'M2', status: 'InProgress' }
      ];
      
      Dashboard.renderTimeline(mockCtx, milestones);
      expect(mockChart).toHaveBeenCalled();
    });

    it('should destroy existing chart before creating new one', () => {
      const mockCtx = { canvas: {} };
      const milestones = [{ name: 'M1', status: 'Completed' }];
      
      Dashboard.renderTimeline(mockCtx, milestones);
      const firstChart = Dashboard.state.timelineChart;
      firstChart.destroy = vi.fn();
      
      Dashboard.renderTimeline(mockCtx, milestones);
      expect(firstChart.destroy).toHaveBeenCalled();
    });

    it('should map milestones to labels', () => {
      const mockCtx = { canvas: {} };
      const milestones = [
        { name: 'Phase 1', status: 'Completed' },
        { name: 'Phase 2', status: 'InProgress' }
      ];
      
      Dashboard.renderTimeline(mockCtx, milestones);
      const chartConfig = mockChart.mock.calls[0][1];
      expect(chartConfig.data.labels).toEqual(['Phase 1', 'Phase 2']);
    });
  });

  describe('Status Color Mapping', () => {
    it('should map all milestone statuses to colors', () => {
      const milestones = [
        { name: 'M1', status: 'Completed' },
        { name: 'M2', status: 'InProgress' },
        { name: 'M3', status: 'AtRisk' },
        { name: 'M4', status: 'Future' }
      ];
      
      const datasets = Dashboard.buildTimelineDatasets(milestones);
      expect(datasets).toHaveLength(4);
      expect(datasets[0].borderColor).toContain('39, 174, 96');
      expect(datasets[1].borderColor).toContain('52, 152, 219');
      expect(datasets[2].borderColor).toContain('231, 76, 60');
      expect(datasets[3].borderColor).toContain('149, 165, 166');
    });

    it('should default to Future color for unknown status', () => {
      const milestones = [{ name: 'M1', status: 'Unknown' }];
      const datasets = Dashboard.buildTimelineDatasets(milestones);
      expect(datasets[0].borderColor).toContain('149, 165, 166');
    });
  });

  describe('Responsive Behavior', () => {
    it('should track viewport changes', () => {
      Dashboard.state.currentViewport = null;
      Dashboard.handleResponsiveResize();
      
      expect(Dashboard.state.currentViewport).toBeDefined();
      expect(Dashboard.state.currentViewport.width).toBe(window.innerWidth);
      expect(Dashboard.state.currentViewport.height).toBe(window.innerHeight);
    });

    it('should skip resize if viewport unchanged', () => {
      Dashboard.state.currentViewport = {
        width: window.innerWidth,
        height: window.innerHeight
      };
      
      Dashboard.state.timelineChart = { resize: vi.fn() };
      Dashboard.handleResponsiveResize();
      
      expect(Dashboard.state.timelineChart.resize).not.toHaveBeenCalled();
    });

    it('should apply responsive CSS classes for tablet viewport', () => {
      Object.defineProperty(window, 'innerWidth', { value: 1024, configurable: true });
      Dashboard.applyResponsiveClasses({ width: 1024, height: 768 });
      
      expect(document.documentElement.classList.contains('viewport-tablet')).toBe(true);
    });

    it('should apply responsive CSS classes for desktop viewport', () => {
      Object.defineProperty(window, 'innerWidth', { value: 1280, configurable: true });
      Dashboard.applyResponsiveClasses({ width: 1280, height: 1024 });
      
      expect(document.documentElement.classList.contains('viewport-desktop')).toBe(true);
    });

    it('should apply responsive CSS classes for presentation viewport', () => {
      Dashboard.applyResponsiveClasses({ width: 1920, height: 1080 });
      expect(document.documentElement.classList.contains('viewport-presentation')).toBe(true);
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty milestone array', () => {
      const datasets = Dashboard.buildTimelineDatasets([]);
      expect(datasets).toEqual([]);
    });

    it('should handle null context gracefully', () => {
      expect(() => Dashboard.renderTimeline(null, [])).not.toThrow();
    });

    it('should handle single milestone', () => {
      const milestones = [{ name: 'Solo', status: 'Completed' }];
      const datasets = Dashboard.buildTimelineDatasets(milestones);
      expect(datasets).toHaveLength(1);
      expect(datasets[0].data).toEqual([100]);
    });
  });
});