using System.Text.Json;
using ReportingDashboard.Data;

namespace ReportingDashboard.Services;

public enum DataErrorType
{
    None,
    FileNotFound,
    JsonParseError,
    ValidationError
}

public class DashboardDataService : IDisposable
{
    private readonly string _dataFilePath;
    private FileSystemWatcher? _watcher;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public DashboardData? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorDetails { get; private set; }
    public DataErrorType ErrorType { get; private set; }
    public bool HasData => Data != null;

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
        ErrorDetails = null;
        ErrorType = DataErrorType.None;

        try
        {
            if (!File.Exists(_dataFilePath))
            {
                ErrorType = DataErrorType.FileNotFound;
                ErrorMessage = "Dashboard data not found. Please ensure data.json exists in the Data/ directory.";
                ErrorDetails = _dataFilePath;
                return;
            }

            var json = File.ReadAllText(_dataFilePath);

            try
            {
                using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });
            }
            catch (JsonException ex)
            {
                ErrorType = DataErrorType.JsonParseError;
                ErrorMessage = "Error reading data.json. Please fix the JSON syntax and refresh.";
                var line = ex.LineNumber.HasValue ? ex.LineNumber.Value + 1 : 0;
                var pos = ex.BytePositionInLine.HasValue ? ex.BytePositionInLine.Value + 1 : 0;
                ErrorDetails = line > 0
                    ? $"Line {line}, Character {pos}: {ex.Message}"
                    : ex.Message;
                return;
            }

            try
            {
                Data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                ErrorType = DataErrorType.ValidationError;
                ErrorMessage = "data.json is missing required fields or contains invalid data types.";
                ErrorDetails = ex.Message;
                return;
            }

            if (Data == null)
            {
                ErrorType = DataErrorType.ValidationError;
                ErrorMessage = "data.json is empty or could not be deserialized.";
                return;
            }

            var validationErrors = new List<string>();

            if (Data.Project == null)
                validationErrors.Add("project section is missing");
            if (Data.Timeline == null)
                validationErrors.Add("timeline section is missing");
            if (Data.Heatmap == null)
                validationErrors.Add("heatmap section is missing");

            if (validationErrors.Count > 0)
            {
                ErrorType = DataErrorType.ValidationError;
                ErrorMessage = $"data.json is missing required fields: {string.Join(", ", validationErrors.Select(e => e.Replace(" section is missing", "")))}. Please check the file format.";
                ErrorDetails = string.Join("\n", validationErrors.Select(e => $"  \u2022 {e}"));
                Data = null;
                return;
            }

            if (Data.Timeline!.Tracks == null || Data.Timeline.Tracks.Count == 0)
                validationErrors.Add("timeline.tracks must have at least 1 entry");
            if (Data.Heatmap!.Columns == null || Data.Heatmap.Columns.Count == 0)
                validationErrors.Add("heatmap.columns must have at least 1 entry");
            if (Data.Heatmap.Rows == null || Data.Heatmap.Rows.Count == 0)
                validationErrors.Add("heatmap.rows must have at least 1 entry");
            if (Data.Heatmap.CurrentColumn != null && Data.Heatmap.Columns != null
                && !Data.Heatmap.Columns.Contains(Data.Heatmap.CurrentColumn))
                validationErrors.Add($"heatmap.currentColumn value '{Data.Heatmap.CurrentColumn}' is not found in heatmap.columns");

            if (validationErrors.Count > 0)
            {
                ErrorType = DataErrorType.ValidationError;
                ErrorMessage = "data.json has invalid or incomplete configuration.";
                ErrorDetails = string.Join("\n", validationErrors.Select(e => $"  \u2022 {e}"));
                Data = null;
                return;
            }
        }
        catch (Exception ex)
        {
            ErrorType = DataErrorType.JsonParseError;
            ErrorMessage = "An unexpected error occurred while loading data.json.";
            ErrorDetails = ex.Message;
            Data = null;
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}