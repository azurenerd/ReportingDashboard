using ReportingDashboard.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5000));

builder.Services.AddRazorComponents();
builder.Services.AddScoped<DashboardDataService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<ReportingDashboard.Components.App>();

app.Run();