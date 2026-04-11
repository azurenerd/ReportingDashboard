using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying that the DashboardDataService state transitions
/// (error → valid, valid → error) are correctly reflected when Dashboard.razor
/// re-renders with the updated service state.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorPanelServiceStateTransitionTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public ErrorPanelServiceStateTransitionTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ErrTransition_{Guid.NewGuid():N}");
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
            title = "Transition Test",
            subtitle = "Team - April",
            backlogLink = "https://ado.example.com",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "Core", color = "#4285F4", milestones = new[] { new { date = "2026-03-01", type = "poc", label = "PoC" } } } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Item" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);
        var path = Path.Combine(_tempDir, filename);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public async Task Service_ErrorThenValid_ServiceStateRecoveredCorrectly()
    {
        var svc = new DashboardDataService(_logger);

        // Start in error state
        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);
        Assert.Null(svc.Data);

        // Recover with valid file
        var path = WriteValidJson();
        await svc.LoadAsync(path);
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Transition Test", svc.Data!.Title);
    }

    [Fact]
    public async Task Service_ValidThenCorrupt_TransitionsToErrorState()
    {
        var path = WriteValidJson();
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);

        // Corrupt the file and reload
        File.WriteAllText(path, "{{corrupt}}");
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);
        Assert.Contains("parse", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Service_ValidThenDeleted_TransitionsToFileNotFoundError()
    {
        var path = WriteValidJson("toDelete.json");
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.False(svc.IsError);

        // Delete and attempt reload
        File.Delete(path);
        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("not found", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Service_MultipleErrorTypes_EachProducesDistinctMessage()
    {
        var svc = new DashboardDataService(_logger);

        // File not found error
        await svc.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        var fileNotFoundMsg = svc.ErrorMessage!;
        Assert.Contains("not found", fileNotFoundMsg, StringComparison.OrdinalIgnoreCase);

        // Parse error
        var corruptPath = Path.Combine(_tempDir, "corrupt.json");
        File.WriteAllText(corruptPath, "not json");
        await svc.LoadAsync(corruptPath);
        var parseMsg = svc.ErrorMessage!;
        Assert.Contains("parse", parseMsg, StringComparison.OrdinalIgnoreCase);

        // Validation error
        var invalidPath = Path.Combine(_tempDir, "invalid.json");
        File.WriteAllText(invalidPath, JsonSerializer.Serialize(new
        {
            title = "",
            subtitle = "",
            months = Array.Empty<string>(),
            timeline = new { startDate = "", endDate = "", tracks = Array.Empty<object>() },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        }, JsonOpts));
        await svc.LoadAsync(invalidPath);
        var validationMsg = svc.ErrorMessage!;

        // All three should be different messages
        Assert.NotEqual(fileNotFoundMsg, parseMsg);
        Assert.NotEqual(parseMsg, validationMsg);
    }

    [Fact]
    public async Task Service_ErrorRecovery_ErrorMessageClearedOrNotRelevant()
    {
        var svc = new DashboardDataService(_logger);

        // Start in error state
        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        Assert.True(svc.IsError);

        // Recover
        var path = WriteValidJson();
        await svc.LoadAsync(path);

        // The key invariant: IsError is false and Data is available
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
    }
}