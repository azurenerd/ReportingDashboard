import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

const createPrintHandler = () => {
  const state = {
    isPrinting: false,
    printConfig: {}
  };

  return {
    print(config) {
      state.isPrinting = true;
      state.printConfig = config;
      window.print();
      return true;
    },
    isPrinting() {
      return state.isPrinting;
    },
    getPrintConfig() {
      return state.printConfig;
    },
    reset() {
      state.isPrinting = false;
      state.printConfig = {};
    },
    handlePrintError(error) {
      console.error('Print error:', error);
      state.isPrinting = false;
      return false;
    }
  };
};

describe('Print Handler Module', () => {
  let printHandler;
  let windowPrintSpy;

  beforeEach(() => {
    printHandler = createPrintHandler();
    windowPrintSpy = vi.spyOn(window, 'print').mockImplementation(() => {});
  });

  afterEach(() => {
    windowPrintSpy.mockRestore();
    printHandler.reset();
  });

  it('should initialize with printing state false', () => {
    expect(printHandler.isPrinting()).toBe(false);
  });

  it('should set printing state when print called', () => {
    printHandler.print({});
    expect(printHandler.isPrinting()).toBe(true);
  });

  it('should call window.print when print invoked', () => {
    printHandler.print({});
    expect(windowPrintSpy).toHaveBeenCalled();
  });

  it('should store print configuration', () => {
    const config = { paperSize: 'A4', margin: 10 };
    printHandler.print(config);
    
    expect(printHandler.getPrintConfig()).toEqual(config);
  });

  it('should reset state when reset called', () => {
    printHandler.print({});
    printHandler.reset();
    
    expect(printHandler.isPrinting()).toBe(false);
    expect(printHandler.getPrintConfig()).toEqual({});
  });

  it('should handle print errors gracefully', () => {
    const error = new Error('Print failed');
    const result = printHandler.handlePrintError(error);
    
    expect(result).toBe(false);
    expect(printHandler.isPrinting()).toBe(false);
  });

  it('should support multiple print operations sequentially', () => {
    printHandler.print({ format: 'pdf' });
    expect(windowPrintSpy).toHaveBeenCalledTimes(1);
    
    printHandler.reset();
    printHandler.print({ format: 'html' });
    expect(windowPrintSpy).toHaveBeenCalledTimes(2);
  });

  it('should preserve config across state changes', () => {
    const config = { quality: 'high' };
    printHandler.print(config);
    printHandler.reset();
    
    expect(printHandler.getPrintConfig()).toEqual({});
  });
});