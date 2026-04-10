using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// Register ProjectDataService as Singleton
builder.Services.AddSingleton<IProjectDataService, ProjectDataService>();

// Configure Blazor Server options
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.JSInteropDefaultAsyncTimeout = TimeSpan.FromSeconds(60);
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files with cache headers for Bootstrap and Icons
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        var path = context.File.PhysicalPath;
        if (path != null && (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                             path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase) ||
                             path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
                             path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)))
        {
            context.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
    }
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Initialize ProjectDataService before starting application
var dataService = app.Services.GetRequiredService<IProjectDataService>();
try
{
    await dataService.InitializeAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to initialize ProjectDataService during startup");
    // Continue running; error will be displayed in DashboardPage error alert
}

app.Run();