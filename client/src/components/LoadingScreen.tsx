/** Full-screen loading state shown during initial data fetch. */
export default function LoadingScreen() {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-950">
      <div className="text-center">
        <div className="w-12 h-12 border-2 border-accent-cyan border-t-transparent rounded-full animate-spin mx-auto" />
        <p className="mt-4 text-sm text-gray-400 font-medium">Initializing Command Center…</p>
      </div>
    </div>
  );
}