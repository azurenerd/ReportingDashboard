import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import WebGLFallback, { isWebGL2Supported } from '../../client/src/components/WebGLFallback';

describe('WebGLFallback', () => {
  describe('isWebGL2Supported', () => {
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

      expect(isWebGL2Supported()).toBe(true);
      expect(mockCanvas.getContext).toHaveBeenCalledWith('webgl2');
    });

    it('[Unit] returns false when WebGL2 context is null', () => {
      const mockCanvas = {
        getContext: vi.fn().mockReturnValue(null),
      };
      createElementSpy = vi
        .spyOn(document, 'createElement')
        .mockReturnValue(mockCanvas as unknown as HTMLElement);

      expect(isWebGL2Supported()).toBe(false);
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

      expect(isWebGL2Supported()).toBe(false);
    });
  });

  describe('WebGLFallback component', () => {
    it('[Unit] renders the "WebGL2 Not Supported" heading', () => {
      render(<WebGLFallback />);

      expect(
        screen.getByRole('heading', { name: /WebGL2 Not Supported/i })
      ).toBeInTheDocument();
    });

    it('[Unit] renders browser recommendations for Chrome, Edge, and Firefox', () => {
      render(<WebGLFallback />);

      expect(screen.getByText(/Chrome/)).toBeInTheDocument();
      expect(screen.getByText(/Edge/)).toBeInTheDocument();
      expect(screen.getByText(/Firefox/)).toBeInTheDocument();
      expect(screen.getByText(/chrome:\/\/gpu/)).toBeInTheDocument();
    });
  });
});