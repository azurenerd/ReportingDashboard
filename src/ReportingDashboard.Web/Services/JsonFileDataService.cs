using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class JsonFileDataService : IDataService
{
    private readonly DashboardData? _data;
    private readonly string? _error;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonFileDataService(IOptions<DashboardOptions> options)
    {
        var configuredPath = options.Value.DataFilePath ?? "./data.json";
        var path = configuredPath;

        try
        {
            // Only fall back to data.sample.json when using the default path
            // and the default data.json doesn't exist. If a specific path was
            // provided (e.g., by tests), respect it and report an error if missing.
            if (!File.Exists(path))
            {
                var isDefaultPath = configuredPath == "./data.json"
                    || configuredPath == "data.json";

                if (isDefaultPath)
                {
                    // Try to find data.sample.json as fallback for out-of-the-box experience
                    var samplePath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".", "data.sample.json");
                    if (File.Exists(samplePath))
                    {
                        path = samplePath;
                    }
                    else
                    {
                        _error = $"Could not load data.json - file not found at {Path.GetFullPath(configuredPath)}. "
                               + "Please create a data.json file. See data.sample.json for the expected schema.";
                        return;
                    }
                }
                else
                {
                    _error = $"Could not load data.json - file not found at {Path.GetFullPath(path)}. "
                           + "Please create a data.json file. See data.sample.json for the expected schema.";
                    return;
                }
            }

            var json = File.ReadAllText(path);
            _data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _error = $"Could not parse data.json: {ex.Message}";
        }
        catch (Exception ex)
        {
            _error = $"Error loading data.json: {ex.Message}";
        }
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;
}