using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Configure Kestrel for localhost:5000 only
builder.WebHost.UseUrls("http://127.0.0.1:5000");

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register application services
// Note: Concrete implementations (DataWatcherService, DataValidator, DataLoaderService) 
// are placeholder stubs that will be implemented in T2, T3, T4.
// They are registered but not instantiated until Index.razor calls them.
builder.Services.AddSingleton<IDataWatcherService, DataWatcherService>();
builder.Services.AddSingleton<IDataValidator, DataValidator>();
builder.Services.AddScoped<IDataLoaderService, DataLoaderService>();

// Add logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<AgentSquad.Runner.App>()
    .AddInteractiveServerRenderMode();

// Start the application
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

// Auto-launch browser on startup
lifetime.ApplicationStarted.Register(async () =>
{
    var config = app.Services.GetRequiredService<IConfiguration>();
    var port = config.GetValue<int>("AppSettings:Port") ?? 5000;
    var url = $"http://127.0.0.1:{port}";
    
    try
    {
        // Open default browser
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
    }
    catch
    {
        // Silently fail if browser cannot be launched (e.g., in headless environment)
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Failed to auto-launch browser. Navigate to {Url} manually.", url);
    }
});

app.Run();