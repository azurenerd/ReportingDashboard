using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying how various nowDate and startDate values
/// from data.json flow through DashboardDataService deserialization and
/// are available for the Header component's NowLabel logic.
/// Tests the data layer contract for timeline date fields that affect
/// NowLabel rendering.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderTimelineNowDateVariationsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public HeaderTimelineNowDateVariationsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"NowDateVar_{Guid.NewGuid():N}");
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

    private object BuildData(string nowDate = "2026-04-10", string startDate = "2026-01-01",
        string endDate = "2026-07-01", string currentMonth = "April") => new
    {
        title = "Timeline Dates Test",
        subtitle = "Sub",
        backlogLink = "https://link",
        currentMonth,
        months = new[] { "April" },
        timeline = new
        {
            startDate,
            endDate,
            nowDate,
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

    #region NowDate Deserialization Variations

    [Theory]
    [InlineData("2026-04-10")]
    [InlineData("2026-12-31")]
    [InlineData("2025-01-01")]
    [InlineData("2030-06-15")]
    public async Task LoadAsync_VariousNowDates_DeserializesCorrectly(string nowDate)
    {
        var path = WriteJson(BuildData(nowDate: nowDate));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(nowDate, svc.Data!.Timeline!.NowDate);
    }

    [Fact]
    public async Task LoadAsync_EmptyNowDate_DeserializesAsEmptyString()
    {
        var path = WriteJson(BuildData(nowDate: ""));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("", svc.Data!.Timeline!.NowDate);
    }

    [Fact]
    public async Task LoadAsync_NowDateWithTimeComponent_PreservedAsIs()
    {
        var path = WriteJson(BuildData(nowDate: "2026-04-10T14:30:00"));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("2026-04-10T14:30:00", svc.Data!.Timeline!.NowDate);
    }

    [Fact]
    public async Task LoadAsync_NonDateNowDate_StillDeserializes()
    {
        var path = WriteJson(BuildData(nowDate: "not-a-date"));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        // Service should still load - NowDate is a string field, not validated as date
        Assert.False(svc.IsError);
        Assert.Equal("not-a-date", svc.Data!.Timeline!.NowDate);
    }

    #endregion

    #region StartDate Deserialization Variations

    [Theory]
    [InlineData("2026-01-01")]
    [InlineData("2025-06-15")]
    [InlineData("2028-03-01")]
    public async Task LoadAsync_VariousStartDates_DeserializesCorrectly(string startDate)
    {
        var path = WriteJson(BuildData(startDate: startDate));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(startDate, svc.Data!.Timeline!.StartDate);
    }

    #endregion

    #region CurrentMonth + NowDate Combined Verification

    [Theory]
    [InlineData("January", "2026-01-15")]
    [InlineData("February", "2026-02-28")]
    [InlineData("March", "2026-03-01")]
    [InlineData("December", "2026-12-31")]
    public async Task LoadAsync_CurrentMonthAndNowDate_BothAvailableForNowLabel(
        string currentMonth, string nowDate)
    {
        var path = WriteJson(BuildData(currentMonth: currentMonth, nowDate: nowDate));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(currentMonth, svc.Data!.CurrentMonth);
        Assert.Equal(nowDate, svc.Data!.Timeline!.NowDate);

        // Verify both are parseable for the NowLabel logic
        Assert.True(DateTime.TryParse(nowDate, out var parsed));
        Assert.True(parsed.Year > 2000);
    }

    [Fact]
    public async Task LoadAsync_NowDateMismatchesCurrentMonth_BothPreservedAsIs()
    {
        // currentMonth says "April" but nowDate is in December
        var path = WriteJson(BuildData(currentMonth: "April", nowDate: "2026-12-15"));
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("April", svc.Data!.CurrentMonth);
        Assert.Equal("2026-12-15", svc.Data!.Timeline!.NowDate);
    }

    #endregion

    #region File Reload With Different NowDate

    [Fact]
    public async Task LoadAsync_ReloadWithDifferentNowDate_ReflectsNewValue()
    {
        var svc = new DashboardDataService(_logger);

        var path1 = WriteJson(BuildData(nowDate: "2026-04-10"));
        await svc.LoadAsync(path1);
        Assert.Equal("2026-04-10", svc.Data!.Timeline!.NowDate);

        var path2 = WriteJson(BuildData(nowDate: "2027-08-20"));
        await svc.LoadAsync(path2);
        Assert.Equal("2027-08-20", svc.Data!.Timeline!.NowDate);
    }

    [Fact]
    public async Task LoadAsync_ReloadFromValidNowDateToEmpty_StillLoads()
    {
        var svc = new DashboardDataService(_logger);

        var path1 = WriteJson(BuildData(nowDate: "2026-04-10"));
        await svc.LoadAsync(path1);
        Assert.Equal("2026-04-10", svc.Data!.Timeline!.NowDate);

        var path2 = WriteJson(BuildData(nowDate: ""));
        await svc.LoadAsync(path2);
        Assert.Equal("", svc.Data!.Timeline!.NowDate);
    }

    #endregion
}