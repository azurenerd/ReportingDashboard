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

const app = express();
const PORT = process.env.PORT ? parseInt(process.env.PORT, 10) : 3001;

app.use(cors({ origin: 'http://localhost:5173' }));
app.use(express.json());

const mockData = generateMockData();
app.use(createProjectRoutes(mockData));
app.use(createSprintRoutes(mockData));
app.use(createRiskRoutes(mockData));
app.use(createTeamRoutes(mockData));
app.use(createRoadmapRoutes(mockData));
app.use(createReportRoutes(mockData));
app.use(errorHandler);

app.listen(PORT, () => {
  console.log(`[Server] ReportingDashboard API running on http://localhost:${PORT}`);
});