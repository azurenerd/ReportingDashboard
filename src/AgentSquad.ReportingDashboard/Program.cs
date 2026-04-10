using AgentSquad.ReportingDashboard.Models;
using AgentSquad.ReportingDashboard.Services;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register DataService as singleton
builder.Services.AddSingleton<DataService>();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Initialize DataService at startup before accepting requests
using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
    await dataService.InitializeAsync();
}

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();