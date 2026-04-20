var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();

// T3 owns DashboardDataService and its DI registration.
// T2 owns App.razor; until then the scaffold roots its routes at the Index placeholder.

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Web.Components.Pages.Index>();
app.MapGet("/healthz", () => Results.Text("ok", "text/plain"));

app.Run();