import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import WebGLFallback, { supportsWebGL2 } from '../../client/src/components/WebGLFallback';

describe('WebGLFallback', () => {
  describe('supportsWebGL2', () => {
    let createElementSpy: ReturnType<typeof vi.spyOn>;

    afterEach(() => {
      createElementSpy?.mockRestore();
    });

    it('[Unit] returns true when WebGL2 context is available', () => {
      const mockContext = {};
      const mockCanvas = {
        getContext: vi.fn().mockReturnValue(mockContext),
      };
      createElementSpy = vi
        .spyOn(document, 'createElement')
        .mockReturnValue(mockCanvas as unknown as HTMLElement);

      expect(supportsWebGL2()).toBe(true);
      expect(mockCanvas.getContext).toHaveBeenCalledWith('webgl2');
    });

    it('[Unit] returns false when WebGL2 context is null', () => {
      const mockCanvas = {
        getContext: vi.fn().mockReturnValue(null),
      };
      createElementSpy = vi
        .spyOn(document, 'createElement')
        .mockReturnValue(mockCanvas as unknown as HTMLElement);

      expect(supportsWebGL2()).toBe(false);
    });

    it('[Unit] returns false when getContext throws an exception', () => {
      const mockCanvas = {
        getContext: vi.fn().mockImplementation(() => {
          throw new Error('WebGL not supported');
        }),
      };
      createElementSpy = vi
        .spyOn(document, 'createElement')
        .mockReturnValue(mockCanvas as unknown as HTMLElement);

      expect(supportsWebGL2()).toBe(false);
    });
  });

  describe('WebGLFallback component', () => {
    it('[Unit] renders the "WebGL2 Not Supported" heading', () => {
      render(<WebGLFallback />);

      expect(
        screen.getByRole('heading', { name: /WebGL2 Not Supported/i })
      ).toBeInTheDocument();
    });

    it('[Unit] renders browser recommendations and chrome://gpu tip', () => {
      render(<WebGLFallback />);

      expect(screen.getByText(/Chrome/)).toBeInTheDocument();
      expect(screen.getByText(/Edge/)).toBeInTheDocument();
      expect(screen.getByText(/Firefox/)).toBeInTheDocument();
      expect(screen.getByText(/chrome:\/\/gpu/)).toBeInTheDocument();
    });

    it('[Unit] renders full-screen overlay with dark background styling', () => {
      const { container } = render(<WebGLFallback />);
      const overlay = container.firstElementChild as HTMLElement;

      expect(overlay.className).toContain('fixed');
      expect(overlay.className).toContain('inset-0');
      expect(overlay.className).toContain('z-50');
      expect(overlay.className).toContain('bg-gray-950');
    });

    it('[Unit] renders the WebGL2 requirement explanation text', () => {
      render(<WebGLFallback />);

      expect(
        screen.getByText(/requires WebGL2 for 3D rendering/i)
      ).toBeInTheDocument();
      expect(
        screen.getByText(/hardware acceleration enabled/i)
      ).toBeInTheDocument();
    });

    it('[Unit] renders the glassmorphism card wrapper with max-w-lg constraint', () => {
      const { container } = render(<WebGLFallback />);
      const card = container.querySelector('.glass-card');

      expect(card).not.toBeNull();
      expect(card!.className).toContain('max-w-lg');
      expect(card!.className).toContain('text-center');
    });
  });
});