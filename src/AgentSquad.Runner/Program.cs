using AgentSquad.Runner.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register caching service
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDataCache, MemoryCacheAdapter>();

// Register data provider as scoped (new instance per request/circuit)
builder.Services.AddScoped<IDataProvider, DataProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline
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

app.Run();