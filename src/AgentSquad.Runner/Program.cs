using MudBlazor.Services;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudBlazorSnackbar();
builder.Services.AddMudBlazorDialog();
builder.Services.AddMudBlazorServices();

// Register DashboardDataService as singleton (loads data.json once, shared across all requests)
builder.Services.AddSingleton<DashboardDataService>();

// Bind DashboardOptions from appsettings.json
builder.Services.Configure<DashboardOptions>(
    builder.Configuration.GetSection("Dashboard"));

// Logging configuration
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();