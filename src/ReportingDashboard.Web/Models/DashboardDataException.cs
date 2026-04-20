namespace ReportingDashboard.Web.Models;

public sealed class DashboardDataException : Exception
{
    public DashboardDataException(string message) : base(message) { }
    public DashboardDataException(string message, Exception inner) : base(message, inner) { }
}