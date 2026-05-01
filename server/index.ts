/**
 * Express backend entry point for the ReportingDashboard.
 *
 * Architecture:
 * - Mock data is generated ONCE at startup via generateMockData() and held in memory.
 * - Each route module is a factory function that receives the data singleton
 *   and returns an Express Router. This pattern keeps routes pure and testable.
 * - CORS is configured for the Vite dev server origin (localhost:5173).
 * - The centralized errorHandler is mounted last to catch any unhandled errors.
 */
import express from 'express';
import cors from 'cors';
import { generateMockData } from './data/mockData.js';
import { createProjectRoutes } from './routes/projectRoutes.js';
import { createSprintRoutes } from './routes/sprintRoutes.js';
import { createRiskRoutes } from './routes/riskRoutes.js';
import { createTeamRoutes } from './routes/teamRoutes.js';
import { createRoadmapRoutes } from './routes/roadmapRoutes.js';
import { createReportRoutes } from './routes/reportRoutes.js';
import { errorHandler } from './middleware/errorHandler.js';

const PORT = parseInt(process.env.PORT || '3001', 10);
const app = express();

// ── Middleware ──
app.use(cors({ origin: 'http://localhost:5173' }));
app.use(express.json());

// ── Generate mock data singleton at startup (deterministic, in-memory) ──
const data = generateMockData();
console.log(
  `[Server] Mock data loaded: ${data.projectItems.length} items, ` +
  `${data.risks.length} risks, ${data.teamActivity.events.length} events, ` +
  `${data.roadmap.milestones.length} milestones`
);

// ── Mount API routes (all prefixed with /api) ──
app.use('/api', createProjectRoutes(data));
app.use('/api', createSprintRoutes(data));
app.use('/api', createRiskRoutes(data));
app.use('/api', createTeamRoutes(data));
app.use('/api', createRoadmapRoutes(data));
app.use('/api', createReportRoutes(data));

// ── Centralized error handler (must be last middleware) ──
app.use(errorHandler);

// ── Start server ──
app.listen(PORT, () => {
  console.log(`[Server] Express listening on http://localhost:${PORT}`);
  console.log(`[Server] Endpoints available:`);
  console.log(`  GET /api/project-summary`);
  console.log(`  GET /api/project-items`);
  console.log(`  GET /api/sprint-metrics`);
  console.log(`  GET /api/risks`);
  console.log(`  GET /api/team-activity`);
  console.log(`  GET /api/roadmap`);
  console.log(`  GET /api/report/:id`);
});