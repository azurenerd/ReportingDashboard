namespace ReportingDashboard.Services;

public class DashboardDataException : Exception
{
    public DashboardDataException(string message) : base(message) { }

    public DashboardDataException(string message, Exception innerException)
        : base(message, innerException) { }
}