namespace ReportingDashboard.Models;

public record DashboardData(
    ProjectHeader Header,
    TimelineConfig Timeline,
    HeatmapData Heatmap
);