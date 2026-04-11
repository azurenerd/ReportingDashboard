namespace ReportingDashboard.Models;

public record DashboardData(
    ProjectInfo Project,
    TimelineConfig Timeline,
    List<MilestoneTrack> Tracks,
    HeatmapData Heatmap
);

public record ProjectInfo(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentMonth
);

public record TimelineConfig(
    List<string> Months,
    double NowPosition
);

public record MilestoneTrack(
    string Id,
    string Label,
    string Color,
    List<Milestone> Milestones
);

public record Milestone(
    string Date,
    string Type,
    double Position,
    string? Label
);

public record HeatmapData(
    List<string> Months,
    List<HeatmapCategory> Categories
);

public record HeatmapCategory(
    string Name,
    string CssClass,
    string Emoji,
    Dictionary<string, List<string>> Items
);