using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    internal const string CacheKey = "dashboard:current";
    private const int MaxFileBytes = 10 * 1024 * 1024; // 10MB safety cap
    private const int ReadRetryCount = 3;
    private const int ReadRetryDelayMs = 50;

    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly IMemoryCache _cache;

    private readonly string _filePath;
    private readonly int _debounceMs;
    private readonly SemaphoreSlim _reloadLock = new(1, 1);

    private FileSystemWatcher? _watcher;
    private System.Timers.Timer? _debounceTimer;
    private bool _disposed;

    public event EventHandler? DataChanged;

    public DashboardDataService(
        IWebHostEnvironment env,
        IConfiguration config,
        IMemoryCache cache,
        ILogger<DashboardDataService> logger)
    {
        _env = env;
        _config = config;
        _cache = cache;
        _logger = logger;

        var relative = _config["Dashboard:DataFilePath"] ?? "wwwroot/data.json";
        if (relative.Contains(".."))
        {
            _logger.LogWarning("Dashboard:DataFilePath contains '..'; rejecting and using default.");
            relative = "wwwroot/data.json";
        }

        _filePath = Path.IsPathRooted(relative)
            ? relative
            : Path.GetFullPath(Path.Combine(_env.ContentRootPath, relative));

        if (!int.TryParse(_config["Dashboard:HotReloadDebounceMs"], out _debounceMs) || _debounceMs <= 0)
        {
            _debounceMs = 250;
        }

        // Initial synchronous load so the first request gets cached content.
        _cache.Set(CacheKey, LoadFromDisk());
        StartWatcher();
    }

    public DashboardLoadResult GetCurrent()
    {
        if (_cache.TryGetValue(CacheKey, out DashboardLoadResult? cached) && cached is not null)
        {
            return cached;
        }

        var result = LoadFromDisk();
        _cache.Set(CacheKey, result);
        return result;
    }

    private void StartWatcher()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            var file = Path.GetFileName(_filePath);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(file))
            {
                _logger.LogWarning("Cannot start FileSystemWatcher: invalid path {Path}", _filePath);
                return;
            }

            if (!Directory.Exists(dir))
            {
                _logger.LogWarning("Watch directory does not exist: {Dir}", dir);
                return;
            }

            _watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite
                             | NotifyFilters.CreationTime
                             | NotifyFilters.FileName
                             | NotifyFilters.Size,
                EnableRaisingEvents = true,
            };
            _watcher.Changed += OnFileEvent;
            _watcher.Created += OnFileEvent;
            _watcher.Renamed += OnFileEvent;

            _debounceTimer = new System.Timers.Timer(_debounceMs)
            {
                AutoReset = false,
            };
            _debounceTimer.Elapsed += (_, _) => ReloadAndCache();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start FileSystemWatcher for {Path}", _filePath);
        }
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        try
        {
            _debounceTimer?.Stop();
            _debounceTimer?.Start();
        }
        catch (ObjectDisposedException)
        {
            // Raced with Dispose - ignore.
        }
    }

    private void ReloadAndCache()
    {
        if (!_reloadLock.Wait(0))
        {
            // Another reload is already in flight; restart debounce to coalesce.
            try { _debounceTimer?.Start(); } catch (ObjectDisposedException) { }
            return;
        }
        try
        {
            var result = LoadFromDisk();
            _cache.Set(CacheKey, result);
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during data.json reload");
        }
        finally
        {
            _reloadLock.Release();
        }
    }

    private DashboardLoadResult LoadFromDisk()
    {
        var sw = Stopwatch.StartNew();

        if (!File.Exists(_filePath))
        {
            _logger.LogWarning("data.json not found at {Path}", _filePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: $"data.json not found at {_filePath}. See README for schema.",
                    Line: null,
                    Column: null,
                    Kind: "NotFound"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        byte[]? bytes = null;
        Exception? lastIoError = null;
        for (var attempt = 0; attempt < ReadRetryCount; attempt++)
        {
            try
            {
                using var fs = new FileStream(
                    _filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);

                if (fs.Length > MaxFileBytes)
                {
                    return new DashboardLoadResult(
                        Data: null,
                        Error: new DashboardLoadError(
                            FilePath: _filePath,
                            Message: $"data.json exceeds {MaxFileBytes} bytes.",
                            Line: null,
                            Column: null,
                            Kind: "ParseError"),
                        LoadedAt: DateTimeOffset.UtcNow);
                }

                bytes = new byte[fs.Length];
                var read = 0;
                while (read < bytes.Length)
                {
                    var n = fs.Read(bytes, read, bytes.Length - read);
                    if (n == 0) break;
                    read += n;
                }
                break;
            }
            catch (IOException ex)
            {
                lastIoError = ex;
                Thread.Sleep(ReadRetryDelayMs);
            }
        }

        if (bytes is null)
        {
            _logger.LogError(lastIoError, "Failed to read data.json after {Count} attempts", ReadRetryCount);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: $"Unable to read data.json: {lastIoError?.Message ?? "I/O error"}",
                    Line: null,
                    Column: null,
                    Kind: "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        DashboardData? data;
        try
        {
            data = JsonSerializer.Deserialize<DashboardData>(bytes, JsonOptions.Default);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse data.json");
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: ex.Message,
                    Line: ex.LineNumber is long l ? (int?)(l + 1) : null,
                    Column: ex.BytePositionInLine is long c ? (int?)(c + 1) : null,
                    Kind: "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        if (data is null)
        {
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: "data.json deserialized to null.",
                    Line: null,
                    Column: null,
                    Kind: "ParseError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        var errors = DashboardDataValidator.Validate(data);
        if (errors.Count > 0)
        {
            var msg = string.Join("; ", errors);
            _logger.LogWarning("data.json validation failed: {Errors}", msg);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: msg,
                    Line: null,
                    Column: null,
                    Kind: "ValidationError"),
                LoadedAt: DateTimeOffset.UtcNow);
        }

        sw.Stop();
        _logger.LogInformation(
            "Loaded data.json ({Size} bytes) in {Elapsed}ms",
            bytes.Length,
            sw.ElapsedMilliseconds);

        return new DashboardLoadResult(Data: data, Error: null, LoadedAt: DateTimeOffset.UtcNow);
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
                _watcher.Changed -= OnFileEvent;
                _watcher.Created -= OnFileEvent;
                _watcher.Renamed -= OnFileEvent;
                _watcher.Dispose();
            }
            _debounceTimer?.Dispose();
            _reloadLock.Dispose();
        }
        catch
        {
            // Best-effort disposal.
        }
    }
}
