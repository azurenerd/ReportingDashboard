namespace ReportingDashboard.Models;

public record TimelineConfig(
    DateTime StartDate,
    DateTime EndDate,
    DateTime NowDate,
    List<Track> Tracks,
    List<Milestone> Milestones
);