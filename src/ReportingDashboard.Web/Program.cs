using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddSingleton<IDashboardDataService, StubDashboardDataService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>();
app.Run();