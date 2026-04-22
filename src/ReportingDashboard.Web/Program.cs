using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.Configure<DashboardOptions>(
    builder.Configuration.GetSection("Dashboard"));
builder.Services.AddSingleton<IDataService, JsonFileDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();
