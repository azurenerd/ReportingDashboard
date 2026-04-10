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
/// Temporary stub implementation replaced by DashboardDataService in T2.
/// </summary>
internal sealed class PlaceholderDataService : IDashboardDataService
{
    public DashboardData? Data => null;
    public string? LoadError => null;
    public bool IsLoaded => false;
    public event Action? OnDataChanged;

    public Task LoadAsync() => Task.CompletedTask;

    public void Dispose()
    {
        // Suppress unused event warning
        _ = OnDataChanged;
    }
}