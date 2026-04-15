namespace ReportingDashboard.Models;

public record StatusRow(
    string Category,
    string ColorTheme,
    List<MonthCell> Cells
);