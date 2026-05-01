/** Shown when WebGL2 is not supported by the browser. */
export default function WebGLFallback() {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-950 p-8">
      <div className="glass-card max-w-md text-center">
        <h1 className="text-xl font-bold text-red-400">WebGL2 Not Supported</h1>
        <p className="mt-3 text-sm text-gray-300">
          This dashboard requires WebGL2 to render the 3D scene. Please use a modern browser
          such as Chrome, Edge, or Firefox.
        </p>
      </div>
    </div>
  );
}