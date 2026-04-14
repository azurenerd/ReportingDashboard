using ReportingDashboard.Components;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

var dataService = app.Services.GetRequiredService<DashboardDataService>();
try
{
    dataService.Initialize();
}
catch (DashboardDataException ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError("Dashboard startup failed: {Message}", ex.Message);
    logger.LogError("Please check your data.json file and try again.");
    return;
}

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();