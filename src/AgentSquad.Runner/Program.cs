using AgentSquad.Runner.Services;
using AgentSquad.Runner;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DisconnectedCircuitMaxRetained = 100;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    });

builder.Services.AddScoped<IProjectDataService, ProjectDataService>();

var app = builder.Build();

var projectDataService = app.Services.GetRequiredService<IProjectDataService>();
try
{
    await projectDataService.InitializeAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to initialize project data service.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();