using AgentSquad.ReportingDashboard.Services;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Register DataService as singleton
builder.Services.AddSingleton<DataService>();

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

// Initialize DataService at startup before accepting requests
using (var scope = app.Services.CreateScope())
{
	var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
	await dataService.InitializeAsync();
}

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run("http://localhost:5000");