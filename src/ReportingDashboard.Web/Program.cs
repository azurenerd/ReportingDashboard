using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:5000");

builder.Configuration.AddJsonFile("data.json", optional: false, reloadOnChange: true);

builder.Services.Configure<JsonSerializerOptions>(o =>
{
    o.PropertyNameCaseInsensitive = true;
    o.Converters.Add(new DateOnlyJsonConverter());
    o.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.Services.Configure<DashboardData>(builder.Configuration);
builder.Services.AddSingleton<DashboardDataProvider>();

builder.Services.AddRazorComponents();

var app = builder.Build();

app.UseExceptionHandler("/error", createScopeForErrors: true);
app.UseStatusCodePagesWithReExecute("/error", "?code={0}");

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();