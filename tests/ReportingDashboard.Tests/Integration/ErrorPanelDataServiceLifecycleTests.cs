using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the DashboardDataService lifecycle as it relates to error panel display.
/// Tests the full file → service → error state pipeline including concurrent access,
/// reload scenarios, and error message format suitability for UI rendering.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorPanelDataServiceLifecycleTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public ErrorPanelDataServiceLifecycleTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ErrLifecycle_{Guid.NewGuid():N}");
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

    private string WriteValidJson(string filename = "data.json")
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "Lifecycle Test",
            subtitle = "Team",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path = Path.Combine(_tempDir, filename);
        File.WriteAllText(path, json);
        return path;
    }

    #region Singleton Service State Consistency

    [Fact]
    public async Task SingletonService_ErrorStateAvailableToMultipleConsumers()
    {
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));

        // Multiple reads should return consistent state (singleton pattern)
        for (int i = 0; i < 10; i++)
        {
            Assert.True(svc.IsError);
            Assert.NotNull(svc.ErrorMessage);
            Assert.Null(svc.Data);
        }
    }

    [Fact]
    public async Task SingletonService_ValidStateAvailableToMultipleConsumers()
    {
        var path = WriteValidJson();
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        for (int i = 0; i < 10; i++)
        {
            Assert.False(svc.IsError);
            Assert.NotNull(svc.Data);
            Assert.Equal("Lifecycle Test", svc.Data!.Title);
        }
    }

    #endregion

    #region Error Message Format for UI Display

    [Fact]
    public async Task FileNotFound_ErrorMessageIsUserFriendly()
    {
        var svc = new DashboardDataService(_logger);
        var missingPath = Path.Combine(_tempDir, "data.json");
        await svc.LoadAsync(missingPath);

        var msg = svc.ErrorMessage!;

        // Should contain the file path so user knows which file is missing
        Assert.Contains(missingPath, msg);
        // Should not contain raw exception type
        Assert.DoesNotContain("System.IO.FileNotFoundException", msg);
        // Should be reasonable length for display
        Assert.True(msg.Length < 500, $"Error message too long: {msg.Length}");
    }

    [Fact]
    public async Task ParseError_ErrorMessageIsUserFriendly()
    {
        var path = Path.Combine(_tempDir, "parse.json");
        File.WriteAllText(path, "{ broken: true, }}}");
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var msg = svc.ErrorMessage!;

        Assert.Contains("parse", msg, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.Text.Json.JsonException", msg);
        Assert.DoesNotContain("at System.", msg);
    }

    [Fact]
    public async Task ValidationError_ErrorMessageIsActionable()
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
        var path = Path.Combine(_tempDir, "val.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var msg = svc.ErrorMessage!;

        // Should tell user which field is the problem
        Assert.Contains("title", msg, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Various Malformed JSON Formats

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    [InlineData("null")]
    [InlineData("true")]
    [InlineData("42")]
    [InlineData("\"just a string\"")]
    [InlineData("[1, 2, 3]")]
    [InlineData("{")]
    [InlineData("}{")]
    public async Task MalformedInput_AllProduceErrorState(string content)
    {
        var path = Path.Combine(_tempDir, $"test_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.True(svc.IsError, $"Expected error for content: '{content}'");
        Assert.NotNull(svc.ErrorMessage);
        Assert.False(string.IsNullOrWhiteSpace(svc.ErrorMessage));
    }

    #endregion

    #region Rapid State Transitions

    [Fact]
    public async Task RapidTransitions_FinalStateIsCorrect()
    {
        var svc = new DashboardDataService(_logger);
        var validPath = WriteValidJson("rapid.json");

        // Rapid error → valid → error → valid
        await svc.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        Assert.True(svc.IsError);

        await svc.LoadAsync(validPath);
        Assert.False(svc.IsError);

        File.WriteAllText(validPath, "broken");
        await svc.LoadAsync(validPath);
        Assert.True(svc.IsError);

        File.WriteAllText(validPath, JsonSerializer.Serialize(new
        {
            title = "Final",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "Jan",
            months = new[] { "January" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10", tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts));
        await svc.LoadAsync(validPath);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Final", svc.Data!.Title);
    }

    #endregion
}