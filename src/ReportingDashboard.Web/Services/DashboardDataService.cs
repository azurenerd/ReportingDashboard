using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    private const string CacheKey = "dashboard:current";
    private const int DebounceMs = 250;
    private const int MaxReadRetries = 3;
    private const int ReadRetryDelayMs = 50;

    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly string _dataFilePath;
    private readonly SemaphoreSlim _reloadLock = new(1, 1);
    private readonly System.Timers.Timer _debounceTimer;
    private readonly FileSystemWatcher? _watcher;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public event EventHandler? DataChanged;

    public DashboardDataService(
        IMemoryCache cache,
        IWebHostEnvironment env,
        IConfiguration configuration,
        ILogger<DashboardDataService> logger)
    {
        _cache = cache;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        var configuredPath = configuration["Dashboard:DataFilePath"] ?? "wwwroot/data.json";
        _dataFilePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(env.ContentRootPath, configuredPath));

        _logger.LogInformation("DashboardDataService starting. DataFilePath: {Path}", _dataFilePath);

        _debounceTimer = new System.Timers.Timer(DebounceMs) { AutoReset = false };
        _debounceTimer.Elapsed += (_, _) => _ = ReloadAsync();

        LoadAndCache();

        try
        {
            var dir = Path.GetDirectoryName(_dataFilePath);
            var file = Path.GetFileName(_dataFilePath);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                _watcher = new FileSystemWatcher(dir, file)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileChanged;
                _watcher.Renamed += OnFileChanged;
            }
            else
            {
                _logger.LogWarning("Directory for data file does not exist; hot-reload disabled: {Dir}", dir);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start FileSystemWatcher for {Path}. Hot-reload disabled.", _dataFilePath);
        }
    }

    public DashboardLoadResult GetCurrent()
    {
        if (_cache.TryGetValue(CacheKey, out DashboardLoadResult? result) && result is not null)
        {
            return result;
        }

        return LoadAndCache();
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private async Task ReloadAsync()
    {
        await _reloadLock.WaitAsync().ConfigureAwait(false);
        try
        {
            LoadAndCache();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during data.json reload.");
        }
        finally
        {
            _reloadLock.Release();
        }
    }

    private DashboardLoadResult LoadAndCache()
    {
        var result = Load();
        _cache.Set(CacheKey, result);
        return result;
    }

    private DashboardLoadResult Load()
    {
        var sw = Stopwatch.StartNew();

        if (!File.Exists(_dataFilePath))
        {
            _logger.LogError("data.json not found at {Path}", _dataFilePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _dataFilePath,
                    Message: $"data.json not found at {_dataFilePath}. See README for schema.",
                    Line: null,
                    Column: null,
                    Kind: "NotFound"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        string? json = null;
        Exception? lastIoError = null;
        for (int attempt = 0; attempt < MaxReadRetries; attempt++)
        {
            try
            {
                using var stream = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                json = reader.ReadToEnd();
                lastIoError = null;
                break;
            }
            catch (IOException ex)
            {
                lastIoError = ex;
                if (attempt < MaxReadRetries - 1)
                {
                    Thread.Sleep(ReadRetryDelayMs);
                }
            }
        }

        if (json is null)
        {
            _logger.LogError(lastIoError, "Failed to read data.json after {Attempts} attempts: {Path}", MaxReadRetries, _dataFilePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _dataFilePath,
                    Message: $"Failed to read data.json: {lastIoError?.Message ?? "unknown IO error"}",
                    Line: null,
                    Column: null,
                    Kind: "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        DashboardData? data;
        try
        {
            data = JsonSerializer.Deserialize<DashboardData>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json at {Path}", _dataFilePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _dataFilePath,
                    Message: ex.Message,
                    Line: ex.LineNumber.HasValue ? (int)ex.LineNumber.Value + 1 : null,
                    Column: ex.BytePositionInLine.HasValue ? (int)ex.BytePositionInLine.Value + 1 : null,
                    Kind: "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deserializing data.json at {Path}", _dataFilePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _dataFilePath,
                    Message: ex.Message,
                    Line: null,
                    Column: null,
                    Kind: "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        if (data is null)
        {
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _dataFilePath,
                    Message: "data.json deserialized to null.",
                    Line: null,
                    Column: null,
                    Kind: "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        var validationErrors = DashboardDataValidator.Validate(data);
        if (validationErrors.Count > 0)
        {
            foreach (var err in validationErrors)
            {
                _logger.LogWarning("data.json validation warning: {Error}", err);
            }

            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _dataFilePath,
                    Message: string.Join("; ", validationErrors),
                    Line: null,
                    Column: null,
                    Kind: "ValidationError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        sw.Stop();
        _logger.LogInformation("Loaded data.json ({Bytes} bytes) in {ElapsedMs}ms", json.Length, sw.ElapsedMilliseconds);

        return new DashboardLoadResult(
            Data: data,
            Error: null,
            LoadedAt: DateTimeOffset.UtcNow);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_watcher is not null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnFileChanged;
                _watcher.Created -= OnFileChanged;
                _watcher.Renamed -= OnFileChanged;
                _watcher.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing FileSystemWatcher.");
        }

        _debounceTimer.Stop();
        _debounceTimer.Dispose();
        _reloadLock.Dispose();
    }
}