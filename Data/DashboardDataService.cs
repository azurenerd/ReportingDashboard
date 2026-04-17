using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReportingDashboard.Data;

public class DashboardDataService
{
    private readonly string _dataFilePath;

    public DashboardDataService(IConfiguration config)
    {
        _dataFilePath = config.GetValue<string>("DashboardDataPath") ?? "Data/data.json";
    }

    public async Task<DashboardReport> GetDataAsync()
    {
        ValidateFilePath();
        var json = await ReadJsonFileAsync();
        var data = DeserializeJson(json);
        ValidateSchema(data);
        return data;
    }

    private void ValidateFilePath()
    {
        var fullPath = Path.GetFullPath(_dataFilePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Dashboard data file not found at: {fullPath}. " +
                $"Please create a data.json file or ensure the DashboardDataPath setting in appsettings.json is correct.",
                fullPath);
        }
    }

    private async Task<string> ReadJsonFileAsync()
    {
        try
        {
            return await File.ReadAllTextAsync(_dataFilePath);
        }
        catch (IOException ex) when (!(ex is FileNotFoundException))
        {
            throw new InvalidOperationException(
                $"Unable to read data file at {Path.GetFullPath(_dataFilePath)}: {ex.Message}",
                ex);
        }
    }

    private DashboardReport DeserializeJson(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            var data = JsonSerializer.Deserialize<DashboardReport>(json, options);
            
            if (data == null)
            {
                throw new InvalidOperationException(
                    "data.json deserialized to null. The file may be empty or contain invalid JSON.");
            }

            return data;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Invalid JSON in data.json at line {ex.LineNumber}, position {ex.BytePositionInLine}: {ex.Message}",
                ex);
        }
    }

    private void ValidateSchema(DashboardReport data)
    {
        ValidateHeader(data.Header);
        ValidateTimelineTracks(data.TimelineTracks);
        ValidateHeatmap(data.Heatmap);
    }

    private void ValidateHeader(HeaderInfo header)
    {
        if (string.IsNullOrWhiteSpace(header?.Title))
        {
            throw new InvalidOperationException(
                "Validation error: header.title is required and cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(header.TimelineStartDate))
        {
            throw new InvalidOperationException(
                "Validation error: header.timelineStartDate is required (ISO 8601 format, e.g., '2026-01-01').");
        }

        if (string.IsNullOrWhiteSpace(header.TimelineEndDate))
        {
            throw new InvalidOperationException(
                "Validation error: header.timelineEndDate is required (ISO 8601 format, e.g., '2026-07-01').");
        }

        if (string.IsNullOrWhiteSpace(header.ReportDate))
        {
            throw new InvalidOperationException(
                "Validation error: header.reportDate is required (ISO 8601 format, e.g., '2026-04-17').");
        }

        ValidateDateFormat("header.timelineStartDate", header.TimelineStartDate);
        ValidateDateFormat("header.timelineEndDate", header.TimelineEndDate);
        ValidateDateFormat("header.reportDate", header.ReportDate);

        if (header.TimelineMonths == null || header.TimelineMonths.Count == 0)
        {
            throw new InvalidOperationException(
                "Validation error: header.timelineMonths must be a non-empty array of month labels (e.g., [\"Jan\", \"Feb\"]).");
        }

        if (string.IsNullOrWhiteSpace(header.BacklogLink))
        {
            header.BacklogLink = "#";
        }
    }

    private void ValidateTimelineTracks(List<TimelineTrack>? tracks)
    {
        if (tracks == null || tracks.Count == 0)
        {
            throw new InvalidOperationException(
                "Validation error: timelineTracks must be a non-empty array of track objects.");
        }

        for (int i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];

            if (string.IsNullOrWhiteSpace(track?.Id))
            {
                throw new InvalidOperationException(
                    $"Validation error: timelineTracks[{i}].id is required.");
            }

            if (string.IsNullOrWhiteSpace(track.Name))
            {
                throw new InvalidOperationException(
                    $"Validation error: timelineTracks[{i}].name is required.");
            }

            if (string.IsNullOrWhiteSpace(track.Color))
            {
                track.Color = "#999";
            }

            if (track.Milestones == null)
            {
                track.Milestones = [];
            }

            ValidateMilestones(i, track.Milestones);
        }
    }

    private void ValidateMilestones(int trackIndex, List<Milestone> milestones)
    {
        for (int i = 0; i < milestones.Count; i++)
        {
            var milestone = milestones[i];

            if (string.IsNullOrWhiteSpace(milestone?.Label))
            {
                throw new InvalidOperationException(
                    $"Validation error: timelineTracks[{trackIndex}].milestones[{i}].label is required.");
            }

            if (string.IsNullOrWhiteSpace(milestone.Date))
            {
                throw new InvalidOperationException(
                    $"Validation error: timelineTracks[{trackIndex}].milestones[{i}].date is required (ISO 8601 format).");
            }

            ValidateDateFormat($"timelineTracks[{trackIndex}].milestones[{i}].date", milestone.Date);

            if (string.IsNullOrWhiteSpace(milestone.Type))
            {
                milestone.Type = "checkpoint";
            }
            else if (!new[] { "checkpoint", "minor", "poc", "production" }.Contains(milestone.Type.ToLower()))
            {
                throw new InvalidOperationException(
                    $"Validation error: timelineTracks[{trackIndex}].milestones[{i}].type must be one of: 'checkpoint', 'minor', 'poc', 'production'.");
            }

            if (milestone.LabelPosition != null && !new[] { "above", "below" }.Contains(milestone.LabelPosition.ToLower()))
            {
                throw new InvalidOperationException(
                    $"Validation error: timelineTracks[{trackIndex}].milestones[{i}].labelPosition must be 'above' or 'below'.");
            }
        }
    }

    private void ValidateHeatmap(HeatmapData? heatmap)
    {
        if (heatmap == null)
        {
            throw new InvalidOperationException(
                "Validation error: heatmap object is required.");
        }

        if (heatmap.Columns == null || heatmap.Columns.Count == 0)
        {
            throw new InvalidOperationException(
                "Validation error: heatmap.columns must be a non-empty array of month names.");
        }

        if (heatmap.HighlightColumnIndex < 0 || heatmap.HighlightColumnIndex >= heatmap.Columns.Count)
        {
            throw new InvalidOperationException(
                $"Validation error: heatmap.highlightColumnIndex ({heatmap.HighlightColumnIndex}) must be between 0 and {heatmap.Columns.Count - 1}.");
        }

        if (heatmap.Rows == null || heatmap.Rows.Count == 0)
        {
            throw new InvalidOperationException(
                "Validation error: heatmap.rows must be a non-empty array of row objects.");
        }

        var expectedCategories = new[] { "shipped", "in-progress", "carryover", "blockers" };
        if (heatmap.Rows.Count != 4)
        {
            throw new InvalidOperationException(
                $"Validation error: heatmap.rows must contain exactly 4 rows (shipped, in-progress, carryover, blockers), but found {heatmap.Rows.Count}.");
        }

        for (int i = 0; i < heatmap.Rows.Count; i++)
        {
            var row = heatmap.Rows[i];

            if (string.IsNullOrWhiteSpace(row?.Category))
            {
                throw new InvalidOperationException(
                    $"Validation error: heatmap.rows[{i}].category is required.");
            }

            if (!expectedCategories.Contains(row.Category.ToLower()))
            {
                throw new InvalidOperationException(
                    $"Validation error: heatmap.rows[{i}].category must be one of: 'shipped', 'in-progress', 'carryover', 'blockers'.");
            }

            if (string.IsNullOrWhiteSpace(row.Label))
            {
                row.Label = row.Category;
            }

            if (row.CellItems == null)
            {
                row.CellItems = [];
            }

            ValidateRowItems(i, row.CellItems, heatmap.Columns.Count);
        }
    }

    private void ValidateRowItems(int rowIndex, List<List<string>> cellItems, int expectedColumnCount)
    {
        if (cellItems.Count != expectedColumnCount)
        {
            throw new InvalidOperationException(
                $"Validation error: heatmap.rows[{rowIndex}].cellItems must have {expectedColumnCount} columns to match heatmap.columns, but found {cellItems.Count}.");
        }

        for (int i = 0; i < cellItems.Count; i++)
        {
            if (cellItems[i] == null)
            {
                cellItems[i] = [];
            }
        }
    }

    private void ValidateDateFormat(string fieldName, string dateStr)
    {
        try
        {
            DateOnly.Parse(dateStr);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                $"Validation error: {fieldName} must be in ISO 8601 format (YYYY-MM-DD), but got '{dateStr}'.");
        }
    }
}