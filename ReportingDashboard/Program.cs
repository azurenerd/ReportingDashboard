using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

var app = builder.Build();

// Force eager initialization so startup errors surface immediately
_ = app.Services.GetRequiredService<IDashboardDataService>();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();