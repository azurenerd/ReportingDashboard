namespace ReportingDashboard.Models;

public record Milestone(
    string TrackId,
    DateTime Date,
    string Label,
    string Type,
    string? Description
);