export type RiskSeverity = 'Critical' | 'High' | 'Medium' | 'Low';

export interface Risk {
  id: string;
  title: string;
  description: string;
  severity: RiskSeverity;
  owner: string;
  status: 'Open' | 'Mitigated' | 'Closed';
  impact: string;
  mitigation: string;
  dateIdentified: string;
}