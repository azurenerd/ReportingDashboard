namespace ReportingDashboard.Models;

public record DashboardData(
    ProjectInfo Project,
    List<Milestone> Milestones,
    List<WorkItem> Shipped,
    List<InProgressItem> InProgress,
    List<CarriedOverItem> CarriedOver,
    List<KeyMetric> Metrics
);

public record ProjectInfo(
    string Name,
    string Executive,
    string Status,
    string ReportDate,
    string ReportingPeriod
);

public record Milestone(
    string Title,
    string Date,
    string Status,
    string Description
);

public record WorkItem(
    string Title,
    string Description,
    string Category,
    string Priority
);

public record InProgressItem(
    string Title,
    string Description,
    string Category,
    string Priority,
    int PercentComplete
);

public record CarriedOverItem(
    string Title,
    string Description,
    string Category,
    string Priority,
    string OriginalTarget,
    string Reason
);

public record KeyMetric(
    string Label,
    string Value,
    string Trend
);