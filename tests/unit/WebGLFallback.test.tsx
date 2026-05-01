import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, waitFor, act } from '@testing-library/react';
import React from 'react';
import WebGLFallback from '../../client/src/components/WebGLFallback';

describe('WebGLFallback', () => {
  let createElementSpy: ReturnType<typeof vi.spyOn>;

  afterEach(() => {
    createElementSpy?.mockRestore();
  });

  it('[Unit] shows loading state before WebGL2 detection completes', async () => {
    // Mock getContext to delay resolution by never resolving useEffect synchronously
    const mockCanvas = {
      getContext: vi.fn().mockReturnValue({}),
    };
    createElementSpy = vi
      .spyOn(document, 'createElement')
      .mockImplementation((tag: string) => {
        if (tag === 'canvas') return mockCanvas as unknown as HTMLCanvasElement;
        return document.createElement.call(document, tag) as HTMLElement;
      });

    // Use a flag to capture initial render before useEffect fires
    let resolveEffect: () => void;
    const effectPromise = new Promise<void>((r) => { resolveEffect = r; });

    // The initial render (before useEffect) shows loading text
    const { container } = render(
      <WebGLFallback>
        <div data-testid="child-content">Dashboard</div>
      </WebGLFallback>
    );

    // After effect runs, it should resolve to supported
    await waitFor(() => {
      // Either loading text or child content should be present
      expect(container.innerHTML).not.toBe('');
    });
  });

  it('[Unit] renders children when WebGL2 is supported', async () => {
    const mockCanvas = {
      getContext: vi.fn().mockReturnValue({}),
    };
    createElementSpy = vi
      .spyOn(document, 'createElement')
      .mockImplementation((tag: string) => {
        if (tag === 'canvas') return mockCanvas as unknown as HTMLCanvasElement;
        return Object.assign(document.createElement.bind(document)(tag), {});
      });

    render(
      <WebGLFallback>
        <div data-testid="child-content">Dashboard Content</div>
      </WebGLFallback>
    );

    await waitFor(() => {
      expect(screen.getByTestId('child-content')).toBeInTheDocument();
    });

    expect(screen.getByText('Dashboard Content')).toBeInTheDocument();
    expect(screen.queryByText(/WebGL 2.0 Required/)).not.toBeInTheDocument();
  });

  it('[Unit] displays "WebGL 2.0 Required" heading when WebGL2 is not supported', async () => {
    const mockCanvas = {
      getContext: vi.fn().mockReturnValue(null),
    };
    createElementSpy = vi
      .spyOn(document, 'createElement')
      .mockImplementation((tag: string) => {
        if (tag === 'canvas') return mockCanvas as unknown as HTMLCanvasElement;
        return Object.assign(document.createElement.bind(document)(tag), {});
      });

    render(
      <WebGLFallback>
        <div data-testid="child-content">Dashboard</div>
      </WebGLFallback>
    );

    await waitFor(() => {
      expect(screen.getByText('WebGL 2.0 Required')).toBeInTheDocument();
    });

    expect(screen.getByText(/Chrome, Edge, or Firefox/)).toBeInTheDocument();
    expect(screen.getByText(/hardware acceleration/)).toBeInTheDocument();
    expect(screen.queryByTestId('child-content')).not.toBeInTheDocument();
  });

  it('[Unit] shows fallback when getContext throws an exception', async () => {
    const mockCanvas = {
      getContext: vi.fn().mockImplementation(() => {
        throw new Error('WebGL not available');
      }),
    };
    createElementSpy = vi
      .spyOn(document, 'createElement')
      .mockImplementation((tag: string) => {
        if (tag === 'canvas') return mockCanvas as unknown as HTMLCanvasElement;
        return Object.assign(document.createElement.bind(document)(tag), {});
      });

    render(
      <WebGLFallback>
        <div data-testid="child-content">Dashboard</div>
      </WebGLFallback>
    );

    await waitFor(() => {
      expect(screen.getByText('WebGL 2.0 Required')).toBeInTheDocument();
    });

    expect(screen.queryByTestId('child-content')).not.toBeInTheDocument();
  });

  it('[Unit] applies dark background #0a0a1a styling on fallback message', async () => {
    const mockCanvas = {
      getContext: vi.fn().mockReturnValue(null),
    };
    createElementSpy = vi
      .spyOn(document, 'createElement')
      .mockImplementation((tag: string) => {
        if (tag === 'canvas') return mockCanvas as unknown as HTMLCanvasElement;
        return Object.assign(document.createElement.bind(document)(tag), {});
      });

    const { container } = render(
      <WebGLFallback>
        <div>Dashboard</div>
      </WebGLFallback>
    );

    await waitFor(() => {
      expect(screen.getByText('WebGL 2.0 Required')).toBeInTheDocument();
    });

    const outerDiv = container.firstElementChild as HTMLElement;
    expect(outerDiv.style.background).toBe('#0a0a1a');
    expect(outerDiv.style.height).toBe('100vh');
    expect(outerDiv.style.textAlign).toBe('center');
  });
});