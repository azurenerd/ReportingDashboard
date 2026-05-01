import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
  type ChartOptions,
} from 'chart.js';
import { Bar, Line } from 'react-chartjs-2';
import { get } from '../api/client';
import { useAnimatedValue } from '../hooks/useAnimatedValue';
import type { SprintMetrics } from '../types';

/* Register Chart.js components once at module level */
ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
);

/* ── Color constants matching the dark futuristic theme ── */
const CYAN = '#00e5ff';
const CYAN_40 = 'rgba(0, 229, 255, 0.4)';
const PURPLE = '#b388ff';
const PURPLE_40 = 'rgba(179, 136, 255, 0.4)';
const GRID_COLOR = '#ffffff10';
const TICK_COLOR = '#a0aec0';

/* ── Shared Chart.js options factory for dark-mode styling ── */
function makeDarkScales(): ChartOptions<'bar'>['scales'] {
  return {
    x: {
      grid: { color: GRID_COLOR },
      ticks: { color: TICK_COLOR, font: { family: 'Inter', size: 11 } },
      border: { color: GRID_COLOR },
    },
    y: {
      grid: { color: GRID_COLOR },
      ticks: { color: TICK_COLOR, font: { family: 'Inter', size: 11 } },
      border: { color: GRID_COLOR },
      beginAtZero: true,
    },
  };
}

/* ── Velocity bar chart: planned vs completed story points ── */
function VelocityChart({ data }: { data: SprintMetrics }) {
  const chartData = {
    labels: data.velocity.sprints,
    datasets: [
      {
        label: 'Planned',
        data: data.velocity.planned,
        backgroundColor: CYAN_40,
        borderColor: CYAN,
        borderWidth: 1.5,
        borderRadius: 4,
      },
      {
        label: 'Completed',
        data: data.velocity.completed,
        backgroundColor: PURPLE_40,
        borderColor: PURPLE,
        borderWidth: 1.5,
        borderRadius: 4,
      },
    ],
  };

  const options: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const,
        labels: {
          color: TICK_COLOR,
          font: { family: 'Inter', size: 11 },
          boxWidth: 12,
          boxHeight: 12,
          padding: 12,
        },
      },
      title: {
        display: true,
        text: 'Sprint Velocity',
        color: '#ffffff',
        font: { family: 'Inter', size: 13, weight: 600 },
        padding: { bottom: 8 },
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        titleFont: { family: 'Inter' },
        bodyFont: { family: 'Inter' },
        borderColor: 'rgba(255,255,255,0.1)',
        borderWidth: 1,
        cornerRadius: 6,
      },
    },
    scales: makeDarkScales(),
  };

  return <Bar data={chartData} options={options} />;
}

/* ── Burndown line chart: ideal (dashed) vs actual (solid) ── */
function BurndownChart({ data }: { data: SprintMetrics }) {
  /* Filter out null values for the actual line so Chart.js
     draws it only through known data points */
  const actualData = data.burndown.actual.map((v) =>
    v === null || v === undefined ? (null as unknown as number) : v,
  );

  const chartData = {
    labels: data.burndown.days,
    datasets: [
      {
        label: 'Ideal',
        data: data.burndown.ideal,
        borderColor: CYAN,
        backgroundColor: 'transparent',
        borderDash: [6, 4],
        borderWidth: 2,
        pointRadius: 0,
        tension: 0,
      },
      {
        label: 'Actual',
        data: actualData,
        borderColor: PURPLE,
        backgroundColor: PURPLE_40,
        fill: true,
        borderWidth: 2.5,
        pointRadius: 3,
        pointBackgroundColor: PURPLE,
        pointBorderColor: PURPLE,
        tension: 0.25,
        spanGaps: false,
      },
    ],
  };

  const options: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const,
        labels: {
          color: TICK_COLOR,
          font: { family: 'Inter', size: 11 },
          boxWidth: 12,
          boxHeight: 12,
          padding: 12,
        },
      },
      title: {
        display: true,
        text: 'Sprint Burndown',
        color: '#ffffff',
        font: { family: 'Inter', size: 13, weight: 600 },
        padding: { bottom: 8 },
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        titleFont: { family: 'Inter' },
        bodyFont: { family: 'Inter' },
        borderColor: 'rgba(255,255,255,0.1)',
        borderWidth: 1,
        cornerRadius: 6,
      },
    },
    scales: {
      x: {
        grid: { color: GRID_COLOR },
        ticks: { color: TICK_COLOR, font: { family: 'Inter', size: 11 } },
        border: { color: GRID_COLOR },
      },
      y: {
        grid: { color: GRID_COLOR },
        ticks: { color: TICK_COLOR, font: { family: 'Inter', size: 11 } },
        border: { color: GRID_COLOR },
        beginAtZero: true,
      },
    },
  };

  return <Line data={chartData} options={options} />;
}

/* ── Animated metric indicator card ── */
function SprintMetricCard({
  label,
  value,
  icon,
  glowClass,
  colorClass,
  delay = 0,
}: {
  label: string;
  value: number;
  icon: React.ReactNode;
  glowClass: string;
  colorClass: string;
  delay?: number;
}) {
  const animatedValue = useAnimatedValue(value, 1200);

  return (
    <motion.div
      className={`glass-card ${glowClass} flex items-center gap-3 min-w-[130px]`}
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.45, delay }}
      <div className={`flex-shrink-0 ${colorClass}`}>{icon}</div>
      <div className="flex flex-col">
        <span className="text-[10px] font-medium text-gray-400 uppercase tracking-wider">
          {label}
        </span>
        <span className={`text-2xl font-bold ${colorClass} tabular-nums leading-tight`}>
          {animatedValue}
        </span>
      </div>
    </motion.div>
  );
}

/* ── SVG icons for the three metric cards ── */
const BugIcon = (
  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M12 9v2m0 4h.01M5.07 19H19a2 2 0 001.75-2.98L13.75 4.08a2 2 0 00-3.5 0L3.32 16.02A2 2 0 005.07 19z"
    />
  </svg>
);

const BlockerIcon = (
  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M18.364 5.636a9 9 0 11-12.728 0m12.728 0L5.636 18.364"
    />
  </svg>
);

const CarryoverIcon = (
  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
    <path
      strokeLinecap="round"
      strokeLinejoin="round"
      d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
    />
  </svg>
);

/* ── Loading skeleton while sprint data is being fetched ── */
function LoadingSkeleton() {
  return (
    <div className="absolute bottom-4 left-4 z-10 flex flex-col gap-3 max-w-[780px]">
      <div className="flex gap-3">
        {[1, 2].map((i) => (
          <div
            key={i}
            className="glass-card glow-border-cyan w-[360px] h-[220px] animate-pulse flex items-center justify-center"
            <div className="w-8 h-8 border-2 border-accent-cyan/40 border-t-transparent rounded-full animate-spin" />
          </div>
        ))}
      </div>
      <div className="flex gap-3">
        {[1, 2, 3].map((i) => (
          <div
            key={i}
            className="glass-card glow-border-purple w-[130px] h-[72px] animate-pulse"
          />
        ))}
      </div>
    </div>
  );
}

/* ── Error state ── */
function ErrorState({ message }: { message: string }) {
  return (
    <div className="absolute bottom-4 left-4 z-10">
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
            <p className="text-sm font-semibold text-accent-red">
              Failed to load sprint metrics
            </p>
            <p className="text-xs text-gray-400 mt-0.5">{message}</p>
          </div>
        </div>
      </div>
    </div>
  );
}

/**
 * Sprint Metrics Charts Section (US-04)
 *
 * Displays two Chart.js charts and three animated metric cards:
 * - Velocity bar chart: planned vs completed story points across 5 sprints
 * - Burndown line chart: ideal (dashed) vs actual (solid) across 10 days
 * - Open bug count, blocker count, and carryover items with animated counters
 *
 * All data fetched from GET /api/sprint-metrics.
 * Dark theme with glassmorphism containers, cyan/purple accent colors.
 */
export default function SprintCharts() {
  const [data, setData] = useState<SprintMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchMetrics() {
      try {
        const metrics = await get<SprintMetrics>('/sprint-metrics');
        if (!cancelled) setData(metrics);
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Unknown error');
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchMetrics();
    return () => {
      cancelled = true;
    };
  }, []);

  if (loading) return <LoadingSkeleton />;
  if (error) return <ErrorState message={error} />;
  if (!data) return null;

  return (
    <div className="absolute bottom-4 left-4 z-10 flex flex-col gap-3 max-w-[780px] pointer-events-auto">
      {/* Sprint label */}
      <motion.div
        className="flex items-center gap-2"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.4 }}
        <span className="text-xs font-semibold text-accent-cyan uppercase tracking-wider">
          {data.sprintName}
        </span>
        <span className="text-[10px] text-gray-500">
          Sprint {data.sprintNumber}
        </span>
      </motion.div>

      {/* Charts row */}
      <div className="flex gap-3">
        {/* Velocity bar chart */}
        <motion.div
          className="glass-card glow-border-cyan w-[360px]"
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.5, delay: 0.1 }}
          <div className="h-[200px]">
            <VelocityChart data={data} />
          </div>
        </motion.div>

        {/* Burndown line chart */}
        <motion.div
          className="glass-card glow-border-purple w-[360px]"
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.5, delay: 0.2 }}
          <div className="h-[200px]">
            <BurndownChart data={data} />
          </div>
        </motion.div>
      </div>

      {/* Metric indicator cards row */}
      <div className="flex gap-3">
        <SprintMetricCard
          label="Open Bugs"
          value={data.openBugs}
          icon={BugIcon}
          glowClass="glow-border-orange"
          colorClass="text-accent-orange"
          delay={0.3}
        />
        <SprintMetricCard
          label="Blockers"
          value={data.blockers}
          icon={BlockerIcon}
          glowClass="glow-border-red"
          colorClass="text-accent-red"
          delay={0.4}
        />
        <SprintMetricCard
          label="Carryover"
          value={data.carryoverItems}
          icon={CarryoverIcon}
          glowClass="glow-border-purple"
          colorClass="text-accent-purple"
          delay={0.5}
        />
      </div>
    </div>
  );
}