import { useState, useEffect, useCallback } from 'react';

/** Activity event shape matching the GET /api/team-activity contract */
export interface TeamActivityEvent {
  id: string;
  type: 'pr-completed' | 'task-completed' | 'comment' | 'deployment' | 'review';
  actor: string;
  actorAvatar: string;
  description: string;
  timestamp: string;
  relatedItemId: string | null;
}

export interface TeamActivityMember {
  id: string;
  name: string;
  role: string;
  avatar: string;
}

export interface TeamActivityResponse {
  events: TeamActivityEvent[];
  teamMembers: TeamActivityMember[];
}

/**
 * Fetches team activity data from GET /api/team-activity.
 * Uses the standard fetch pattern consistent with the architecture's api/client.ts approach.
 * Returns { data, error, isLoading } tuple.
 */
export function useTeamActivity() {
  const [data, setData] = useState<TeamActivityResponse | null>(null);
  const [error, setError] = useState<Error | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const fetchData = useCallback(async () => {
    try {
      const res = await fetch('/api/team-activity');
      if (!res.ok) {
        throw new Error(`API error: ${res.status} ${res.statusText}`);
      }
      const json: TeamActivityResponse = await res.json();
      setData(json);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err : new Error('Failed to fetch team activity'));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return { data, error, isLoading };
}