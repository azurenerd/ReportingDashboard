export default function WebGLFallback() {
  return (
    <div className="absolute inset-0 z-50 flex flex-col items-center justify-center bg-gray-950 p-8">
      <h1 className="text-2xl font-bold text-red-400">WebGL2 Not Supported</h1>
      <p className="mt-4 text-gray-400 text-center max-w-md">
        This dashboard requires WebGL2. Please use Chrome, Edge, or Firefox with hardware acceleration.
      </p>
    </div>
  );
}