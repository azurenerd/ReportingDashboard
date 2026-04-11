using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

// Load data before serving requests
var dataService = app.Services.GetRequiredService<DashboardDataService>();
await dataService.LoadAsync(Path.Combine(app.Environment.WebRootPath, "data.json"));

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();