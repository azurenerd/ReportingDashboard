using ReportingDashboard.Components;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

// Load dashboard data before first request
var dataService = app.Services.GetRequiredService<DashboardDataService>();
await dataService.LoadAsync(Path.Combine(app.Environment.WebRootPath, "data.json"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();