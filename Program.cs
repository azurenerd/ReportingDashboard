using AgentSquad.Core.Configuration;
using AgentSquad.Core.Messaging;
using AgentSquad.Core.GitHub;
using AgentSquad.Core.Persistence;
using AgentSquad.Orchestrator;
using AgentSquad.Agents;
using AgentSquad.Dashboard.Components;
using AgentSquad.Dashboard.Hubs;
using AgentSquad.Dashboard.Services;
using AgentSquad.Runner;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration
builder.Services.Configure<AgentSquadConfig>(
    builder.Configuration.GetSection("AgentSquad"));
builder.Services.Configure<LimitsConfig>(
    builder.Configuration.GetSection("AgentSquad:Limits"));

// Core services
builder.Services.AddInProcessMessageBus();
builder.Services.AddSingleton<AgentSquad.Core.AI.AgentUsageTracker>();
builder.Services.AddSingleton<AgentSquad.Core.Diagnostics.RequirementsCache>();
builder.Services.AddSingleton<AgentSquad.Core.Diagnostics.AgentChatService>();
builder.Services.AddSemanticKernelModels();
builder.Services.AddGitHubIntegration();

// Persistence — database scoped per repo to prevent cross-project contamination
var repoSlug = builder.Configuration["AgentSquad:Project:GitHubRepo"]?.Replace('/', '_') ?? "default";
var dbPath = $"agentsquad_{repoSlug}.db";
builder.Services.AddSingleton(new AgentStateStore(dbPath));
builder.Services.AddSingleton(new AgentMemoryStore(dbPath));
builder.Services.AddSingleton<ProjectFileManager>(sp =>
{
    var config = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentSquadConfig>>().Value;
    return new ProjectFileManager(
        sp.GetRequiredService<IGitHubService>(),
        sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ProjectFileManager>>(),
        config.Project.DefaultBranch);
});

// GitHub workflows
builder.Services.AddSingleton<PullRequestWorkflow>(sp =>
{
    var config = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AgentSquadConfig>>().Value;
    return new PullRequestWorkflow(
        sp.GetRequiredService<IGitHubService>(),
        sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PullRequestWorkflow>>(),
        config.Project.DefaultBranch);
});
builder.Services.AddSingleton<IssueWorkflow>();
builder.Services.AddSingleton<ConflictResolver>();

// Orchestrator (registry, health monitor, deadlock detector, spawn manager, workflow)
builder.Services.AddOrchestrator();

// Agent factory
builder.Services.AddSingleton<IAgentFactory, AgentFactory>();

// Dashboard: Blazor Server + SignalR
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddSingleton<DashboardDataService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DashboardDataService>());

// Worker service that starts the core agents and kicks off the workflow
builder.Services.AddHostedService<AgentSquadWorker>();

var app = builder.Build();

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Static file middleware configuration
// Serves files from wwwroot/ with appropriate cache headers
app.UseStaticFiles(new StaticFileOptions
{
    // Configure MIME types for proper browser rendering
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = new Dictionary<string, string>
        {
            // Standard MIME types
            { ".json", "application/json" },
            { ".js", "application/javascript" },
            { ".css", "text/css" },
            { ".html", "text/html; charset=utf-8" },
            { ".svg", "image/svg+xml" },
            { ".ico", "image/x-icon" },
            // Font MIME types
            { ".woff", "font/woff" },
            { ".woff2", "font/woff2" },
            { ".ttf", "font/ttf" },
            { ".otf", "font/otf" },
            { ".eot", "application/vnd.ms-fontobject" }
        }
    },
    OnPrepareResponse = ctx =>
    {
        // Cache policy: long-lived for static assets, short-lived for data files
        if (ctx.File.Name.EndsWith(".html"))
        {
            // HTML: short cache (1 hour) to detect updates quickly
            ctx.Context.Response.Headers.CacheControl = "public, max-age=3600";
        }
        else if (ctx.File.Name.EndsWith(".json") && ctx.File.Name.Contains("data.json"))
        {
            // Data: no cache (revalidate on every load)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        }
        else if (ctx.File.Name.EndsWith(".css") || ctx.File.Name.EndsWith(".js"))
        {
            // Static assets: long cache (30 days, safe for production use)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=2592000, immutable";
        }
        else if (ctx.File.Name.EndsWith(".woff2") || ctx.File.Name.EndsWith(".woff"))
        {
            // Fonts: very long cache (1 year)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }
        else
        {
            // Default: moderate cache (1 day)
            ctx.Context.Response.Headers.CacheControl = "public, max-age=86400";
        }

        // Disable MIME sniffing for security
        ctx.Context.Response.Headers.XContentTypeOptions = "nosniff";
        
        // Enable compression for text assets
        if (ctx.File.Name.EndsWith(".css") || ctx.File.Name.EndsWith(".js") || ctx.File.Name.EndsWith(".json"))
        {
            ctx.Context.Response.Headers.ContentEncoding = "gzip";
        }
    }
});

app.UseAntiforgery();

// SignalR hub for real-time dashboard updates
app.MapHub<AgentHub>("/agenthub");

// Blazor Server components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();