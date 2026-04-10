using ReportingDashboard.Components;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IDashboardDataService, PlaceholderDataService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

var app = builder.Build();

var dataService = app.Services.GetRequiredService<IDashboardDataService>();
await dataService.LoadAsync();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>
/// Temporary placeholder implementation of IDashboardDataService.
/// Replaced by the real DashboardDataService in task T2.
/// </summary>
internal sealed class PlaceholderDataService : IDashboardDataService
{
    public DashboardData? Data => null;

    public string? LoadError => "Dashboard data not loaded. Place a valid data.json at the configured DashboardDataPath and implement DashboardDataService.";

    public bool IsLoaded => false;

#pragma warning disable CS0067 // Event is never invoked in placeholder
    public event Action? OnDataChanged;
#pragma warning restore CS0067

    public Task LoadAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}