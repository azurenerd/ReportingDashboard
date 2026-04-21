using ReportingDashboard.Components;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(
    $"http://localhost:{builder.Configuration.GetValue<int>("Port", 5000)}");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();