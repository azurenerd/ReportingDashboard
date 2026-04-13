namespace ReportingDashboard.Web.Models;

public class ProjectInfo
{
    public string ProjectName { get; set; } = string.Empty;

    public string ExecutiveSponsor { get; set; } = string.Empty;

    public string ReportingPeriod { get; set; } = string.Empty;

    public string LastUpdated { get; set; } = string.Empty;

    public string OverallStatus { get; set; } = "OnTrack";

    public string Summary { get; set; } = string.Empty;
}