const express = require('express');
const router = express.Router();
const mockData = require('../data/mockData');

router.get('/project-summary', (_req, res) => {
  res.json(mockData.projectSummary);
});

router.get('/project-items', (_req, res) => {
  res.json(mockData.projectItems);
});

router.get('/sprint-metrics', (_req, res) => {
  res.json(mockData.sprintMetrics);
});

router.get('/risks', (_req, res) => {
  res.json(mockData.risks);
});

router.get('/team-activity', (_req, res) => {
  res.json(mockData.teamActivity);
});

router.get('/roadmap', (_req, res) => {
  res.json(mockData.roadmap);
});

router.get('/report/:id', (req, res) => {
  const detail = mockData.reportDetails[req.params.id];
  if (!detail) {
    return res.status(404).json({ error: 'Not Found' });
  }
  res.json(detail);
});

module.exports = router;