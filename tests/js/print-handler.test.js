import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

describe('PrintHandler Module', () => {
  let PrintHandler;

  beforeEach(() => {
    document.body.innerHTML = `
      <div class="timeline-container"><canvas></canvas></div>
      <div class="work-item-list"></div>
      <div class="work-item-column"></div>
      <button>Test Button</button>
    `;
    
    PrintHandler = require('../wwwroot/js/print-handler.js').PrintHandler;
    PrintHandler.state = {
      originalBodyOverflow: null,
      originalHtmlOverflow: null,
      isPrintMode: false
    };
  });

  afterEach(() => {
    vi.clearAllMocks();
    document.body.innerHTML = '';
  });

  describe('Initialization', () => {
    it('should initialize print handler', () => {
      const attachSpy = vi.spyOn(PrintHandler, 'attachPrintListeners');
      const styleSpy = vi.spyOn(PrintHandler, 'addPrintStyles');
      
      PrintHandler.init();
      
      expect(attachSpy).toHaveBeenCalled();
      expect(styleSpy).toHaveBeenCalled();
    });

    it('should not add duplicate print styles', () => {
      PrintHandler.addPrintStyles();
      const stylesBefore = document.querySelectorAll('style#print-handler-styles').length;
      
      PrintHandler.addPrintStyles();
      const stylesAfter = document.querySelectorAll('style#print-handler-styles').length;
      
      expect(stylesAfter).toBe(stylesBefore);
    });
  });

  describe('Print Mode Transitions', () => {
    it('should set print mode on print start', () => {
      PrintHandler.onPrintStart();
      expect(PrintHandler.state.isPrintMode).toBe(true);
    });

    it('should add print-optimized class on print start', () => {
      PrintHandler.onPrintStart();
      expect(document.documentElement.classList.contains('print-optimized')).toBe(true);
    });

    it('should clear print mode on print end', () => {
      PrintHandler.state.isPrintMode = true;
      PrintHandler.onPrintEnd();
      expect(PrintHandler.state.isPrintMode).toBe(false);
    });

    it('should remove print-optimized class on print end', () => {
      document.documentElement.classList.add('print-optimized');
      PrintHandler.onPrintEnd();
      expect(document.documentElement.classList.contains('print-optimized')).toBe(false);
    });
  });

  describe('Scrollbar Handling', () => {
    it('should hide scrollbars on print', () => {
      PrintHandler.hideScrollbars();
      
      expect(document.documentElement.style.overflow).toBe('hidden');
      expect(document.body.style.overflow).toBe('hidden');
    });

    it('should preserve original overflow values', () => {
      document.documentElement.style.overflow = 'auto';
      document.body.style.overflow = 'scroll';
      
      PrintHandler.hideScrollbars();
      
      expect(PrintHandler.state.originalHtmlOverflow).toBe('auto');
      expect(PrintHandler.state.originalBodyOverflow).toBe('scroll');
    });

    it('should add scrollbar-hidden class to containers', () => {
      PrintHandler.hideScrollbars();
      
      const containers = document.querySelectorAll('.timeline-container, .work-item-list');
      containers.forEach(container => {
        expect(container.classList.contains('scrollbar-hidden')).toBe(true);
      });
    });

    it('should restore scrollbars on show', () => {
      PrintHandler.state.originalHtmlOverflow = 'auto';
      PrintHandler.state.originalBodyOverflow = 'scroll';
      
      PrintHandler.showScrollbars();
      
      expect(document.documentElement.style.overflow).toBe('auto');
      expect(document.body.style.overflow).toBe('scroll');
    });

    it('should remove scrollbar-hidden class from containers', () => {
      const containers = document.querySelectorAll('.timeline-container, .work-item-list');
      containers.forEach(container => {
        container.classList.add('scrollbar-hidden');
      });
      
      PrintHandler.showScrollbars();
      
      containers.forEach(container => {
        expect(container.classList.contains('scrollbar-hidden')).toBe(false);
      });
    });

    it('should handle null original overflow values gracefully', () => {
      PrintHandler.state.originalHtmlOverflow = null;
      PrintHandler.state.originalBodyOverflow = null;
      
      expect(() => PrintHandler.showScrollbars()).not.toThrow();
    });
  });

  describe('Print Optimization', () => {
    it('should hide form elements during capture', () => {
      PrintHandler.optimizeForCapture();
      
      const buttons = document.querySelectorAll('button');
      buttons.forEach(btn => {
        expect(btn.style.display).toBe('none');
      });
    });

    it('should adjust timeline container flex wrap', () => {
      PrintHandler.optimizeForCapture();
      
      const timeline = document.querySelector('.timeline-container');
      expect(timeline.style.flexWrap).toBe('wrap');
      expect(timeline.style.gap).toBe('8pt');
    });

    it('should set work item columns to visible overflow', () => {
      PrintHandler.optimizeForCapture();
      
      const columns = document.querySelectorAll('.work-item-column');
      columns.forEach(col => {
        expect(col.style.maxHeight).toBe('none');
        expect(col.style.overflow).toBe('visible');
      });
    });

    it('should disable transitions during print', () => {
      PrintHandler.optimizeForCapture();
      
      expect(document.documentElement.style.transition).toBe('none !important');
    });

    it('should restore normal view after print', () => {
      PrintHandler.optimizeForCapture();
      PrintHandler.restoreNormalView();
      
      const buttons = document.querySelectorAll('button');
      buttons.forEach(btn => {
        expect(btn.style.display).toBe('');
      });
      
      const timeline = document.querySelector('.timeline-container');
      expect(timeline.style.flexWrap).toBe('');
      expect(timeline.style.gap).toBe('');
    });

    it('should re-enable transitions after print', () => {
      PrintHandler.optimizeForCapture();
      PrintHandler.restoreNormalView();
      
      expect(document.documentElement.style.transition).toBe('');
    });
  });

  describe('Print Dialog Triggers', () => {
    it('should call window.print on printDashboard', () => {
      const printSpy = vi.spyOn(window, 'print');
      PrintHandler.printDashboard();
      expect(printSpy).toHaveBeenCalled();
    });

    it('should log message on captureScreenshot', () => {
      const logSpy = vi.spyOn(console, 'log');
      PrintHandler.captureScreenshot();
      expect(logSpy).toHaveBeenCalledWith('Capturing screenshot for PowerPoint export...');
    });

    it('should trigger print on captureScreenshot', () => {
      const printSpy = vi.spyOn(PrintHandler, 'printDashboard');
      PrintHandler.captureScreenshot();
      expect(printSpy).toHaveBeenCalled();
    });
  });

  describe('Viewport Optimization', () => {
    it('should set explicit dimensions for 1280x720', () => {
      PrintHandler.optimizeForViewport(1280, 720);
      
      expect(document.documentElement.style.width).toBe('1280px');
      expect(document.documentElement.style.height).toBe('720px');
    });

    it('should set explicit dimensions for 1920x1080', () => {
      PrintHandler.optimizeForViewport(1920, 1080);
      
      expect(document.documentElement.style.width).toBe('1920px');
      expect(document.documentElement.style.height).toBe('1080px');
    });

    it('should handle edge case dimensions', () => {
      expect(() => PrintHandler.optimizeForViewport(0, 0)).not.toThrow();
      expect(document.documentElement.style.width).toBe('0px');
    });
  });

  describe('Print Styles', () => {
    it('should add print media query styles to document', () => {
      PrintHandler.addPrintStyles();
      const style = document.getElementById('print-handler-styles');
      expect(style).toBeDefined();
      expect(style.textContent).toContain('@media print');
    });

    it('should include scrollbar-hidden rules', () => {
      PrintHandler.addPrintStyles();
      const style = document.getElementById('print-handler-styles');
      expect(style.textContent).toContain('.scrollbar-hidden');
    });

    it('should include print-optimized rules', () => {
      PrintHandler.addPrintStyles();
      const style = document.getElementById('print-handler-styles');
      expect(style.textContent).toContain('.print-optimized');
    });
  });

  describe('Complete Print Workflow', () => {
    it('should execute full print workflow', () => {
      const attachSpy = vi.spyOn(PrintHandler, 'attachPrintListeners');
      const styleSpy = vi.spyOn(PrintHandler, 'addPrintStyles');
      
      PrintHandler.init();
      PrintHandler.onPrintStart();
      
      expect(PrintHandler.state.isPrintMode).toBe(true);
      expect(document.documentElement.classList.contains('print-optimized')).toBe(true);
      
      PrintHandler.onPrintEnd();
      
      expect(PrintHandler.state.isPrintMode).toBe(false);
      expect(document.documentElement.classList.contains('print-optimized')).toBe(false);
    });
  });
});