import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { get } from '../api/client';
import { useAnimatedValue } from '../hooks/useAnimatedValue';
import type { ProjectSummary } from '../types';

/** Returns the appropriate Tailwind text color class for a health color value. */
function healthColorClass(color: 'green' | 'yellow' | 'red'): string {
  switch (color) {
    case 'green':
      return 'text-accent-green';
    case 'yellow':
      return 'text-accent-orange';
    case 'red':
      return 'text-accent-red';
  }
}

/** Returns a glow border class based on health color. */
function healthGlowClass(color: 'green' | 'yellow' | 'red'): string {
  switch (color) {
    case 'green':
      return 'glow-border-green';
    case 'yellow':
      return 'glow-border-orange';
    case 'red':
      return 'glow-border-red';
  }
}

/** Animated metric card displaying a single numeric KPI. */
function MetricCard({
  label,
  value,
  suffix = '',
  glowClass = 'glow-border-cyan',
  colorClass = 'text-accent-cyan',
  delay = 0,
}: {
  label: string;
  value: number;
  suffix?: string;
  glowClass?: string;
  colorClass?: string;
  delay?: number;
}) {
  const animatedValue = useAnimatedValue(value, 1200);

  return (
    <motion.div
      className={`glass-card ${glowClass} flex flex-col items-center justify-center min-w-[140px]`}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, delay }}
      <span className="text-xs font-medium text-gray-400 uppercase tracking-wider mb-1">
        {label}
      </span>
      <span className={`text-3xl font-bold ${colorClass} tabular-nums`}>
        {animatedValue}
        {suffix}
      </span>
    </motion.div>
  );
}

/** Loading skeleton shown while project summary is being fetched. */
function LoadingSkeleton() {
  return (
    <div className="absolute top-4 left-4 z-10 flex flex-wrap gap-3 max-w-[780px]">
      {Array.from({ length: 5 }).map((_, i) => (
        <div
          key={i}
          className="glass-card glow-border-cyan min-w-[140px] h-[88px] animate-pulse flex items-center justify-center"
          <div className="w-8 h-8 border-2 border-accent-cyan/40 border-t-transparent rounded-full animate-spin" />
        </div>
      ))}
    </div>
  );
}

/** Error state displayed when the API call fails. */
function ErrorState({ message }: { message: string }) {
  return (
    <div className="absolute top-4 left-4 z-10">
      <div className="glass-card glow-border-red max-w-[360px]">
        <div className="flex items-center gap-2">
          <svg
            className="w-5 h-5 text-accent-red flex-shrink-0"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"
            />
          </svg>
          <div>
            <p className="text-sm font-semibold text-accent-red">Failed to load project data</p>
            <p className="text-xs text-gray-400 mt-0.5">{message}</p>
          </div>
        </div>
      </div>
    </div>
  );
}

/**
 * Project Overview Glassmorphism Cards (US-02)
 *
 * Displays key project health metrics in a row of glassmorphism-styled cards:
 * - Project name, status, and current sprint (header card)
 * - Completion percentage with animated counter
 * - Delivery confidence indicator
 * - Days remaining in sprint
 * - Health score with color coding (green >= 80, yellow 50-79, red < 50)
 *
 * Fetches data independently from GET /api/project-summary.
 */
export default function DashboardCards() {
  const [data, setData] = useState<ProjectSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchSummary() {
      try {
        const summary = await get<ProjectSummary>('/project-summary');
        if (!cancelled) {
          setData(summary);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Unknown error');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    fetchSummary();
    return () => {
      cancelled = true;
    };
  }, []);

  if (loading) return <LoadingSkeleton />;
  if (error) return <ErrorState message={error} />;
  if (!data) return null;

  return (
    <div className="absolute top-4 left-4 z-10 flex flex-wrap gap-3 max-w-[780px]">
      {/* Header card: Project name, status, sprint */}
      <motion.div
        className="glass-card glow-border-purple min-w-[200px]"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        <h2 className="text-base font-bold text-white leading-tight">{data.name}</h2>
        <div className="flex items-center gap-2 mt-1.5">
          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold bg-accent-cyan/20 text-accent-cyan uppercase tracking-wide">
            {data.status}
          </span>
          <span className="text-xs text-gray-400">{data.currentSprint}</span>
        </div>
        <div className="flex gap-3 mt-2 text-[10px] text-gray-500">
          <span>{data.totalEpics} Epics</span>
          <span>{data.totalFeatures} Features</span>
          <span>{data.totalStories} Stories</span>
        </div>
      </motion.div>

      {/* Completion percentage */}
      <MetricCard
        label="Completion"
        value={data.completionPercent}
        suffix="%"
        glowClass="glow-border-cyan"
        colorClass="text-accent-cyan"
        delay={0.1}
      />

      {/* Delivery confidence */}
      <MetricCard
        label="Confidence"
        value={data.deliveryConfidence}
        suffix="%"
        glowClass="glow-border-purple"
        colorClass="text-accent-purple"
        delay={0.2}
      />

      {/* Days remaining */}
      <MetricCard
        label="Days Left"
        value={data.daysRemaining}
        suffix=""
        glowClass="glow-border-cyan"
        colorClass="text-white"
        delay={0.3}
      />

      {/* Health score with color coding */}
      <MetricCard
        label="Health"
        value={data.healthScore}
        suffix=""
        glowClass={healthGlowClass(data.healthColor)}
        colorClass={healthColorClass(data.healthColor)}
        delay={0.4}
      />
    </div>
  );
}