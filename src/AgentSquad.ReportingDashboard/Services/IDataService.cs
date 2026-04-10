namespace AgentSquad.ReportingDashboard.Services;

using AgentSquad.ReportingDashboard.Models;

public interface IDataService
{
	DashboardData CurrentData { get; }
	
	event Func<Task> OnDataChanged;
	
	Task InitializeAsync();
	Task<DashboardData> ReloadDataAsync();
}