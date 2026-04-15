using ReportingDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Parse --data CLI argument
string dataFilePath = "data.json";
var dataArgIndex = Array.IndexOf(args, "--data");
if (dataArgIndex >= 0 && dataArgIndex + 1 < args.Length)
    dataFilePath = args[dataArgIndex + 1];

// Resolve to absolute path relative to content root
if (!Path.IsPathRooted(dataFilePath))
    dataFilePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, dataFilePath));
else
    dataFilePath = Path.GetFullPath(dataFilePath);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton(new DashboardDataServiceOptions { FilePath = dataFilePath });
builder.Services.AddSingleton<DashboardDataService>();

builder.Services.AddServerSideBlazor().AddCircuitOptions(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromHours(1);
});

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

// Trigger initial data load
var dataService = app.Services.GetRequiredService<DashboardDataService>();
dataService.Initialize();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();