using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    internal const string CacheKey = "dashboard:current";
    internal const int DebounceMs = 250;
    internal const int ReadRetryCount = 3;
    internal const int ReadRetryDelayMs = 50;

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly FileSystemWatcher? _watcher;
    private readonly System.Timers.Timer _debounceTimer;
    private bool _disposed;

    public event EventHandler? DataChanged;

    public DashboardDataService(
        IWebHostEnvironment env,
        IMemoryCache cache,
        ILogger<DashboardDataService> logger)
        : this(ResolveDataFilePath(env), cache, logger)
    {
    }

    internal DashboardDataService(
        string filePath,
        IMemoryCache cache,
        ILogger<DashboardDataService> logger)
    {
        _cache = cache;
        _logger = logger;
        _filePath = filePath;

        _debounceTimer = new System.Timers.Timer(DebounceMs)
        {
            AutoReset = false,
        };
        _debounceTimer.Elapsed += (_, _) => Reload();

        Reload();

        var directory = Path.GetDirectoryName(_filePath);
        var fileName = Path.GetFileName(_filePath);
        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        {
            try
            {
                _watcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite
                                   | NotifyFilters.Size
                                   | NotifyFilters.CreationTime
                                   | NotifyFilters.FileName,
                    EnableRaisingEvents = true,
                };
                _watcher.Changed += OnFileEvent;
                _watcher.Created += OnFileEvent;
                _watcher.Renamed += OnFileEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start FileSystemWatcher for {Path}", _filePath);
            }
        }
    }

    public DashboardLoadResult GetCurrent()
    {
        if (_cache.TryGetValue(CacheKey, out DashboardLoadResult? cached) && cached is not null)
        {
            return cached;
        }
        return Reload();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        try
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }
        catch (ObjectDisposedException)
        {
            // racing with Dispose; ignore
        }
    }

    internal DashboardLoadResult Reload()
    {
        _gate.Wait();
        try
        {
            var sw = Stopwatch.StartNew();
            var result = LoadFromDisk();
            _cache.Set(CacheKey, result);
            sw.Stop();

            if (result.Error is null)
            {
                _logger.LogInformation(
                    "Loaded data.json from {Path} in {ElapsedMs}ms",
                    _filePath, sw.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to load data.json from {Path}: {Kind} - {Message}",
                    _filePath, result.Error.Kind, result.Error.Message);
            }

            try { DataChanged?.Invoke(this, EventArgs.Empty); }
            catch (Exception ex) { _logger.LogError(ex, "DataChanged handler threw"); }

            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    private DashboardLoadResult LoadFromDisk()
    {
        var loadedAt = DateTimeOffset.UtcNow;

        if (!File.Exists(_filePath))
        {
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: $"data.json not found at {_filePath}. See README for schema.",
                    Line: null,
                    Column: null,
                    Kind: DashboardLoadErrorKind.NotFound),
                LoadedAt: loadedAt);
        }

        byte[]? bytes = null;
        Exception? lastIo = null;
        for (var attempt = 0; attempt < ReadRetryCount; attempt++)
        {
            try
            {
                using var stream = new FileStream(
                    _filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                bytes = ms.ToArray();
                break;
            }
            catch (FileNotFoundException)
            {
                return new DashboardLoadResult(
                    Data: null,
                    Error: new DashboardLoadError(
                        FilePath: _filePath,
                        Message: $"data.json not found at {_filePath}. See README for schema.",
                        Line: null,
                        Column: null,
                        Kind: DashboardLoadErrorKind.NotFound),
                    LoadedAt: loadedAt);
            }
            catch (IOException ex)
            {
                lastIo = ex;
                Thread.Sleep(ReadRetryDelayMs);
            }
        }

        if (bytes is null)
        {
            _logger.LogError(lastIo, "IOException reading {Path} after {Attempts} attempts", _filePath, ReadRetryCount);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: lastIo?.Message ?? "I/O error reading data.json",
                    Line: null,
                    Column: null,
                    Kind: DashboardLoadErrorKind.ParseError),
                LoadedAt: loadedAt);
        }

        try
        {
            var data = JsonSerializer.Deserialize<DashboardData>(bytes, JsonOptions);
            if (data is null)
            {
                return new DashboardLoadResult(
                    Data: null,
                    Error: new DashboardLoadError(
                        FilePath: _filePath,
                        Message: "data.json deserialized to null.",
                        Line: null,
                        Column: null,
                        Kind: DashboardLoadErrorKind.ParseError),
                    LoadedAt: loadedAt);
            }
            return new DashboardLoadResult(data, Error: null, loadedAt);
        }
        catch (JsonException jex)
        {
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: jex.Message,
                    Line: jex.LineNumber.HasValue ? (int)(jex.LineNumber.Value + 1) : null,
                    Column: jex.BytePositionInLine.HasValue ? (int)(jex.BytePositionInLine.Value + 1) : null,
                    Kind: DashboardLoadErrorKind.ParseError),
                LoadedAt: loadedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error parsing {Path}", _filePath);
            return new DashboardLoadResult(
                Data: null,
                Error: new DashboardLoadError(
                    FilePath: _filePath,
                    Message: ex.Message,
                    Line: null,
                    Column: null,
                    Kind: DashboardLoadErrorKind.ParseError),
                LoadedAt: loadedAt);
        }
    }

    private static string ResolveDataFilePath(IWebHostEnvironment env)
    {
        var root = env.WebRootPath;
        if (string.IsNullOrEmpty(root))
        {
            root = Path.Combine(env.ContentRootPath, "wwwroot");
        }
        return Path.Combine(root, "data.json");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_watcher is not null)
        {
            try
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnFileEvent;
                _watcher.Created -= OnFileEvent;
                _watcher.Renamed -= OnFileEvent;
                _watcher.Dispose();
            }
            catch { /* best-effort */ }
        }

        try { _debounceTimer.Stop(); _debounceTimer.Dispose(); } catch { }
        _gate.Dispose();
    }
}
