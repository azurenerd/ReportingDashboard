// TODO: Remove this temporary verification view once real dashboard components are wired up.
// It renders every hook's output to confirm data loads from all 7 endpoints.

import React, { useState } from 'react';
import {
  useProjectSummary,
  useProjectItems,
  useSprintMetrics,
  useRisks,
  useTeamActivity,
  useRoadmap,
  useReportDetail,
} from './hooks/useProjectData';
import type { AsyncState } from './types';

// Error boundary to prevent blank screen if a hook or child component throws
class ErrorBoundary extends React.Component<
  { children: React.ReactNode },
  { error: Error | null }
  constructor(props: { children: React.ReactNode }) {
    super(props);
    this.state = { error: null };
  }

  static getDerivedStateFromError(error: Error) {
    return { error };
  }

  render() {
    if (this.state.error) {
      return (
        <div style={{ padding: '2rem', background: '#111', color: '#f66', minHeight: '100vh', fontFamily: 'monospace' }}>
          <h1>Render Error</h1>
          <pre>{this.state.error.message}</pre>
        </div>
      );
    }
    return this.props.children;
  }
}

function HookDebug({ label, state }: { label: string; state: AsyncState<unknown> }) {
  return (
    <div style={{ marginBottom: '1rem', fontFamily: 'monospace', fontSize: 12 }}>
      <strong>{label}</strong>
      {state.loading && <span> loading...</span>}
      {state.error && (
        <span style={{ color: '#f66' }}> error: {state.error.message}</span>
      )}
      {!state.loading && !state.error && state.data != null && (
        <pre style={{ maxHeight: 150, overflow: 'auto', marginTop: 4 }}>
          {JSON.stringify(state.data, null, 2).slice(0, 600)}
        </pre>
      )}
    </div>
  );
}

function Dashboard() {
  const [detailId, setDetailId] = useState<string | null>('epic-001');

  const summary = useProjectSummary();
  const items = useProjectItems();
  const sprint = useSprintMetrics();
  const risks = useRisks();
  const activity = useTeamActivity();
  const roadmap = useRoadmap();
  const detail = useReportDetail(detailId);

  return (
    <div style={{ padding: '2rem', background: '#111', color: '#eee', minHeight: '100vh' }}>
      <h1 style={{ marginBottom: '1rem' }}>Hook Verification</h1>
      <HookDebug label="useProjectSummary" state={summary} />
      <HookDebug label="useProjectItems" state={items} />
      <HookDebug label="useSprintMetrics" state={sprint} />
      <HookDebug label="useRisks" state={risks} />
      <HookDebug label="useTeamActivity" state={activity} />
      <HookDebug label="useRoadmap" state={roadmap} />

      <div style={{ marginBottom: '1rem', fontFamily: 'monospace', fontSize: 12 }}>
        <strong>useReportDetail</strong> (id: {detailId ?? 'null'})
        {detail.loading && <span> loading...</span>}
        {detail.error && (
          <span style={{ color: '#f66' }}> error: {detail.error.message}</span>
        )}
        {!detail.loading && !detail.error && detail.data != null && (
          <pre style={{ maxHeight: 150, overflow: 'auto', marginTop: 4 }}>
            {JSON.stringify(detail.data, null, 2).slice(0, 600)}
          </pre>
        )}
        <div style={{ marginTop: 8 }}>
          <label>Change ID:{' '}</label>
          <input
            value={detailId ?? ''}
            onChange={(e) => setDetailId(e.target.value || null)}
            placeholder="e.g. epic-001, risk-001"
            style={{ background: '#222', color: '#eee', border: '1px solid #555', padding: '4px 8px' }}
          />
        </div>
      </div>
    </div>
  );
}

export default function App() {
  return (
    <ErrorBoundary>
      <Dashboard />
    </ErrorBoundary>
  );
}