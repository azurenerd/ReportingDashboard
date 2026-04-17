using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.ListenLocalhost(5000));

builder.Services.AddRazorComponents();
builder.Services.AddScoped<DashboardDataService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

app.Run();