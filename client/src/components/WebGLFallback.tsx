/**
 * WebGLFallback — Detects WebGL2 support and renders a user-friendly
 * fallback message when the browser cannot run the 3D dashboard.
 *
 * Exports `supportsWebGL2()` utility for use as a gate check in App.tsx.
 */

/**
 * Tests whether the current browser supports WebGL2 by creating a temporary
 * canvas and requesting a webgl2 context.
 */
export function supportsWebGL2(): boolean {
  try {
    const canvas = document.createElement('canvas');
    const gl = canvas.getContext('webgl2');
    return gl !== null;
  } catch {
    return false;
  }
}

/**
 * Full-screen fallback UI shown when WebGL2 is not available.
 * Styled to match the dark futuristic theme with glassmorphism card.
 */
export default function WebGLFallback() {
  return (
    <div className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-gray-950 p-8">
      <div className="glass-card p-10 text-center max-w-lg">
        <div className="text-5xl mb-4">⚠️</div>
        <h1 className="text-2xl font-bold text-red-400 mb-2">
          WebGL2 Not Supported
        </h1>
        <p className="text-gray-400 leading-relaxed">
          This dashboard requires WebGL2 for 3D rendering. Please use a recent
          <strong className="text-gray-300"> Chrome</strong>,
          <strong className="text-gray-300"> Edge</strong>, or
          <strong className="text-gray-300"> Firefox</strong> with hardware
          acceleration enabled.
        </p>
        <p className="mt-4 text-sm text-gray-500">
          Tip: Check{' '}
          <code className="text-cyan-400">chrome://gpu</code> to verify WebGL2
          status.
        </p>
      </div>
    </div>
  );
}