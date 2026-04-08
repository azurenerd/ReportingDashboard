namespace AgentSquad.Runner.Models;

public enum ErrorContext
{
    FileNotFound,
    InvalidJson,
    ValidationFailed,
    NullProject,
    MissingMilestones,
    NullMetrics,
    NullWorkItems,
    InvalidCompletionPercentage,
    InvalidHealthStatus,
    UnknownError
}