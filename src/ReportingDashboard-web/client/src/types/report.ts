export interface ReportDetail {
  id: string;
  title: string;
  type: string;
  description: string;
  owner: string;
  status: string;
  priority: string;
  estimate: number | null;
  remainingWork: number | null;
  dependencies: string[];
  recentActivity: {
    date: string;
    action: string;
    actor: string;
  }[];
  tags: string[];
  createdDate: string;
  updatedDate: string;
}