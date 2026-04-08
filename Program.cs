using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure static file options
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".json"] = "application/json";

var staticFileOptions = new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path.Value;

        if (path != null)
        {
            // Short cache for dynamic data files
            if (path.StartsWith("/data/", StringComparison.OrdinalIgnoreCase) || 
                path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Context.Response.Headers.CacheControl = "public, max-age=300, must-revalidate";
                ctx.Context.Response.Headers.ETag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue("\"" + ctx.File.LastModified.Ticks + "\"");
            }
            // Long cache for immutable assets
            else if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".eot", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            }
            // Default cache for other files
            else
            {
                ctx.Context.Response.Headers.CacheControl = "public, max-age=3600";
            }
        }
    }
};

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files with configured cache headers
app.UseStaticFiles(staticFileOptions);

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();