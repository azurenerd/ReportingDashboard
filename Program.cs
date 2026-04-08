using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server registration
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// TODO: Register IDataProvider and IDataCache services
// Single authoritative data source: wwwroot/data.json
// Implementation deferred to separate PR implementing DataProvider service layer
// builder.Services.AddSingleton<IDataProvider, DataProvider>();
// builder.Services.AddSingleton<IDataCache, DataCache>();

var app = builder.Build();

// Configure static file serving with MIME type mappings and cache headers
var provider = new FileExtensionContentTypeProvider();

// Add MIME type mappings
provider.Mappings[".json"] = "application/json";
provider.Mappings[".js"] = "application/javascript";
provider.Mappings[".css"] = "text/css";
provider.Mappings[".html"] = "text/html; charset=utf-8";
provider.Mappings[".svg"] = "image/svg+xml";
provider.Mappings[".ico"] = "image/x-icon";
provider.Mappings[".woff"] = "font/woff";
provider.Mappings[".woff2"] = "font/woff2";
provider.Mappings[".ttf"] = "font/ttf";
provider.Mappings[".otf"] = "font/otf";
provider.Mappings[".eot"] = "application/vnd.ms-fontobject";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx =>
    {
        // Version-based cache strategy: assets with ?v= parameter cached long-term
        var hasVersionParameter = ctx.Context.Request.QueryString.HasValue &&
                                  ctx.Context.Request.QueryString.Value.Contains("v=");

        if (ctx.File.Name == "data.json")
        {
            // Data: never cache (always fresh on page load)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        }
        else if (hasVersionParameter)
        {
            // Versioned assets: long cache (1 year, immutable)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
        else
        {
            // Unversioned HTML: moderate cache (1 hour, check for updates)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=3600";
        }

        // Disable MIME sniffing for security
        ctx.Context.Response.Headers.XContentTypeOptions = "nosniff";
    }
});

// Antiforgery
app.UseAntiforgery();

// Blazor Server routing
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();