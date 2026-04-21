using ReportingDashboard.Components;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddRazorComponents();

builder.Services.Configure<DashboardOptions>(
    builder.Configuration.GetSection("Dashboard"));
builder.Services.AddSingleton<IDataService, JsonFileDataService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();