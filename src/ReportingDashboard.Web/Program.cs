using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Static SSR only — no .AddInteractiveServerComponents().
builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

builder.Logging.AddConsole();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();
app.MapGet("/healthz", () => "ok");

app.Run();