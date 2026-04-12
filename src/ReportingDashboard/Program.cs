using ReportingDashboard.Components;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddRazorComponents();
builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

var dataService = app.Services.GetRequiredService<DashboardDataService>();
await dataService.LoadAsync();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();