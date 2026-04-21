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
        var path = options.Value.DataFilePath ?? "./data.json";
        try
        {
            if (!File.Exists(path))
            {
                _error = $"Could not load data.json - file not found at {Path.GetFullPath(path)}. "
                       + "Please create a data.json file. See data.sample.json for the expected schema.";
                return;
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