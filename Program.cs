using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IDataProvider, DataProvider>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    var dataProvider = app.Services.GetRequiredService<IDataProvider>();
    var projectData = dataProvider.LoadProjectDataAsync().GetAwaiter().GetResult();
    logger.LogInformation("Application startup: Data loaded successfully");
}
catch (Exception ex)
{
    logger.LogError($"Application startup failed: {ex.Message}");
    logger.LogError($"Stack trace: {ex.StackTrace}");
    throw;
}

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

app.Run();