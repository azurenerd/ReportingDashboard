using AgentSquad.Runner.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register application services with appropriate lifetimes
// Scoped: One instance per HTTP request (best for data access with state)
builder.Services.AddScoped<DashboardDataService>();

// Singleton: One instance for entire application (stateless utility services)
builder.Services.AddSingleton<IDateCalculationService, DateCalculationService>();
builder.Services.AddSingleton<IVisualizationService, VisualizationService>();

// Configure logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    
    var logLevel = builder.Environment.IsDevelopment() 
        ? LogLevel.Debug 
        : LogLevel.Warning;
    
    config.SetMinimumLevel(logLevel);
});

var app = builder.Build();

// Configure error handling middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<AgentSquad.Runner.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();