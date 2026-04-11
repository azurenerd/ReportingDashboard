using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Focused tests on DashboardDataService error state properties
/// specifically related to the ErrorPanel component integration:
/// IsError, ErrorMessage content, and Data nullity in various scenarios.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceErrorStateTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataServiceErrorStateTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashErrState_{Guid.NewGuid():N}");
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
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private string WriteTempJson(string content)
    {
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    #region Initial State (Never Loaded)

    [Fact]
    public void NewService_IsErrorIsFalse()
    {
        var svc = CreateService();
        Assert.False(svc.IsError);
    }

    [Fact]
    public void NewService_DataIsNull()
    {
        var svc = CreateService();
        Assert.Null(svc.Data);
    }

    [Fact]
    public void NewService_ErrorMessageIsNull()
    {
        var svc = CreateService();
        Assert.Null(svc.ErrorMessage);
    }

    #endregion

    #region File Not Found Error Messages

    [Fact]
    public async Task FileNotFound_ErrorMessageContainsFilePath()
    {
        var svc = CreateService();
        var missingPath = Path.Combine(_tempDir, "missing_data.json");

        await svc.LoadAsync(missingPath);

        Assert.True(svc.IsError);
        Assert.Contains(missingPath, svc.ErrorMessage!);
    }

    [Fact]
    public async Task FileNotFound_ErrorMessageContainsNotFound()
    {
        var svc = CreateService();

        await svc.LoadAsync(Path.Combine(_tempDir, "absent.json"));

        Assert.Contains("not found", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FileNotFound_DataRemainsNull()
    {
        var svc = CreateService();

        await svc.LoadAsync(Path.Combine(_tempDir, "absent.json"));

        Assert.Null(svc.Data);
    }

    #endregion

    #region Parse Error Messages

    [Fact]
    public async Task MalformedJson_ErrorMessageContainsParse()
    {
        var path = WriteTempJson("{ invalid json ??? }");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("parse", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MalformedJson_DataRemainsNull()
    {
        var path = WriteTempJson("not even json");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task TruncatedJson_SetsError()
    {
        var path = WriteTempJson("{\"title\":\"test\"");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task JsonArray_SetsError()
    {
        var path = WriteTempJson("[1, 2, 3]");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task NullLiteral_SetsError()
    {
        var path = WriteTempJson("null");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    #endregion

    #region Validation Error Messages

    [Fact]
    public async Task EmptyTitle_ErrorMentionsTitle()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts);
        var path = WriteTempJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmptyMonths_SetsError()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = Array.Empty<string>(),
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts);
        var path = WriteTempJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("months", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmptyTracks_SetsError()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = Array.Empty<object>() },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts);
        var path = WriteTempJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("track", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region ErrorMessage Suitability for UI Display

    [Fact]
    public async Task FileNotFound_ErrorMessageIsHumanReadable()
    {
        var svc = CreateService();
        await svc.LoadAsync(Path.Combine(_tempDir, "data.json"));

        // Should not contain raw exception type names
        Assert.DoesNotContain("FileNotFoundException", svc.ErrorMessage!);
        Assert.DoesNotContain("System.IO", svc.ErrorMessage!);
    }

    [Fact]
    public async Task ParseError_ErrorMessageDoesNotLeakStackTrace()
    {
        var path = WriteTempJson("{{bad}}");
        var svc = CreateService();
        await svc.LoadAsync(path);

        Assert.DoesNotContain("at System.", svc.ErrorMessage!);
        Assert.DoesNotContain("StackTrace", svc.ErrorMessage!);
    }

    [Fact]
    public async Task ErrorMessage_IsNotExcessivelyLong()
    {
        var path = WriteTempJson("{ invalid }");
        var svc = CreateService();
        await svc.LoadAsync(path);

        // Error messages should be reasonable length for UI display
        Assert.True(svc.ErrorMessage!.Length < 1000,
            $"Error message too long for UI display: {svc.ErrorMessage.Length} chars");
    }

    #endregion

    #region State Consistency

    [Fact]
    public async Task ErrorState_IsErrorAndErrorMessageAreConsistent()
    {
        var svc = CreateService();
        await svc.LoadAsync(Path.Combine(_tempDir, "nope.json"));

        // When IsError is true, ErrorMessage should not be null or empty
        Assert.True(svc.IsError);
        Assert.False(string.IsNullOrEmpty(svc.ErrorMessage));
    }

    [Fact]
    public async Task SuccessState_IsErrorAndDataAreConsistent()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts);
        var path = WriteTempJson(json);
        var svc = CreateService();
        await svc.LoadAsync(path);

        // When IsError is false, Data should not be null
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task ErrorState_DataIsNull()
    {
        var svc = CreateService();
        await svc.LoadAsync(Path.Combine(_tempDir, "nope.json"));

        // When in error state, Data should be null
        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    #endregion
}