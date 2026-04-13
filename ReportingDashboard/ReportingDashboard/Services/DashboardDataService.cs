using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private DashboardConfig? _cachedConfig;
    private bool _loaded;

    /// <summary>
    /// Non-null if data.json loading or parsing failed. Contains the error message.
    /// </summary>
    public string? LoadError { get; private set; }

    public DashboardDataService(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Returns the parsed dashboard configuration, or null if loading failed.
    /// Results are cached after the first successful load. Failures are not cached,
    /// allowing retry on subsequent calls (e.g., after fixing a transient file lock).
    /// </summary>
    public async Task<DashboardConfig?> GetDashboardConfigAsync()
    {
        if (_loaded)
            return _cachedConfig;

        var path = Path.Combine(_env.WebRootPath, "data", "data.json");

        try
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Configuration file not found at: {path}");

            var json = await File.ReadAllTextAsync(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<DashboardConfig>(json, options);

            if (config is null)
                throw new InvalidDataException("Deserialization returned null. The JSON file may be empty or contain only 'null'.");

            if (string.IsNullOrWhiteSpace(config.Title))
                throw new InvalidDataException("Required field 'title' is missing or empty in data.json.");

            if (config.Months is null || config.Months.Count == 0)
                throw new InvalidDataException("Required field 'months' is missing or empty in data.json.");

            _cachedConfig = config;
            LoadError = null;
            _loaded = true;
            return _cachedConfig;
        }
        catch (JsonException ex)
        {
            LoadError = $"Failed to load data.json: JSON parsing error at line {ex.LineNumber}, position {ex.BytePositionInLine} - {ex.Message}";
            return null;
        }
        catch (FileNotFoundException ex)
        {
            LoadError = $"Failed to load data.json: {ex.Message}";
            return null;
        }
        catch (InvalidDataException ex)
        {
            LoadError = $"Failed to load data.json: {ex.Message}";
            return null;
        }
        catch (Exception ex)
        {
            LoadError = $"Failed to load data.json: {ex.Message}";
            return null;
        }
    }
}