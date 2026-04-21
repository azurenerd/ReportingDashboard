using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed class DashboardDataService : IDashboardDataService, IDisposable
{
    private readonly string _filePath;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardDataService> _logger;
    private readonly FileSystemWatcher _watcher;
    private readonly System.Timers.Timer _debounceTimer;
    private readonly SemaphoreSlim _reloadLock = new(1, 1);
    private const string CacheKey = "dashboard:current";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public DashboardDataService(
        IWebHostEnvironment env,
        IMemoryCache cache,
        ILogger<DashboardDataService> logger)
    {
        _cache = cache;
        _logger = logger;
        _filePath = Path.Combine(env.WebRootPath, "data.json");

        LoadAndCache();

        var directory = Path.GetDirectoryName(_filePath)!;
        var fileName = Path.GetFileName(_filePath);
        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Renamed += (s, e) => OnFileChanged(s, e);

        _debounceTimer = new System.Timers.Timer(250) { AutoReset = false };
        _debounceTimer.Elapsed += async (s, e) => await ReloadAsync();
    }

    public DashboardLoadResult GetCurrent()
    {
        return _cache.Get<DashboardLoadResult>(CacheKey) ?? LoadAndCache();
    }

    public event EventHandler? DataChanged;

    private void OnFileChanged(object? sender, FileSystemEventArgs e)
    {
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private async Task ReloadAsync()
    {
        await _reloadLock.WaitAsync();
        try
        {
            LoadAndCache();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _reloadLock.Release();
        }
    }

    private DashboardLoadResult LoadAndCache()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        DashboardLoadResult result;

        try
        {
            if (!File.Exists(_filePath))
            {
                _logger.LogWarning("data.json not found at {Path}", _filePath);
                result = new DashboardLoadResult(
                    null,
                    new DashboardLoadError(_filePath,
                        $"data.json not found at {_filePath}. See README for schema.",
                        null, null, "NotFound"),
                    DateTimeOffset.Now);
            }
            else
            {
                var json = ReadFileWithRetry(_filePath);
                var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

                if (data is null)
                {
                    result = new DashboardLoadResult(
                        null,
                        new DashboardLoadError(_filePath, "Deserialization returned null.",
                            null, null, "ParseError"),
                        DateTimeOffset.Now);
                }
                else
                {
                    var validationErrors = DashboardDataValidator.Validate(data);
                    if (validationErrors.Count > 0)
                    {
                        var message = string.Join(" ", validationErrors);
                        _logger.LogWarning("Validation errors in data.json: {Errors}", message);
                        result = new DashboardLoadResult(
                            null,
                            new DashboardLoadError(_filePath, message, null, null, "ValidationError"),
                            DateTimeOffset.Now);
                    }
                    else
                    {
                        sw.Stop();
                        _logger.LogInformation(
                            "Loaded data.json ({Size} bytes) in {Duration}ms",
                            new FileInfo(_filePath).Length, sw.ElapsedMilliseconds);
                        result = new DashboardLoadResult(data, null, DateTimeOffset.Now);
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json");
            result = new DashboardLoadResult(
                null,
                new DashboardLoadError(_filePath, ex.Message,
                    (int?)ex.LineNumber, (int?)ex.BytePositionInLine, "ParseError"),
                DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load data.json");
            result = new DashboardLoadResult(
                null,
                new DashboardLoadError(_filePath, ex.Message, null, null, "ParseError"),
                DateTimeOffset.Now);
        }

        _cache.Set(CacheKey, result);
        return result;
    }

    private static string ReadFileWithRetry(string path, int maxRetries = 3, int delayMs = 50)
    {
        for (int i = 0; i < maxRetries - 1; i++)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (IOException)
            {
                Thread.Sleep(delayMs);
            }
        }
        using var finalStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var finalReader = new StreamReader(finalStream);
        return finalReader.ReadToEnd();
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
        _debounceTimer.Dispose();
        _reloadLock.Dispose();
    }
}