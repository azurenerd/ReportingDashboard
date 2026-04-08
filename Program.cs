using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

var dataJsonPath = Path.Combine(AppContext.BaseDirectory, "data.json");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IDataCache, DataCache>();
builder.Services.AddScoped<IDataValidator, DataValidator>();

builder.Services.AddSingleton<IDataProvider>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DataProvider>>();
    var dataCache = sp.GetRequiredService<IDataCache>();
    return new DataProvider(logger, dataCache, dataJsonPath);
});

var app = builder.Build();

var dataProvider = app.Services.GetRequiredService<IDataProvider>();
try
{
    await dataProvider.LoadProjectDataAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Failed to load project data during startup");
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();