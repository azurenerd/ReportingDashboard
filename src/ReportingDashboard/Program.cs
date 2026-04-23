using ReportingDashboard.Components;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<DashboardDataService>();

var app = builder.Build();

// Eagerly initialize the data service so FileSystemWatcher starts immediately
app.Services.GetRequiredService<DashboardDataService>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
