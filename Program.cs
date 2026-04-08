using AgentSquad.Runner.Components;
using AgentSquad.Runner.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLogging(logging =>
{
    if (builder.Environment.IsDevelopment())
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    }
    else
    {
        logging.AddEventSourceLogger();
        logging.SetMinimumLevel(LogLevel.Information);
    }
});

builder.Services.AddSingleton<ProjectDataService>();

builder.Services.AddCircuitOptions(options =>
{
    options.DisconnectedCircuitMaxRetained = 100;
    if (!builder.Environment.IsDevelopment())
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    }
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();