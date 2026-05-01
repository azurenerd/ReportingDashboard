import React, { useEffect, useState } from 'react';

/**
 * WebGLFallback: Gates rendering on WebGL2 support (AC #8).
 * If the browser lacks WebGL2, displays a styled fallback message
 * instead of rendering the 3D dashboard.
 */
export default function WebGLFallback({ children }: { children: React.ReactNode }) {
  const [supported, setSupported] = useState<boolean | null>(null);

  useEffect(() => {
    try {
      const canvas = document.createElement('canvas');
      const gl = canvas.getContext('webgl2');
      setSupported(gl !== null);
    } catch {
      setSupported(false);
    }
  }, []);

  // Don't render anything until detection completes
  if (supported === null) {
    return (
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        height: '100vh',
        background: '#0a0a1a',
        color: 'rgba(255,255,255,0.6)',
      }}>
        <p>Checking WebGL support...</p>
      </div>
    );
  }

  if (!supported) {
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
          <div style={{
            width: 80,
            height: 80,
            margin: '0 auto 24px',
            borderRadius: '50%',
            background: 'rgba(255, 82, 82, 0.15)',
            border: '2px solid rgba(255, 82, 82, 0.4)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 36,
          }}>
            ⚠
          </div>
          <h1 style={{
            fontSize: 28,
            fontWeight: 700,
            marginBottom: 12,
            color: '#ff5252',
          }}>
            WebGL 2.0 Required
          </h1>
          <p style={{
            color: 'rgba(255,255,255,0.6)',
            fontSize: 16,
            lineHeight: 1.6,
            maxWidth: 480,
          }}>
            This dashboard requires a browser with WebGL 2.0 support for 3D rendering.
            Please use a recent version of Chrome, Edge, or Firefox.
          </p>
          <p style={{
            color: 'rgba(255,255,255,0.3)',
            fontSize: 13,
            marginTop: 20,
          }}>
            Tip: Ensure hardware acceleration is enabled in your browser settings.
          </p>
        </div>
      </div>
    );
  }

  return <>{children}</>;
}