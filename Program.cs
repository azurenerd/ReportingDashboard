using AgentSquad.Runner.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register ProjectDataService as Singleton
// 
// Rationale for Singleton Lifetime:
// - ProjectDataService is stateless: It only reads from the file system and deserializes JSON.
// - Thread-safe by design: No mutable state is maintained across requests/circuits.
// - File-based operation: Loading data.json multiple times per circuit is inefficient; a single
//   instance can safely serve all components across all Blazor circuits.
// - Consistent data access: Singleton ensures all components receive the same data instance
//   during their lifecycle, improving performance and memory efficiency.
// 
// Note: If service design changes to maintain state or become request-specific, consider changing
// to Scoped or Transient. For now, Singleton is optimal for this read-only file-based service.
builder.Services.AddSingleton<ProjectDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();