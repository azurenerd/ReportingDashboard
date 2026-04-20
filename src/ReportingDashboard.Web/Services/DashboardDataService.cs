using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

/// <summary>
/// Singleton data service. T1 ships a minimal working loader so DI composes and the
/// scaffold boots with real, parsed data. T3 replaces this with FileSystemWatcher-based
/// hot reload, validation, and richer error reporting.
/// </summary>
public sealed class DashboardDataService : IDashboardDataService
{
    private const string CacheKey = "dashboard:current";

    private readonly IMemoryCache _cache;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public event EventHandler? DataChanged;

    public DashboardDataService(
        IMemoryCache cache,
        IWebHostEnvironment env,
        IConfiguration config,
        ILogger<DashboardDataService> logger)
    {
        _cache = cache;
        _env = env;
        _config = config;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    public DashboardLoadResult GetCurrent()
    {
        if (_cache.TryGetValue(CacheKey, out DashboardLoadResult? cached) && cached is not null)
        {
            return cached;
        }

        var result = Load();
        _cache.Set(CacheKey, result);
        return result;
    }

    private DashboardLoadResult Load()
    {
        var filePath = ResolveFilePath();
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("data.json not found at {Path}", filePath);
                return new DashboardLoadResult(
                    Data: null,
                    Error: new DashboardLoadError(filePath, "data.json not found.", null, null, "NotFound"),
                    LoadedAt: DateTimeOffset.UtcNow);
            }

            using var stream = File.Open(filePath, new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.ReadWrite
            });
            var data = JsonSerializer.Deserialize<DashboardData>(stream, _jsonOptions);
            if (data is null)
            {
                return new DashboardLoadResult(
                    Data: null,
                    Error: new DashboardLoadError(filePath, "data.json deserialized to null.", null, null, "ParseError"),
                    LoadedAt: DateTimeOffset.UtcNow);
            }

            _logger.LogInformation("Loaded data.json from {Path}", filePath);
            return new DashboardLoadResult(data, Error: null, DateTimeOffset.UtcNow);
        }
        catch (JsonException jex)
        {
            _logger.LogWarning(jex, "Failed to parse data.json at {Path}", filePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    filePath,
                    jex.Message,
                    jex.LineNumber.HasValue ? (int)jex.LineNumber.Value + 1 : null,
                    jex.BytePositionInLine.HasValue ? (int)jex.BytePositionInLine.Value + 1 : null,
                    "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading data.json at {Path}", filePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(filePath, ex.Message, null, null, "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }
    }

    private string ResolveFilePath()
    {
        var configured = _config["Dashboard:DataFilePath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(_env.ContentRootPath, configured);
        }

        var root = _env.WebRootPath;
        if (string.IsNullOrEmpty(root))
        {
            root = Path.Combine(_env.ContentRootPath, "wwwroot");
        }
        return Path.Combine(root, "data.json");
    }

    // Keep compiler from warning about unused event; downstream tasks will raise it.
    private void RaiseChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
}