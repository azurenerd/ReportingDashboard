using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.Configure<DashboardOptions>(
    builder.Configuration.GetSection("Dashboard"));

builder.Services.AddSingleton<DashboardDataService>();
builder.Services.AddSingleton<IDashboardDataService>(sp => sp.GetRequiredService<DashboardDataService>());

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