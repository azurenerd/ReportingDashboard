using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public class JsonFileDataService : IDataService
{
    private readonly DashboardData? _data;
    private readonly string? _error;

    public JsonFileDataService(IOptions<DashboardOptions> options)
    {
        var path = options.Value.DataFilePath ?? "./data.json";
        try
        {
            // If the configured data file doesn't exist, fall back to data.sample.json
            // so the app renders a working dashboard on fresh clone.
            if (!File.Exists(path))
            {
                var samplePath = Path.Combine(Path.GetDirectoryName(path) ?? ".", "data.sample.json");
                if (File.Exists(samplePath))
                {
                    path = samplePath;
                }
                else
                {
                    _error = $"Could not load data.json \u2014 file not found at {Path.GetFullPath(path)}. "
                           + "Please create a data.json file. See data.sample.json for the expected schema.";
                    return;
                }
            }

            var json = File.ReadAllText(path);
            _data = JsonSerializer.Deserialize<DashboardData>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _error = $"Could not parse data.json: {ex.Message}";
        }
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;
}