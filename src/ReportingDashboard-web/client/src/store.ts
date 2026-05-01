import { create } from 'zustand';
import type {
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  ActivityEvent,
  RoadmapMilestone,
  ReportDetail,
} from './types';
import {
  fetchProjectSummary,
  fetchProjectItems,
  fetchSprintMetrics,
  fetchRisks,
  fetchTeamActivity,
  fetchRoadmap,
  fetchReportDetail,
} from './api';

export interface DashboardStore {
  projectSummary: ProjectSummary | null;
  projectItems: ProjectItem[];
  sprintMetrics: SprintMetrics | null;
  risks: Risk[];
  teamActivity: ActivityEvent[];
  roadmap: RoadmapMilestone[];
  isLoading: boolean;
  loadError: string | null;
  selectedNodeId: string | null;
  detailPanelData: ReportDetail | null;
  isDetailPanelOpen: boolean;
  isFlyInComplete: boolean;
  cameraTarget: { position: [number, number, number]; lookAt: [number, number, number] } | null;
  fetchAllData: () => Promise<void>;
  selectNode: (id: string) => Promise<void>;
  clearSelection: () => void;
  setFlyInComplete: () => void;
}

export const useDashboardStore = create<DashboardStore>((set, get) => ({
  projectSummary: null,
  projectItems: [],
  sprintMetrics: null,
  risks: [],
  teamActivity: [],
  roadmap: [],
  isLoading: true,
  loadError: null,
  selectedNodeId: null,
  detailPanelData: null,
  isDetailPanelOpen: false,
  isFlyInComplete: false,
  cameraTarget: null,

  fetchAllData: async () => {
    set({ isLoading: true, loadError: null });
    try {
      const [projectSummary, projectItems, sprintMetrics, risks, teamActivity, roadmap] =
        await Promise.all([
          fetchProjectSummary(),
          fetchProjectItems(),
          fetchSprintMetrics(),
          fetchRisks(),
          fetchTeamActivity(),
          fetchRoadmap(),
        ]);
      set({ projectSummary, projectItems, sprintMetrics, risks, teamActivity, roadmap, isLoading: false });
    } catch (err) {
      set({ isLoading: false, loadError: err instanceof Error ? err.message : 'Failed to load data' });
    }
  },

  selectNode: async (id: string) => {
    set({ selectedNodeId: id, isDetailPanelOpen: true, detailPanelData: null });
    try {
      const detail = await fetchReportDetail(id);
      if (get().selectedNodeId === id) set({ detailPanelData: detail });
    } catch {
      // detail panel shows loading/empty state
    }
  },

  clearSelection: () => {
    set({ selectedNodeId: null, isDetailPanelOpen: false, detailPanelData: null, cameraTarget: null });
  },

  setFlyInComplete: () => {
    set({ isFlyInComplete: true });
  },
}));