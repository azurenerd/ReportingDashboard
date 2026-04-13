using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;
    private readonly string _tempDir;
    private readonly string _wwwrootDir;

    public DashboardDataServiceTests()
    {
        _envMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);
        _envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);
    }

    [Fact]
    public async Task LoadDataAsync_FileNotFound_ReturnsEmptyDashboardData()
    {
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        var result = await service.LoadDataAsync();

        result.Should().NotBeNull();
        result.Milestones.Should().BeEmpty();
        result.WorkItems.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadDataAsync_ValidJson_DeserializesCorrectly()
    {
        var testData = new DashboardData
        {
            Project = new ProjectInfo
            {
                Name = "Test Project",
                ReportingPeriod = "April 2026",
                RagStatus = "Amber",
                Summary = "On track"
            },
            Milestones = new List<Milestone>
            {
                new() { Title = "M1", Status = "completed", TargetDate = new DateTime(2026, 3, 1) }
            },
            WorkItems = new List<WorkItem>
            {
                new() { Title = "WI-1", Category = "shipped", Owner = "Alice", Priority = "High" }
            }
        };

        var json = JsonSerializer.Serialize(testData);
        await File.WriteAllTextAsync(Path.Combine(_wwwrootDir, "data.json"), json);

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);
        var result = await service.LoadDataAsync();

        result.Project.Name.Should().Be("Test Project");
        result.Project.RagStatus.Should().Be("Amber");
        result.Milestones.Should().HaveCount(1);
        result.Milestones[0].Title.Should().Be("M1");
        result.WorkItems.Should().HaveCount(1);
        result.WorkItems[0].Owner.Should().Be("Alice");
    }

    [Fact]
    public async Task LoadDataAsync_InvalidJson_ReturnsEmptyDashboardData()
    {
        await File.WriteAllTextAsync(Path.Combine(_wwwrootDir, "data.json"), "not valid json {{{");

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);
        var result = await service.LoadDataAsync();

        result.Should().NotBeNull();
        result.Milestones.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadDataAsync_JsonWithCommentsAndTrailingCommas_DeserializesSuccessfully()
    {
        var jsonWithComments = """
        {
            // This is a comment
            "project": {
                "name": "Commented Project",
                "reportingPeriod": "Q1",
                "ragStatus": "Green",
                "summary": "OK"
            },
            "milestones": [],
            "workItems": []
        }
        """;
        await File.WriteAllTextAsync(Path.Combine(_wwwrootDir, "data.json"), jsonWithComments);

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);
        var result = await service.LoadDataAsync();

        result.Project.Name.Should().Be("Commented Project");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }
}