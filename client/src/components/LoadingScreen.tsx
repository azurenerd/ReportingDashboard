export default function LoadingScreen() {
  return (
    <div className="absolute inset-0 z-50 flex flex-col items-center justify-center bg-gray-950">
      <div className="w-12 h-12 border-2 border-cyan-400 border-t-transparent rounded-full animate-spin" />
      <p className="mt-4 text-cyan-400 text-sm font-medium tracking-wide">Initializing Dashboard...</p>
    </div>
  );
}