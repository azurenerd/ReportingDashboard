using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server registration
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// TODO: Register IDataProvider and IDataCache services
// These will load project data from wwwroot/data.json
// Placeholder for implementation by data layer team
// builder.Services.AddSingleton<IDataProvider, DataProvider>();
// builder.Services.AddSingleton<IDataCache, DataCache>();

var app = builder.Build();

// Static file serving with cache versioning strategy
// Files are cached based on version query parameter, not time-based max-age
// Example: /css/site.css?v=1.0.0 can be cached long-term
// data.json should not be cached (serve fresh on each request)
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider(),
    OnPrepareResponse = ctx =>
    {
        // Check for version query parameter (e.g., ?v=1.0.0)
        var hasVersionParameter = ctx.Context.Request.QueryString.HasValue &&
                                  ctx.Context.Request.QueryString.Value.Contains("v=");

        if (ctx.File.Name == "data.json")
        {
            // Data files: never cache (always fresh)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        }
        else if (hasVersionParameter)
        {
            // Versioned assets (e.g., site.css?v=1.0.0): cache 1 year (immutable)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
        else
        {
            // Unversioned HTML: cache 1 hour (check for updates)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=3600";
        }

        // Disable MIME sniffing
        ctx.Context.Response.Headers.XContentTypeOptions = "nosniff";
    }
});

// Antiforgery for form submissions
app.UseAntiforgery();

// Blazor Server routing
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();