using ReportingDashboard.Components;
using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Parse --data argument
string dataFilePath = "data.json";
var dataArgIndex = Array.IndexOf(args, "--data");
if (dataArgIndex >= 0 && dataArgIndex + 1 < args.Length)
    dataFilePath = args[dataArgIndex + 1];

dataFilePath = Path.GetFullPath(dataFilePath);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(new DashboardDataServiceOptions { FilePath = dataFilePath });
builder.Services.AddSingleton<DashboardDataService>();

// Configure disconnected circuit retention to 1 hour
builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
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