using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceEdgeCaseTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;
    private readonly string _tempDir;

    public DashboardDataServiceEdgeCaseTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardEdge_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);

        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string WriteFile(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyJson_SetsIsError()
    {
        var path = WriteFile("data.json", "   \n\t  ");

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonWithBom_DeserializesCorrectly()
    {
        var bom = "\uFEFF";
        var json = bom + """{ "title": "BOM Test" }""";
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("BOM Test");
    }

    [Fact]
    public async Task LoadAsync_UnicodeContent_DeserializesCorrectly()
    {
        var json = """{ "title": "日本語テスト", "subtitle": "Ñoño" }""";
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("日本語テスト");
        service.Data.Subtitle.Should().Be("Ñoño");
    }

    [Fact]
    public async Task LoadAsync_DeeplyNestedHeatmap_DeserializesAllKeys()
    {
        var json = """
        {
            "title": "Nested",
            "heatmap": {
                "shipped": {
                    "jan": ["a", "b", "c"],
                    "feb": ["d"],
                    "mar": ["e", "f"],
                    "apr": ["g", "h", "i", "j"]
                },
                "inProgress": { "jan": [], "feb": [], "mar": [], "apr": [] },
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.Data!.Heatmap.Shipped.Should().HaveCount(4);
        service.Data.Heatmap.Shipped["apr"].Should().HaveCount(4);
        service.Data.Heatmap.InProgress["jan"].Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_OnlyTimeline_OtherFieldsGetDefaults()
    {
        var json = """
        {
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-01",
                "tracks": [
                    { "id": "M1", "name": "Track", "color": "#000", "milestones": [] }
                ]
            }
        }
        """;
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().BeEmpty();
        service.Data.Months.Should().BeEmpty();
        service.Data.Timeline.Tracks.Should().HaveCount(1);
    }

    [Fact]
    public async Task LoadAsync_VeryLongTitle_HandlesGracefully()
    {
        var longTitle = new string('A', 10000);
        var json = $$"""{ "title": "{{longTitle}}" }""";
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().HaveLength(10000);
    }

    [Fact]
    public async Task LoadAsync_NumericValueForStringField_SetsIsError()
    {
        var json = """{ "title": 12345 }""";
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        // System.Text.Json may throw or coerce depending on settings
        // With PropertyNameCaseInsensitive, it should throw JsonException
        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_PropertiesAreVirtual_CanBeMocked()
    {
        var mock = new Mock<DashboardDataService>(_mockEnv.Object, _mockLogger.Object);
        mock.Setup(s => s.Data).Returns(new ReportingDashboard.Models.DashboardData { Title = "Mocked" });
        mock.Setup(s => s.IsError).Returns(false);

        mock.Object.Data!.Title.Should().Be("Mocked");
        mock.Object.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_JsonWithComments_SetsIsError()
    {
        var json = """
        {
            // This is a comment
            "title": "Test"
        }
        """;
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonPrimitiveString_SetsIsError()
    {
        var path = WriteFile("data.json", "\"just a string\"");

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonNumber_SetsIsError()
    {
        var path = WriteFile("data.json", "42");

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonBoolean_SetsIsError()
    {
        var path = WriteFile("data.json", "true");

        var service = new DashboardDataService(_mockEnv.Object, _mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }
}