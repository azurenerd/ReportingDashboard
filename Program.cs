using AgentSquad.Runner.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<IDataProvider>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var dataPath = Path.Combine(env.ContentRootPath, "data.json");
    return new JsonFileDataProvider(dataPath);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<AgentSquad.Runner.App>();

app.Run();