using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// IDataProvider and IDataCache implementations to be registered by dedicated data services PR
builder.Services.AddSingleton(typeof(IDataProvider), _ => throw new NotImplementedException("IDataProvider implementation not registered. Register in data services PR."));
builder.Services.AddSingleton(typeof(IDataCache), _ => throw new NotImplementedException("IDataCache implementation not registered. Register in data services PR."));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

var provider = new FileExtensionContentTypeProvider();

provider.Mappings[".woff"] = "font/woff";
provider.Mappings[".woff2"] = "font/woff2";
provider.Mappings[".ttf"] = "font/ttf";
provider.Mappings[".otf"] = "font/otf";
provider.Mappings[".eot"] = "application/vnd.ms-fontobject";
provider.Mappings[".svg"] = "image/svg+xml";
provider.Mappings[".json"] = "application/json";
provider.Mappings[".js"] = "application/javascript";

var staticFileOptions = new StaticFileOptions
{
    ContentTypeProvider = provider,
    OnPrepareResponse = context =>
    {
        // data.json must never cache to ensure fresh data on each page load
        if (Path.GetFileName(context.File.PhysicalPath) == "data.json")
        {
            context.Context.Response.Headers.Append("Cache-Control", "max-age=0, must-revalidate");
        }
        else
        {
            // All other static assets cached for 1 day
            context.Context.Response.Headers.Append("Cache-Control", "public, max-age=86400");
        }
    }
};

app.UseRouting();
app.UseStaticFiles(staticFileOptions);

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();