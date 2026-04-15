namespace ReportingDashboard.Models;

public record ProjectHeader(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentMonth
);