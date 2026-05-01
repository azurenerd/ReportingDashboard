// Data-fetching hooks for every dashboard endpoint.
// Each hook returns { data, loading, error } (AsyncState<T>).
// All hooks use AbortController to cancel in-flight requests on unmount.

import { useState, useEffect, useCallback } from 'react';
import { api } from '../api/client';
import type {
  AsyncState,
  ProjectSummary,
  ProjectItem,
  SprintMetrics,
  Risk,
  TeamActivity,
  Roadmap,
  ReportDetail,
} from '../types';

/** Returns true if the error was caused by an aborted fetch */
function isAbortError(err: unknown): boolean {
  return err instanceof DOMException && err.name === 'AbortError';
}

/**
 * Generic fetch hook. Accepts a stable fetcher that receives an AbortSignal.
 * Creates a new AbortController per effect cycle and aborts on cleanup.
 */
function useFetch<T>(
  fetcher: (signal: AbortSignal) => Promise<T>,
): AsyncState<T> {
  const [state, setState] = useState<AsyncState<T>>({
    data: null,
    loading: true,
    error: null,
  });

  useEffect(() => {
    const controller = new AbortController();
    setState({ data: null, loading: true, error: null });

    fetcher(controller.signal)
      .then((data) => {
        if (!controller.signal.aborted) {
          setState({ data, loading: false, error: null });
        }
      })
      .catch((err: unknown) => {
        // Silently ignore abort errors - component is unmounting
        if (isAbortError(err)) return;
        if (!controller.signal.aborted) {
          const error = err instanceof Error ? err : new Error(String(err));
          setState({ data: null, loading: false, error });
        }
      });

    return () => {
      controller.abort();
    };
  }, [fetcher]);

  return state;
}

// Individual hooks

export function useProjectSummary(): AsyncState<ProjectSummary> {
  const fetcher = useCallback(
    (signal: AbortSignal) => api.getProjectSummary(signal),
    [],
  );
  return useFetch(fetcher);
}

export function useProjectItems(): AsyncState<ProjectItem[]> {
  const fetcher = useCallback(
    (signal: AbortSignal) =>
      api.getProjectItems(signal).then((res) => res.items),
    [],
  );
  return useFetch(fetcher);
}

export function useSprintMetrics(): AsyncState<SprintMetrics> {
  const fetcher = useCallback(
    (signal: AbortSignal) => api.getSprintMetrics(signal),
    [],
  );
  return useFetch(fetcher);
}

export function useRisks(): AsyncState<Risk[]> {
  const fetcher = useCallback(
    (signal: AbortSignal) =>
      api.getRisks(signal).then((res) => res.risks),
    [],
  );
  return useFetch(fetcher);
}

export function useTeamActivity(): AsyncState<TeamActivity> {
  const fetcher = useCallback(
    (signal: AbortSignal) => api.getTeamActivity(signal),
    [],
  );
  return useFetch(fetcher);
}

export function useRoadmap(): AsyncState<Roadmap> {
  const fetcher = useCallback(
    (signal: AbortSignal) => api.getRoadmap(signal),
    [],
  );
  return useFetch(fetcher);
}

/**
 * Fetches report detail for a given id. Refetches whenever id changes.
 * Pass null to skip fetching (returns idle state).
 * Aborts in-flight request when id changes or component unmounts.
 */
export function useReportDetail(id: string | null): AsyncState<ReportDetail> {
  const [state, setState] = useState<AsyncState<ReportDetail>>({
    data: null,
    loading: !!id,
    error: null,
  });

  useEffect(() => {
    if (!id) {
      setState({ data: null, loading: false, error: null });
      return;
    }

    const controller = new AbortController();
    setState({ data: null, loading: true, error: null });

    api
      .getReportDetail(id, controller.signal)
      .then((data) => {
        if (!controller.signal.aborted) {
          setState({ data, loading: false, error: null });
        }
      })
      .catch((err: unknown) => {
        if (isAbortError(err)) return;
        if (!controller.signal.aborted) {
          const error = err instanceof Error ? err : new Error(String(err));
          setState({ data: null, loading: false, error });
        }
      });

    return () => {
      controller.abort();
    };
  }, [id]);

  return state;
}