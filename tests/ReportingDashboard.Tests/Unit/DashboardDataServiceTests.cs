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
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private DashboardDataService CreateService()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(_tempDir);
        return new DashboardDataService(mockEnv.Object);
    }

    private void WriteDataJson(object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);
    }

    [Fact]
    public void GetError_WhenDataJsonMissing_ReturnsFileNotFoundMessage()
    {
        var service = CreateService();

        service.GetDashboardData().Should().BeNull();
        service.GetError().Should().Contain("data.json not found");
    }

    [Fact]
    public void GetDashboardData_WithValidJson_ReturnsPopulatedData()
    {
        WriteDataJson(new
        {
            title = "Test Dashboard",
            subtitle = "Test Subtitle",
            backlogUrl = "https://example.com",
            currentDate = "2026-04-14",
            months = new[] { "Jan", "Feb", "Mar" },
            tracks = new[]
            {
                new
                {
                    id = "M1",
                    label = "Track 1",
                    color = "#0078D4",
                    milestones = new[]
                    {
                        new { date = "2026-01-15", type = "Checkpoint", label = "Alpha" }
                    }
                }
            },
            statusRows = new
            {
                shipped = new Dictionary<string, List<string>> { ["Jan"] = new() { "Item A" } },
                inProgress = new Dictionary<string, List<string>>(),
                carryover = new Dictionary<string, List<string>>(),
                blockers = new Dictionary<string, List<string>>()
            }
        });

        var service = CreateService();

        var data = service.GetDashboardData();
        data.Should().NotBeNull();
        data!.Title.Should().Be("Test Dashboard");
        data.Subtitle.Should().Be("Test Subtitle");
        data.Months.Should().HaveCount(3);
        data.Tracks.Should().HaveCount(1);
        data.Tracks[0].Milestones.Should().HaveCount(1);
        data.StatusRows.Shipped.Should().ContainKey("Jan");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void GetError_WithMalformedJson_ReturnsParsErrorMessage()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{ invalid json }");

        var service = CreateService();

        service.GetDashboardData().Should().BeNull();
        service.GetError().Should().StartWith("Error parsing data.json:");
    }

    [Fact]
    public void Reload_AfterDataChange_ReturnsUpdatedData()
    {
        WriteDataJson(new { title = "Original Title" });
        var service = CreateService();
        service.GetDashboardData()!.Title.Should().Be("Original Title");

        WriteDataJson(new { title = "Updated Title" });
        service.Reload();

        service.GetDashboardData()!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public void GetDashboardData_WithPartialJson_UsesDefaults()
    {
        WriteDataJson(new { title = "Partial" });

        var service = CreateService();
        var data = service.GetDashboardData();

        data.Should().NotBeNull();
        data!.Title.Should().Be("Partial");
        data.Subtitle.Should().BeEmpty();
        data.Months.Should().BeEmpty();
        data.Tracks.Should().BeEmpty();
        data.StatusRows.Should().NotBeNull();
        service.GetError().Should().BeNull();
    }
}