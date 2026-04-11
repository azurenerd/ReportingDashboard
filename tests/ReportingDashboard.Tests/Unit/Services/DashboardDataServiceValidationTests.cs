using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceValidationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public DashboardDataServiceValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashValTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJson(object data)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));
        return path;
    }

    private string WriteRaw(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private DashboardDataService Svc() => new(_logger);

    private object ValidData() => new
    {
        title = "Test",
        subtitle = "Sub",
        backlogLink = "https://link",
        currentMonth = "April",
        months = new[] { "January" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]>(),
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        }
    };

    #region Individual field validation

    [Fact]
    public async Task LoadAsync_MissingTitle_ErrorMentionsTitle()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingSubtitle_ErrorMentionsSubtitle()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingBacklogLink_ErrorMentionsBacklogLink()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("backlogLink", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingCurrentMonth_ErrorMentionsCurrentMonth()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_EmptyMonths_ErrorMentionsMonths()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = Array.Empty<string>(),
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("months", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineStartDate_ErrorMentionsStartDate()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("startDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingTimelineEndDate_ErrorMentionsEndDate()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("endDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_EmptyTimelineTracks_ErrorMentionsTracks()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = Array.Empty<object>() },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("tracks", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_NullTimeline_ErrorMentionsTimeline()
    {
        // Construct JSON manually with null timeline
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["Jan"],
            "timeline": null,
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteRaw(json);
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("timeline", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Multiple validation errors

    [Fact]
    public async Task LoadAsync_MultipleInvalidFields_ErrorContainsAllFieldNames()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "",
            backlogLink = "",
            currentMonth = "",
            months = Array.Empty<string>(),
            timeline = (object?)null,
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("backlogLink", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("months", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MultipleTimelineErrors_AllReportedInMessage()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "", endDate = "", nowDate = "", tracks = Array.Empty<object>() },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("startDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("endDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tracks", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Whitespace-only field values treated as missing

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyTitle_TreatedAsMissing()
    {
        var path = WriteJson(new
        {
            title = "   ",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlySubtitle_TreatedAsMissing()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "  \t  ",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Valid data boundary cases

    [Fact]
    public async Task LoadAsync_MinimalValidData_Succeeds()
    {
        var path = WriteJson(ValidData());
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Null(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_ValidData_ClearsErrorState()
    {
        var path = WriteJson(ValidData());
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_ValidWithManyTracks_Succeeds()
    {
        var tracks = Enumerable.Range(1, 10).Select(i => new
        {
            name = $"M{i}",
            label = $"Track {i}",
            color = $"#{i:D6}",
            milestones = new[] { new { date = "2026-03-01", type = "poc", label = "PoC" } }
        }).ToArray();

        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan", "Feb", "Mar", "Apr" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(10, svc.Data!.Timeline.Tracks.Count);
    }

    [Fact]
    public async Task LoadAsync_ValidWithManyMonths_Succeeds()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "December",
            months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-12-31", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(12, svc.Data!.Months.Count);
    }

    #endregion

    #region JSON edge cases

    [Fact]
    public async Task LoadAsync_JsonWithExtraFields_IgnoredSuccessfully()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["Jan"],
            "unknownField": "should be ignored",
            "anotherExtra": 42,
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteRaw(json);
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_JsonWithUnicodeCharacters_DeserializesCorrectly()
    {
        var path = WriteJson(new
        {
            title = "Privacy Automation — Release Roadmap",
            subtitle = "Trusted Platform · Privacy · April 2026",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "Core — V1", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Contains("—", svc.Data!.Title);
        Assert.Contains("·", svc.Data.Subtitle);
    }

    [Fact]
    public async Task LoadAsync_ArrayInsteadOfObject_SetsError()
    {
        var path = WriteRaw("[1, 2, 3]");
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_NumberInsteadOfObject_SetsError()
    {
        var path = WriteRaw("42");
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_StringInsteadOfObject_SetsError()
    {
        var path = WriteRaw("\"just a string\"");
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_BooleanInsteadOfObject_SetsError()
    {
        var path = WriteRaw("true");
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_TrailingCommaJson_SetsError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
        }
        """;
        var path = WriteRaw(json);
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_DuplicateKeys_DoesNotCrash()
    {
        var json = """
        {
            "title": "First",
            "title": "Second",
            "subtitle": "Sub",
            "backlogLink": "https://link",
            "currentMonth": "April",
            "months": ["Jan"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-07-01", "nowDate": "2026-04-10", "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }] },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteRaw(json);
        var svc = Svc();

        await svc.LoadAsync(path);

        // Should not crash, may succeed or fail validation depending on which value wins
        Assert.NotNull(svc);
    }

    #endregion

    #region State consistency after error

    [Fact]
    public async Task LoadAsync_OnError_DataIsNull()
    {
        var path = WriteRaw("{{{bad json}}}");
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_OnFileNotFound_DataIsNull()
    {
        var svc = Svc();

        await svc.LoadAsync(Path.Combine(_tempDir, "does_not_exist.json"));

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_OnValidationError_DataIsNull()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "",
            months = Array.Empty<string>()
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_ErrorThenSuccess_ClearsErrorState()
    {
        var svc = Svc();

        // First: error
        await svc.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        Assert.True(svc.IsError);
        Assert.Null(svc.Data);

        // Second: success
        var path = WriteJson(ValidData());
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_SuccessThenError_ClearsData()
    {
        var svc = Svc();
        var path = WriteJson(ValidData());

        await svc.LoadAsync(path);
        Assert.NotNull(svc.Data);

        await svc.LoadAsync(Path.Combine(_tempDir, "gone.json"));

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
        Assert.NotNull(svc.ErrorMessage);
    }

    #endregion

    #region Error message format

    [Fact]
    public async Task LoadAsync_FileNotFound_ErrorContainsExactPath()
    {
        var svc = Svc();
        var fakePath = Path.Combine(_tempDir, "specific_file_name.json");

        await svc.LoadAsync(fakePath);

        Assert.Contains("specific_file_name.json", svc.ErrorMessage!);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ErrorStartsWithExpectedPrefix()
    {
        var svc = Svc();

        await svc.LoadAsync(Path.Combine(_tempDir, "x.json"));

        Assert.StartsWith("data.json not found at", svc.ErrorMessage!);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ErrorContainsParsePrefix()
    {
        var path = WriteRaw("{bad}");
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.Contains("Failed to parse data.json", svc.ErrorMessage!);
    }

    [Fact]
    public async Task LoadAsync_NullDeserialization_ErrorContainsNullMessage()
    {
        var path = WriteRaw("null");
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("null", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_ValidationError_ErrorContainsValidationPrefix()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.Contains("data.json validation:", svc.ErrorMessage!);
    }

    #endregion

    #region Heatmap data deserialization

    [Fact]
    public async Task LoadAsync_HeatmapWithAllCategories_DeserializesCorrectly()
    {
        var path = WriteJson(new
        {
            title = "Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "Jan", "Feb", "Mar", "Apr" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Feature A", "Feature B" },
                    ["feb"] = new[] { "Feature C" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Feature D" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["mar"] = new[] { "Feature E" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Blocker 1", "Blocker 2" }
                }
            }
        });
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(2, svc.Data!.Heatmap.Shipped.Count);
        Assert.Equal(2, svc.Data.Heatmap.Shipped["jan"].Count);
        Assert.Single(svc.Data.Heatmap.InProgress);
        Assert.Single(svc.Data.Heatmap.Carryover);
        Assert.Equal(2, svc.Data.Heatmap.Blockers["apr"].Count);
    }

    [Fact]
    public async Task LoadAsync_HeatmapWithEmptyCategories_IsValid()
    {
        var path = WriteJson(ValidData());
        var svc = Svc();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data!.Heatmap);
    }

    #endregion
}