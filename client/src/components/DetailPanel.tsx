import { useDashboardStore } from '../store/dashboardStore';

export default function DetailPanel() {
  const { selectedEntityId } = useDashboardStore();
  if (!selectedEntityId) return null;
  return (
    <div className="absolute top-0 right-0 h-full w-96 glass-panel z-20 p-6">
      <p className="text-gray-400 text-sm">Detail panel for: {selectedEntityId}</p>
    </div>
  );
}