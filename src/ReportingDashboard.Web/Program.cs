using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Kestrel endpoint is also configurable via appsettings.json "Kestrel:Endpoints:Http:Url".
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Web.Components.App>();
app.MapGet("/healthz", () => Results.Text("ok", "text/plain"));

app.Run();