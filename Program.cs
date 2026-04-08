using AgentSquad.Runner.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IDataCache, DataCache>();
builder.Services.AddScoped<IDataValidator, DataValidator>();
builder.Services.AddScoped<IDataProvider, DataProvider>();
builder.Services.AddScoped<IErrorHandler, ErrorLogger>();

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

app.Run();