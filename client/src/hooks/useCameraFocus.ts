import { useEffect } from 'react';
import { useDashboardStore } from '../store/dashboardStore';

export function useCameraFocus(): void {
  const { focusTarget } = useDashboardStore();
  useEffect(() => {
    if (focusTarget) console.debug('[useCameraFocus] Target:', focusTarget);
  }, [focusTarget]);
}