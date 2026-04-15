// Standalone validation script - run with: dotnet run --project ReportingDashboard -- --validate
// This file is excluded from the main app build path; it serves as a manual verification aid.
// To validate data.json deserialization, execute the snippet below in a dotnet-script or test harness.

using System.Text.Json;
using ReportingDashboard.Models;

/// <summary>
/// Validates that data.json deserializes correctly against the DashboardData model.
/// Called from Program.cs when --validate flag is passed.
/// </summary>
public static class DataJsonValidator
{
    public static int Validate(string filePath)
    {
        Console.WriteLine($"Validating: {filePath}");

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"ERROR: File not found: {filePath}");
            return 1;
        }

        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        DashboardData? data;
        try
        {
            data = JsonSerializer.Deserialize<DashboardData>(json, options);
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"ERROR: Invalid JSON - {ex.Message}");
            return 1;
        }

        if (data is null)
        {
            Console.Error.WriteLine("ERROR: Deserialized to null.");
            return 1;
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(data.Title))
            errors.Add("title is missing or empty");
        if (string.IsNullOrWhiteSpace(data.Subtitle))
            errors.Add("subtitle is missing or empty");
        if (string.IsNullOrWhiteSpace(data.Timeline.StartMonth))
            errors.Add("timeline.startMonth is missing");
        if (string.IsNullOrWhiteSpace(data.Timeline.EndMonth))
            errors.Add("timeline.endMonth is missing");
        if (data.Timeline.Tracks.Count == 0)
            errors.Add("timeline.tracks is empty");
        foreach (var track in data.Timeline.Tracks)
        {
            if (track.Milestones.Count == 0)
                errors.Add($"track '{track.Id}' has no milestones");
        }
        if (data.Heatmap.Months.Count == 0)
            errors.Add("heatmap.months is empty");
        if (string.IsNullOrWhiteSpace(data.Heatmap.CurrentMonth))
            errors.Add("heatmap.currentMonth is missing");
        if (data.Heatmap.Categories.Count != 4)
            errors.Add($"expected 4 heatmap categories, found {data.Heatmap.Categories.Count}");

        if (errors.Count > 0)
        {
            Console.Error.WriteLine("VALIDATION ERRORS:");
            foreach (var e in errors) Console.Error.WriteLine($"  - {e}");
            return 1;
        }

        Console.WriteLine("OK - data.json is valid.");
        Console.WriteLine($"  Title: {data.Title}");
        Console.WriteLine($"  Subtitle: {data.Subtitle}");
        Console.WriteLine($"  BacklogUrl: {data.BacklogUrl ?? "(none)"}");
        Console.WriteLine($"  NowDate override: {data.NowDate ?? "(auto)"}");
        Console.WriteLine($"  Timeline: {data.Timeline.StartMonth} to {data.Timeline.EndMonth}, {data.Timeline.Tracks.Count} tracks");
        foreach (var t in data.Timeline.Tracks)
            Console.WriteLine($"    {t.Id} - {t.Label} [{t.Color}] - {t.Milestones.Count} milestones");
        Console.WriteLine($"  Heatmap: {data.Heatmap.Months.Count} months, current={data.Heatmap.CurrentMonth}");
        foreach (var c in data.Heatmap.Categories)
        {
            var totalItems = c.Items.Values.Sum(list => list.Count);
            Console.WriteLine($"    {c.Name} ({c.CssClass}) - {totalItems} items across {c.Items.Count} months");
        }

        return 0;
    }
}