using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.MapGet("/healthz", () => Results.Text("ok", "text/plain"));

app.Run();

public partial class Program { }