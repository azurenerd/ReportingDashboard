var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ReportingDashboard.Components.App>()
    .AddInteractiveServerRenderMode();

app.Urls.Add("http://localhost:5000");
app.Run();