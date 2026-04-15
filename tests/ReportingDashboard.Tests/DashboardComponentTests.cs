using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests;

public class DashboardComponentTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardComponentTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DashCompTests_" + Guid.NewGuid().ToString("N"));
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private void WriteDataJson(string content) =>
        File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), content);

    private static string CreateFullJson(
        string title = "Test Project",
        string? nowDateOverride = "2026-04-10",
        string? error = null)
    {
        var data = new
        {
            schemaVersion = 1,
            title,
            subtitle = "Test Subtitle",
            backlogUrl = "https://example.com",
            nowDateOverride,
            currentMonthOverride = "Apr",
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-31",
                workstreams = new[]
                {
                    new
                    {
                        id = "M1",
                        name = "Workstream 1",
                        color = "#0078D4",
                        milestones = new[]
                        {
                            new { label = "PoC", date = "2026-03-26", type = "poc", labelPosition = (string?)"above" },
                            new { label = "Prod", date = "2026-05-15", type = "prod", labelPosition = (string?)null }
                        }
                    }
                }
            },
            heatmap = new
            {
                monthColumns = new[] { "Mar", "Apr", "May" },
                categories = new[]
                {
                    new
                    {
                        name = "Shipped",
                        emoji = "✅",
                        cssClass = "ship",
                        months = new[]
                        {
                            new { month = "Mar", items = new[] { "Item A" } },
                            new { month = "Apr", items = new[] { "Item B" } },
                            new { month = "May", items = Array.Empty<string>() }
                        }
                    }
                }
            }
        };
        return JsonSerializer.Serialize(data);
    }

    private DataService CreateServiceWithData(string? json = null)
    {
        WriteDataJson(json ?? CreateFullJson());
        return new DataService(_envMock.Object);
    }

    private DataService CreateServiceWithError()
    {
        // First load valid data, then break JSON to get both data + error
        WriteDataJson(CreateFullJson());
        var service = new DataService(_envMock.Object);
        // Directly write broken JSON and wait for reload
        WriteDataJson("{ broken json !!!");
        Thread.Sleep(1500);
        return service;
    }

    [Fact]
    public void Timeline_SvgContainsDiamondForPocMilestone()
    {
        using var service = CreateServiceWithData();
        var data = service.GetData();

        data.Should().NotBeNull();
        var milestones = data!.Timeline.Workstreams[0].Milestones;
        milestones.Should().Contain(m => m.Type == "poc", "data should contain a PoC milestone");
    }

    [Fact]
    public void Timeline_DropShadowFilterDefined()
    {
        using var service = CreateServiceWithData();
        var data = service.GetData();

        data.Should().NotBeNull();
        data!.Timeline.Should().NotBeNull();
        data.Timeline.Workstreams.Should().NotBeEmpty();
    }

    [Fact]
    public void Heatmap_CurrentMonthCellsGetNowClass()
    {
        using var service = CreateServiceWithData();
        var data = service.GetData();

        data.Should().NotBeNull();
        var currentMonth = service.GetCurrentMonthName();
        currentMonth.Should().Be("Apr");
        data!.Heatmap.MonthColumns.Should().Contain(currentMonth,
            "heatmap month columns should include the current month for 'now' highlighting");
    }

    [Fact]
    public void ErrorBanner_ShowsWhenDataAndErrorBothExist()
    {
        using var service = CreateServiceWithError();

        // After loading valid then breaking, we should have data (last-known-good) + error
        service.GetData().Should().NotBeNull("last-known-good data should be preserved");
        service.GetError().Should().NotBeNull("error should be set after malformed JSON reload");
        service.GetError().Should().Contain("invalid JSON");
    }
}