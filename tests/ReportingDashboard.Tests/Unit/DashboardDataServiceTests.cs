using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_tempDir, "data"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private IWebHostEnvironment CreateMockEnv(string webRootPath)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(e => e.WebRootPath).Returns(webRootPath);
        return mock.Object;
    }

    private string BuildValidJson(
        string start = "2026-01-01",
        string end = "2026-06-30",
        string reportDate = "2026-04-15")
    {
        var data = new DashboardData(
            Title: "Project Phoenix",
            Subtitle: "Test Org",
            BacklogUrl: "https://example.com",
            ReportDate: reportDate,
            TimelineRange: new TimelineRange(
                Start: start,
                End: end,
                MonthGridlines: new List<MonthGridline>
                {
                    new("Jan", "2026-01-01"),
                    new("Feb", "2026-02-01")
                }),
            Workstreams: new List<Workstream>
            {
                new("M1", "M1 - Chatbot", "Desc", "#4285F4", new List<Milestone>
                {
                    new("2026-02-15", "PoC Complete", "poc")
                })
            },
            Heatmap: new HeatmapData(
                Title: "Monthly Execution Heatmap",
                Months: new List<string> { "January", "February" },
                CurrentMonth: "April",
                Rows: new List<HeatmapRow>())
        );
        return JsonSerializer.Serialize(data);
    }

    private void WriteDataJson(string json)
    {
        File.WriteAllText(Path.Combine(_tempDir, "data", "data.json"), json);
    }

    [Fact]
    public void ValidJson_LoadsDataWithoutError()
    {
        WriteDataJson(BuildValidJson());

        var service = new DashboardDataService(CreateMockEnv(_tempDir));

        service.Data.Should().NotBeNull();
        service.HasError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
        service.Data!.Title.Should().Be("Project Phoenix");
    }

    [Fact]
    public void MissingFile_SetsErrorMessage()
    {
        // Point to a directory with no data.json
        var emptyDir = Path.Combine(_tempDir, "empty");
        Directory.CreateDirectory(Path.Combine(emptyDir, "data"));

        var service = new DashboardDataService(CreateMockEnv(emptyDir));

        service.HasError.Should().BeTrue();
        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("Unable to load dashboard data");
    }

    [Fact]
    public void MalformedJson_SetsJsonParseError()
    {
        WriteDataJson("{ not valid json !!! }");

        var service = new DashboardDataService(CreateMockEnv(_tempDir));

        service.HasError.Should().BeTrue();
        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("JSON parse error");
    }

    [Fact]
    public void DateToX_CalculatesProportionalPosition()
    {
        // Range: 2026-01-01 to 2026-06-30 (180 days)
        WriteDataJson(BuildValidJson("2026-01-01", "2026-06-30", "2026-04-15"));

        var service = new DashboardDataService(CreateMockEnv(_tempDir));

        // Range start should map to 0
        service.DateToX(new DateOnly(2026, 1, 1)).Should().Be(0.0);

        // Range end should map to SvgWidth (1560)
        service.DateToX(new DateOnly(2026, 6, 30)).Should().BeApproximately(1560.0, 0.01);

        // Midpoint should be roughly half
        var midDate = new DateOnly(2026, 4, 1); // 90 days in
        var totalDays = (new DateOnly(2026, 6, 30).DayNumber - new DateOnly(2026, 1, 1).DayNumber);
        var dayOffset = midDate.DayNumber - new DateOnly(2026, 1, 1).DayNumber;
        var expected = ((double)dayOffset / totalDays) * 1560.0;
        service.DateToX(midDate).Should().BeApproximately(expected, 0.01);
    }

    [Fact]
    public void GetWorkstreamY_DistributesEvenly()
    {
        WriteDataJson(BuildValidJson());

        var service = new DashboardDataService(CreateMockEnv(_tempDir));

        // SvgHeight=185, TopReserved=28, usableHeight=157
        // With 3 workstreams: spacing = 157 / (3+1) = 39.25
        // index 0 => 28 + 39.25 * 1 = 67.25
        // index 1 => 28 + 39.25 * 2 = 106.5
        // index 2 => 28 + 39.25 * 3 = 145.75
        double usable = 185.0 - 28.0;
        double spacing = usable / 4.0;

        service.GetWorkstreamY(0, 3).Should().BeApproximately(28.0 + spacing, 0.01);
        service.GetWorkstreamY(1, 3).Should().BeApproximately(28.0 + spacing * 2, 0.01);
        service.GetWorkstreamY(2, 3).Should().BeApproximately(28.0 + spacing * 3, 0.01);

        // Values should be strictly increasing
        var y0 = service.GetWorkstreamY(0, 3);
        var y1 = service.GetWorkstreamY(1, 3);
        var y2 = service.GetWorkstreamY(2, 3);
        y1.Should().BeGreaterThan(y0);
        y2.Should().BeGreaterThan(y1);
    }
}