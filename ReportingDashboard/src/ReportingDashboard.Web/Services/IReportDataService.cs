using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public interface IReportDataService
{
    Task<ProjectReport> GetReportAsync();
}