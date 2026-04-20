using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

/// <summary>
/// Stub implementation - fleshed out by a downstream task (T3/T4) to add
/// JSON parsing, validation, FileSystemWatcher-based hot-reload, and caching.
/// For now it returns a minimal valid DashboardLoadResult so the app boots.
/// </summary>
public sealed class DashboardDataService : IDashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IMemoryCache _cache;

    public DashboardDataService(
        IWebHostEnvironment env,
        ILogger<DashboardDataService> logger,
        IMemoryCache cache)
    {
        _env = env;
        _logger = logger;
        _cache = cache;
    }

    public event EventHandler? DataChanged;

    public DashboardLoadResult GetCurrent()
    {
        // Placeholder: returns an empty/error result so the page renders
        // its placeholder sections. Real implementation arrives in a later task.
        _ = _env;
        _ = _cache;
        _logger.LogDebug("DashboardDataService stub returning placeholder result.");

        var error = new DashboardLoadError(
            FilePath: "wwwroot/data.json",
            Message: "Data service not yet implemented (T1 scaffolding).",
            Line: null,
            Column: null,
            Kind: "NotImplemented");

        return new DashboardLoadResult(Data: null, Error: error, LoadedAt: DateTimeOffset.UtcNow);
    }

    private void OnDataChanged() => DataChanged?.Invoke(this, EventArgs.Empty);

    // Referenced to silence unused-member analyzer on the private helper above.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    internal JsonSerializerOptions JsonOptions => _jsonOptions;

    internal void RaiseDataChangedForTests() => OnDataChanged();
}
