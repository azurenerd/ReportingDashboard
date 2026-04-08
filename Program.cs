using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IDataCache, MemoryCacheDataProvider>();
builder.Services.AddScoped<IDataProvider, DataProvider>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

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