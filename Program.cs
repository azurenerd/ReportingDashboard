using AgentSquad.Runner.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAntiforgery();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (context.File.Name.EndsWith(".css") || context.File.Name.EndsWith(".js"))
        {
            context.Context.Response.Headers.CacheControl = "public, max-age=3600";
        }
    }
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .WithStaticAssets();

app.Run();