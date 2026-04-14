using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DataServiceTests_" + Guid.NewGuid().ToString("N"));
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private static string CreateValidJson(string title = "Test Dashboard", string? nowDateOverride = null)
    {
        var data = new
        {
            schemaVersion = 1,
            title,
            subtitle = "Test Subtitle",
            backlogUrl = "https://example.com",
            nowDateOverride,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-31",
                workstreams = new[]
                {
                    new
                    {
                        id = "ws1",
                        name = "Workstream 1",
                        color = "#0078D4",
                        milestones = new[]
                        {
                            new { label = "M1", date = "2026-03-01", type = "poc", labelPosition = (string?)null }
                        }
                    }
                }
            },
            heatmap = new
            {
                monthColumns = new[] { "Jan", "Feb", "Mar" },
                categories = new[]
                {
                    new
                    {
                        name = "Shipped",
                        emoji = "✅",
                        cssClass = "ship",
                        months = new[]
                        {
                            new { month = "Jan", items = new[] { "Item A" } }
                        }
                    }
                }
            }
        };
        return JsonSerializer.Serialize(data);
    }

    private void WriteDataJson(string content)
    {
        File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), content);
    }

    [Fact]
    public void ValidJson_LoadsCorrectly()
    {
        WriteDataJson(CreateValidJson("My Project"));

        using var service = new DataService(_envMock.Object);

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("My Project");
        service.GetData()!.SchemaVersion.Should().Be(1);
        service.GetData()!.Timeline.Workstreams.Should().HaveCount(1);
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void MissingFile_ReturnsError()
    {
        // Do not create data.json
        using var service = new DataService(_envMock.Object);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("No data.json found");
    }

    [Fact]
    public void MalformedJson_ReturnsError()
    {
        WriteDataJson("{ this is not valid json!!!");

        using var service = new DataService(_envMock.Object);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("invalid JSON");
    }

    [Fact]
    public void SchemaVersionMismatch_ReturnsError()
    {
        var json = CreateValidJson().Replace("\"schemaVersion\":1", "\"schemaVersion\":99");
        WriteDataJson(json);

        using var service = new DataService(_envMock.Object);

        service.GetError().Should().Contain("expected 1");
        service.GetError().Should().Contain("99");
    }

    [Fact]
    public void GetEffectiveDate_WithOverride_ReturnsParsedDate()
    {
        WriteDataJson(CreateValidJson(nowDateOverride: "2026-02-15"));

        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(new DateOnly(2026, 2, 15));
    }

    [Fact]
    public void GetEffectiveDate_WithoutOverride_ReturnsToday()
    {
        WriteDataJson(CreateValidJson(nowDateOverride: null));

        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }
}