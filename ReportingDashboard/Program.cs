using ReportingDashboard.Components;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

var dataService = app.Services.GetRequiredService<DashboardDataService>();
await dataService.LoadAsync();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();