using System.Text.Json;
using ReportingDashboard.Data;

namespace ReportingDashboard.Services;

public class DashboardDataService : IDisposable
{
    private readonly string _dataFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool HasData => Data is not null && ErrorMessage is null;

    public event Action? OnDataChanged;

    public DashboardDataService(string dataFilePath)
    {
        _dataFilePath = dataFilePath;
        Reload();
    }

    public void Reload()
    {
        Data = null;
        ErrorMessage = null;

        if (!File.Exists(_dataFilePath))
        {
            ErrorMessage = "Dashboard data not found. Please ensure data.json exists in the Data/ directory.";
            return;
        }

        try
        {
            var json = File.ReadAllText(_dataFilePath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (data is null)
            {
                ErrorMessage = "Error reading data.json: deserialization returned null. Please check the file contents and refresh.";
                return;
            }

            var validationError = Validate(data);
            if (validationError is not null)
            {
                ErrorMessage = validationError;
                return;
            }

            Data = data;
        }
        catch (JsonException ex)
        {
            ErrorMessage = $"Error reading data.json: {ex.Message}. Please fix the JSON syntax and refresh.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error reading data.json: {ex.Message}. Please fix the file and refresh.";
        }

        OnDataChanged?.Invoke();
    }

    private static string? Validate(DashboardData data)
    {
        if (data.Timeline.Tracks is null || data.Timeline.Tracks.Count == 0)
            return "Validation error: timeline.tracks must have at least 1 entry.";

        if (data.Heatmap.Columns is null || data.Heatmap.Columns.Count == 0)
            return "Validation error: heatmap.columns must have at least 1 entry.";

        if (!data.Heatmap.Columns.Contains(data.Heatmap.CurrentColumn))
            return $"Validation error: heatmap.currentColumn \"{data.Heatmap.CurrentColumn}\" must match one of the columns values.";

        if (data.Heatmap.Rows is null || data.Heatmap.Rows.Count == 0)
            return "Validation error: heatmap.rows must have at least 1 entry.";

        var validTypes = new HashSet<string> { "start", "checkpoint", "poc", "production" };
        foreach (var track in data.Timeline.Tracks)
        {
            if (track.Milestones is null) continue;
            foreach (var milestone in track.Milestones)
            {
                if (!validTypes.Contains(milestone.Type))
                    return $"Validation error: milestone type \"{milestone.Type}\" in track \"{track.Id}\" is not valid. Allowed: start, checkpoint, poc, production.";
            }
        }

        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}