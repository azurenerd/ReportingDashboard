namespace ReportingDashboard.Models;

public record HeatmapData(
    List<string> Columns,
    int HighlightColumnIndex,
    List<StatusRow> Rows
);