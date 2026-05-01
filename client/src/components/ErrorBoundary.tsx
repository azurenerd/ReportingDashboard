import React from 'react';

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

/**
 * ErrorBoundary: Catches React render errors and displays a fallback UI
 * instead of crashing the entire dashboard.
 */
export default class ErrorBoundary extends React.Component<
  { children: React.ReactNode },
  ErrorBoundaryState
  constructor(props: { children: React.ReactNode }) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('[ErrorBoundary] Caught error:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          height: '100vh',
          background: '#0a0a1a',
          color: '#ffffff',
          padding: 40,
          textAlign: 'center',
          fontFamily: "'Inter', 'Segoe UI', system-ui, sans-serif",
        }}>
          <div>
            <h1 style={{ fontSize: 24, color: '#ff5252', marginBottom: 12 }}>
              Something went wrong
            </h1>
            <p style={{ color: 'rgba(255,255,255,0.6)', fontSize: 14 }}>
              {this.state.error?.message || 'An unexpected error occurred.'}
            </p>
            <button
              onClick={() => this.setState({ hasError: false, error: null })}
              style={{
                marginTop: 20,
                padding: '8px 20px',
                background: 'rgba(0, 212, 255, 0.2)',
                border: '1px solid rgba(0, 212, 255, 0.4)',
                borderRadius: 8,
                color: '#00d4ff',
                cursor: 'pointer',
                fontSize: 14,
              }}
              Try Again
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}