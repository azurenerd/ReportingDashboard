import type { Request, Response, NextFunction } from 'express';

/**
 * Centralized error-handling middleware.
 * Returns a consistent JSON error shape: { error: { code, message } }.
 * Must be registered AFTER all route handlers.
 */
export function errorHandler(
  err: Error & { statusCode?: number; code?: string },
  _req: Request,
  res: Response,
  _next: NextFunction,
): void {
  const statusCode = err.statusCode ?? 500;
  const code = err.code ?? 'INTERNAL_ERROR';

  console.error(`[ErrorHandler] ${code}: ${err.message}`);

  res.status(statusCode).json({
    error: {
      code,
      message: err.message || 'An unexpected error occurred',
    },
  });
}