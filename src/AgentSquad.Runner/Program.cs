using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register singletons (app-lifetime)
builder.Services.AddSingleton<DataWatcherService>();
builder.Services.AddSingleton<IDataWatcherService>(sp => sp.GetRequiredService<DataWatcherService>());

// Register scoped services (per HTTP request)
builder.Services.AddScoped<DataLoaderService>();
builder.Services.AddScoped<IDataLoaderService>(sp => sp.GetRequiredService<DataLoaderService>());

// Add logging
builder.Services.AddLogging();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();