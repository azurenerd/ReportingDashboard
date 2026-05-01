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

// CORS for local development
app.use(cors({ origin: 'http://localhost:5173' }));
app.use(express.json());

// Generate mock data once at startup
const data = generateMockData();

// Mount API routes
app.use('/api', createProjectRoutes(data));
app.use('/api', createSprintRoutes(data));
app.use('/api', createRiskRoutes(data));
app.use('/api', createTeamRoutes(data));
app.use('/api', createRoadmapRoutes(data));
app.use('/api', createReportRoutes(data));

// Error handler
app.use(errorHandler);

app.listen(PORT, () => {
  console.log(`[Server] Express listening on http://localhost:${PORT}`);
});