import { useDashboardStore } from '../store';

export function LoadingState() {
  const isLoading = useDashboardStore((s) => s.isLoading);
  const loadError = useDashboardStore((s) => s.loadError);
  const fetchAllData = useDashboardStore((s) => s.fetchAllData);

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 100,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        background: '#0a0a0f',
      }}
    >
      {isLoading && !loadError && (
        <>
          <div
            style={{
              width: 48,
              height: 48,
              border: '3px solid rgba(0, 212, 255, 0.2)',
              borderTopColor: '#00d4ff',
              borderRadius: '50%',
              animation: 'spin 1s linear infinite',
            }}
          />
          <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
          <p style={{ marginTop: 24, color: '#a0a0b0', fontSize: 14, letterSpacing: 2 }}>
            INITIALIZING COMMAND CENTER...
          </p>
        </>
      )}
      {loadError && (
        <div style={{ textAlign: 'center' }}>
          <p style={{ color: '#ff4444', fontSize: 16, marginBottom: 16 }}>{loadError}</p>
          <button
            onClick={() => fetchAllData()}
            style={{
              padding: '8px 24px',
              background: 'rgba(0, 212, 255, 0.15)',
              border: '1px solid rgba(0, 212, 255, 0.3)',
              borderRadius: 8,
              color: '#00d4ff',
              cursor: 'pointer',
              fontSize: 14,
            }}
          >
            Retry
          </button>
        </div>
      )}
    </div>
  );
}