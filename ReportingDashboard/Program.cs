using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Parse --data CLI argument
string dataFilePath = "data.json";
var dataArgIndex = Array.IndexOf(args, "--data");
if (dataArgIndex >= 0 && dataArgIndex + 1 < args.Length)
{
    dataFilePath = args[dataArgIndex + 1];
}

// Resolve to absolute path
dataFilePath = Path.GetFullPath(dataFilePath);

// Register Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register the data service as singleton
builder.Services.AddSingleton(new DashboardDataServiceOptions { FilePath = dataFilePath });
builder.Services.AddSingleton<DashboardDataService>();

// Extend SignalR circuit retention for long-lived dashboard sessions
builder.Services.AddServerSideBlazor(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(1);
});

// Bind to localhost only
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

// Trigger initial data load (validates file exists and is valid JSON)
var dataService = app.Services.GetRequiredService<DashboardDataService>();
dataService.Initialize();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();