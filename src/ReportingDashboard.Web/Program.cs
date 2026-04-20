using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5080");

builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.MapGet("/healthz", () => "ok");

app.Run();