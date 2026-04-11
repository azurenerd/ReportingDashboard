using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Focused validation edge-case tests for DashboardDataService.
/// Complements DashboardDataServiceActualTests and DashboardDataServiceTests
/// by covering specific validation combinations and boundary conditions.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceValidationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

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

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

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

    private DashboardDataService CreateService() => new(_logger);

    private object CreateMinimalValidData() => new
    {
        title = "T",
        subtitle = "S",
        backlogLink = "https://link",
        currentMonth = "Jan",
        months = new[] { "January" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]>(),
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        }
    };

    #region Individual Validation Failures

    [Fact]
    public async Task LoadAsync_MissingTitle_ErrorMentionsTitle()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingSubtitle_ErrorMentionsSubtitle()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingBacklogLink_ErrorMentionsBacklogLink()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("backlogLink", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingCurrentMonth_ErrorMentionsCurrentMonth()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_EmptyMonthsArray_ErrorMentionsMonths()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = Array.Empty<string>(),
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("months", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Timeline Validation

    [Fact]
    public async Task LoadAsync_TimelineMissingStartDate_ErrorMentionsStartDate()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("startDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_TimelineMissingEndDate_ErrorMentionsEndDate()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("endDate", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_TimelineEmptyTracks_ErrorMentionsTracks()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = Array.Empty<object>() },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("tracks", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Multiple Validation Errors

    [Fact]
    public async Task LoadAsync_MultipleFieldsMissing_ErrorContainsAllFieldNames()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "",
            backlogLink = "",
            currentMonth = "",
            months = Array.Empty<string>(),
            timeline = new { startDate = "", endDate = "", tracks = Array.Empty<object>() },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("backlogLink", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("months", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MultipleErrors_JoinedWithSemicolon()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains(";", svc.ErrorMessage!);
    }

    #endregion

    #region Whitespace-Only Fields

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyTitle_TreatedAsEmpty()
    {
        var path = WriteJson(new
        {
            title = "   ",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlySubtitle_TreatedAsEmpty()
    {
        var path = WriteJson(new
        {
            title = "T",
            subtitle = "\t\n ",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Successful Validation (Boundary)

    [Fact]
    public async Task LoadAsync_MinimalValidData_Succeeds()
    {
        var path = WriteJson(CreateMinimalValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Null(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_SingleCharacterFields_Valid()
    {
        var path = WriteJson(CreateMinimalValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("T", svc.Data!.Title);
    }

    #endregion

    #region Service State After Error

    [Fact]
    public async Task LoadAsync_AfterError_DataIsNull()
    {
        var path = WriteRaw("not json at all");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_AfterValidationError_DataIsNull()
    {
        var path = WriteJson(new { title = "", subtitle = "", months = Array.Empty<string>() });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ErrorMessageContainsNotFound()
    {
        var svc = CreateService();

        await svc.LoadAsync(Path.Combine(_tempDir, "does_not_exist.json"));

        Assert.True(svc.IsError);
        Assert.Contains("not found", svc.ErrorMessage!);
    }

    [Fact]
    public async Task LoadAsync_ValidAfterError_ClearsErrorState()
    {
        var svc = CreateService();

        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        Assert.True(svc.IsError);

        var validPath = WriteJson(CreateMinimalValidData());
        await svc.LoadAsync(validPath);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_ErrorAfterValid_ClearsDataAndSetsError()
    {
        var svc = CreateService();

        var validPath = WriteJson(CreateMinimalValidData());
        await svc.LoadAsync(validPath);
        Assert.NotNull(svc.Data);

        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    #endregion

    #region JSON Edge Cases

    [Fact]
    public async Task LoadAsync_JsonArray_SetsError()
    {
        var path = WriteRaw("[1, 2, 3]");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_JsonNull_SetsError()
    {
        var path = WriteRaw("null");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("null", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_ExtraFieldsInJson_IgnoredAndSucceeds()
    {
        var json = """
        {
            "title": "T",
            "subtitle": "S",
            "backlogLink": "https://link",
            "currentMonth": "Jan",
            "months": ["January"],
            "extraField": "should be ignored",
            "anotherExtra": 42,
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-10",
                "tracks": [{"name": "M1", "label": "L", "color": "#000", "milestones": []}]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteRaw(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("T", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_UnicodeInFields_DeserializesCorrectly()
    {
        var json = """
        {
            "title": "Tableau de bord exécutif",
            "subtitle": "Équipe développement — Avril 2026",
            "backlogLink": "https://link",
            "currentMonth": "Avril",
            "months": ["Janvier"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-10",
                "tracks": [{"name": "M1", "label": "Plateforme", "color": "#000", "milestones": []}]
            },
            "heatmap": {"shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {}}
        }
        """;
        var path = WriteRaw(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Tableau de bord exécutif", svc.Data!.Title);
        Assert.Equal("Équipe développement — Avril 2026", svc.Data.Subtitle);
    }

    #endregion
}