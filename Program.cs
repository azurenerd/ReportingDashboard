using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register ProjectDataService as Singleton
// Singleton is appropriate here because:
// - JSON data is static and doesn't change during application lifetime
// - No need to recreate the service or reload data on each request
// - Reduces memory allocation overhead
builder.Services.AddSingleton<ProjectDataService>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();