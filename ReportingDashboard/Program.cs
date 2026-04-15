using ReportingDashboard.Components;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

string dataFilePath = "data.json";
var dataArgIndex = Array.IndexOf(args, "--data");
if (dataArgIndex >= 0 && dataArgIndex + 1 < args.Length)
    dataFilePath = args[dataArgIndex + 1];

dataFilePath = Path.GetFullPath(dataFilePath);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(new DashboardDataServiceOptions { FilePath = dataFilePath });
builder.Services.AddSingleton<DashboardDataService>();

builder.Services.AddOptions<Microsoft.AspNetCore.Components.Server.CircuitOptions>()
    .Configure(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(1);
    });

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

var dataService = app.Services.GetRequiredService<DashboardDataService>();
dataService.Initialize();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();