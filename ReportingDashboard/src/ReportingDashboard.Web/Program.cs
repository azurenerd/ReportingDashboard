using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

// Eagerly load dashboard data at startup
var dataService = app.Services.GetRequiredService<DashboardDataService>();
await dataService.LoadDataAsync();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();