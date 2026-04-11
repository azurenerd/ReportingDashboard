using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the data contract between data.json header fields
/// and the DashboardDataService → DashboardData model pipeline.
/// Ensures header-specific fields (title, subtitle, backlogLink, currentMonth)
/// round-trip correctly through file I/O and JSON deserialization.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderDataContractTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public HeaderDataContractTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HdrContract_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    #region Header Field Deserialization from Real JSON

    [Theory]
    [InlineData("Short Title")]
    [InlineData("A Very Long Project Title That Spans Many Characters To Test Layout Behavior")]
    [InlineData("Project: <Special> & \"Chars\"")]
    [InlineData("日本語プロジェクト")]
    public async Task LoadAsync_VariousTitles_DeserializedCorrectly(string title)
    {
        var json = CreateValidJsonWithTitle(title);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(title, svc.Data!.Title);
    }

    [Theory]
    [InlineData("Team X - January 2026")]
    [InlineData("Platform Engineering — Q2 Sprint")]
    [InlineData("Équipe développement - Avril 2026")]
    public async Task LoadAsync_VariousSubtitles_DeserializedCorrectly(string subtitle)
    {
        var json = CreateValidJsonWithSubtitle(subtitle);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(subtitle, svc.Data!.Subtitle);
    }

    [Theory]
    [InlineData("https://dev.azure.com/org/project/_backlogs")]
    [InlineData("https://dev.azure.com/org/project/_backlogs?query=active&filter=sprint")]
    [InlineData("https://example.com/with spaces")]
    public async Task LoadAsync_VariousBacklogLinks_DeserializedCorrectly(string link)
    {
        var json = CreateValidJsonWithBacklogLink(link);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(link, svc.Data!.BacklogLink);
    }

    [Theory]
    [InlineData("January")]
    [InlineData("February")]
    [InlineData("December")]
    [InlineData("Apr 2026")]
    public async Task LoadAsync_VariousCurrentMonth_DeserializedCorrectly(string month)
    {
        var json = CreateValidJsonWithCurrentMonth(month);
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(month, svc.Data!.CurrentMonth);
    }

    #endregion

    #region Header Validation Failures

    [Fact]
    public async Task LoadAsync_MissingTitle_ValidationError()
    {
        var json = """
        {
            "subtitle": "Team",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingSubtitle_ValidationError()
    {
        var json = """
        {
            "title": "T",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingBacklogLink_ValidationError()
    {
        var json = """
        {
            "title": "T",
            "subtitle": "S",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("backlogLink", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingCurrentMonth_ValidationError()
    {
        var json = """
        {
            "title": "T",
            "subtitle": "S",
            "backlogLink": "https://link",
            "months": ["April"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_AllHeaderFieldsMissing_ErrorContainsAll()
    {
        var json = """
        {
            "months": ["April"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("backlogLink", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region JSON Case Sensitivity

    [Fact]
    public async Task LoadAsync_CamelCaseJson_DeserializesHeaderFields()
    {
        var json = """
        {
            "title": "CamelCase Test",
            "subtitle": "Sub",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["April"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "nowDate": "2026-04-10", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("CamelCase Test", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_PascalCaseJson_DeserializesHeaderFields()
    {
        // PropertyNameCaseInsensitive = true in the service
        var json = """
        {
            "Title": "PascalCase Test",
            "Subtitle": "Sub",
            "BacklogLink": "https://link",
            "CurrentMonth": "April",
            "Months": ["April"],
            "Timeline": { "StartDate": "2026-01-01", "EndDate": "2026-07-01", "NowDate": "2026-04-10", "Tracks": [{"Name":"M1","Label":"L","Color":"#000","Milestones":[]}] },
            "Heatmap": { "Shipped": {}, "InProgress": {}, "Carryover": {}, "Blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("PascalCase Test", svc.Data!.Title);
        Assert.Equal("April", svc.Data.CurrentMonth);
    }

    #endregion

    #region Extra Fields Resilience

    [Fact]
    public async Task LoadAsync_ExtraFieldsInJson_HeaderFieldsStillDeserialize()
    {
        var json = """
        {
            "title": "Extra Fields Test",
            "subtitle": "Sub",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "unknownField": "should be ignored",
            "version": 42,
            "months": ["April"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "nowDate": "2026-04-10", "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[]}] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Extra Fields Test", svc.Data!.Title);
        Assert.Equal("Sub", svc.Data.Subtitle);
    }

    #endregion

    #region Helper Methods

    private static string CreateValidJsonWithTitle(string title) =>
        JsonSerializer.Serialize(new
        {
            title,
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private static string CreateValidJsonWithSubtitle(string subtitle) =>
        JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle,
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private static string CreateValidJsonWithBacklogLink(string backlogLink) =>
        JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle = "S",
            backlogLink,
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    private static string CreateValidJsonWithCurrentMonth(string currentMonth) =>
        JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth,
            months = new[] { "April" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    #endregion
}