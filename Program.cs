using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register data services
builder.Services.AddSingleton<IDataWatcherService, DataWatcherService>();
builder.Services.AddScoped<IDataLoaderService, DataLoaderService>();
builder.Services.AddSingleton<IDataValidator, DataValidator>();

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Configure the HTTP request pipeline
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