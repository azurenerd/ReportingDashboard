export type ActivityType =
  | 'PR_Merged'
  | 'PR_Opened'
  | 'Task_Completed'
  | 'Bug_Fixed'
  | 'Comment'
  | 'Deploy'
  | 'Review'
  | 'Sprint_Update';

export interface TeamMember {
  id: string;
  name: string;
  role: string;
  avatar: string;
}

export interface ActivityEvent {
  id: string;
  type: ActivityType;
  description: string;
  member: TeamMember;
  timestamp: string;
  relatedItemId: string | null;
}