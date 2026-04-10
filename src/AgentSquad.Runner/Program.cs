using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

// Register DashboardDataService as singleton
builder.Services.AddSingleton<DashboardDataService>();

// Configure DashboardOptions from appsettings.json
builder.Services.Configure<DashboardOptions>(
    builder.Configuration.GetSection("Dashboard"));

// Configure logging
builder.Logging.AddConsole();

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