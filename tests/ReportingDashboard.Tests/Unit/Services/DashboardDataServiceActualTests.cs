using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceActualTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataServiceActualTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashSvcTests_{Guid.NewGuid():N}");
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
        var path = Path.Combine(_tempDir, "data.json");
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        File.WriteAllText(path, json);
        return path;
    }

    private string WriteRawJson(string json)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    private object CreateValidData() => new
    {
        title = "Test Project",
        subtitle = "Team X - April 2026",
        backlogLink = "https://ado.example.com",
        currentMonth = "April",
        months = new[] { "January", "February", "March", "April" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new
                {
                    name = "M1",
                    label = "Core Platform",
                    color = "#4285F4",
                    milestones = new[]
                    {
                        new { date = "2026-02-15", type = "poc", label = "Feb 15" }
                    }
                }
            }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Feature A" } },
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        }
    };

    #region LoadAsync - Success

    [Fact]
    public async Task LoadAsync_ValidFile_SetsDataAndNoError()
    {
        var path = WriteJson(CreateValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_ValidFile_DeserializesTitle()
    {
        var path = WriteJson(CreateValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal("Test Project", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_ValidFile_DeserializesSubtitle()
    {
        var path = WriteJson(CreateValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal("Team X - April 2026", svc.Data!.Subtitle);
    }

    [Fact]
    public async Task LoadAsync_ValidFile_DeserializesMonths()
    {
        var path = WriteJson(CreateValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal(4, svc.Data!.Months.Count);
        Assert.Equal("January", svc.Data.Months[0]);
    }

    [Fact]
    public async Task LoadAsync_ValidFile_DeserializesTimeline()
    {
        var path = WriteJson(CreateValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal("2026-01-01", svc.Data!.Timeline.StartDate);
        Assert.Equal("2026-07-01", svc.Data.Timeline.EndDate);
        Assert.Equal("2026-04-10", svc.Data.Timeline.NowDate);
        Assert.Single(svc.Data.Timeline.Tracks);
    }

    [Fact]
    public async Task LoadAsync_ValidFile_DeserializesHeatmap()
    {
        var path = WriteJson(CreateValidData());
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Single(svc.Data!.Heatmap.Shipped);
        Assert.Contains("jan", svc.Data.Heatmap.Shipped.Keys);
    }

    #endregion

    #region LoadAsync - File Not Found

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsIsError()
    {
        var svc = CreateService();

        await svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ErrorMessageContainsPath()
    {
        var svc = CreateService();
        var fakePath = Path.Combine(_tempDir, "missing.json");

        await svc.LoadAsync(fakePath);

        Assert.NotNull(svc.ErrorMessage);
        Assert.Contains("not found", svc.ErrorMessage!);
        Assert.Contains(fakePath, svc.ErrorMessage);
    }

    #endregion

    #region LoadAsync - Invalid JSON

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsIsError()
    {
        var path = WriteRawJson("{ this is not valid json }");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ErrorMessageContainsParseInfo()
    {
        var path = WriteRawJson("{{{bad}}}");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.NotNull(svc.ErrorMessage);
        Assert.Contains("parse", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region LoadAsync - Null Deserialization

    [Fact]
    public async Task LoadAsync_NullJson_SetsIsError()
    {
        var path = WriteRawJson("null");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
        Assert.NotNull(svc.ErrorMessage);
    }

    #endregion

    #region LoadAsync - Validation Errors

    [Fact]
    public async Task LoadAsync_MissingTitle_SetsValidationError()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "Sub",
            months = new[] { "Jan" },
            timeline = new { tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MissingSubtitle_SetsValidationError()
    {
        var path = WriteJson(new
        {
            title = "Valid Title",
            subtitle = "",
            months = new[] { "Jan" },
            timeline = new { tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_EmptyMonths_SetsValidationError()
    {
        var path = WriteJson(new
        {
            title = "Valid",
            subtitle = "Valid Sub",
            months = Array.Empty<string>(),
            timeline = new { tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("months", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_EmptyTracks_SetsValidationError()
    {
        var path = WriteJson(new
        {
            title = "Valid",
            subtitle = "Valid Sub",
            months = new[] { "Jan" },
            timeline = new { tracks = Array.Empty<object>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("tracks", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_MultipleValidationErrors_AllReported()
    {
        var path = WriteJson(new
        {
            title = "",
            subtitle = "",
            months = Array.Empty<string>(),
            timeline = new { tracks = Array.Empty<object>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("subtitle", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyTitle_SetsValidationError()
    {
        var path = WriteJson(new
        {
            title = "   ",
            subtitle = "Valid",
            months = new[] { "Jan" },
            timeline = new { tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } } }
        });
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("title", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region LoadAsync - Edge Cases

    [Fact]
    public async Task LoadAsync_EmptyJsonObject_SetsValidationError()
    {
        var path = WriteRawJson("{}");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_EmptyFilePath_SetsIsError()
    {
        var svc = CreateService();

        await svc.LoadAsync("");

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_CalledTwice_SecondCallOverwritesFirst()
    {
        var path1 = WriteJson(CreateValidData());
        var svc = CreateService();
        await svc.LoadAsync(path1);
        Assert.False(svc.IsError);
        Assert.Equal("Test Project", svc.Data!.Title);

        // Now load invalid - should overwrite
        var path2 = WriteRawJson("{ bad json");
        await svc.LoadAsync(path2);
        Assert.True(svc.IsError);
    }

    #endregion
}