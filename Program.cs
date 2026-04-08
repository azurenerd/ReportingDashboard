using AgentSquad.Runner.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

var dataJsonPath = Path.Combine(AppContext.BaseDirectory, "data.json");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IDataValidator, DataValidator>();

builder.Services.AddSingleton<IDataProvider>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DataProvider>>();
    return new DataProvider(logger, dataJsonPath);
});

var app = builder.Build();

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