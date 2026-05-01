using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .Destructure.ByTransforming<object>(o =>
            {
                var type = o.GetType();
                var props = type.GetProperties();
                var dict = new Dictionary<string, object?>();
                foreach (var prop in props)
                {
                    // Never log PAT or secrets
                    if (prop.Name.Contains("Pat", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                        prop.Name.Contains("Token", StringComparison.OrdinalIgnoreCase))
                    {
                        dict[prop.Name] = "***REDACTED***";
                    }
                    else
                    {
                        dict[prop.Name] = prop.GetValue(o);
                    }
                }
                return dict;
            }));

    // Kestrel: bind to localhost only
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5000);
    });

    // Memory cache
    builder.Services.AddMemoryCache();

    // Swagger (Development only)
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    var app = builder.Build();

    // Swagger middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();

    // Serve static files from wwwroot (published Vite output)
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Health check / placeholder route
    app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

    // SPA fallback: serve index.html for non-API, non-file routes
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}