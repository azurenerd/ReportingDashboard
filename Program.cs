using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDataCache, MemoryCacheAdapter>();
builder.Services.AddScoped<IDataProvider, DataProvider>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync("http://localhost:5000");