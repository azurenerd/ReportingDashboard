using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceEdgeCaseTests : IDisposable
{
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;
    private readonly string _tempDir;

    public DashboardDataServiceEdgeCaseTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardEdge_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteFile(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private static string GetMinimalValidJson(string title = "Test") => $$"""
    {
        "title": "{{title}}",
        "subtitle": "Test Subtitle",
        "backlogLink": "https://test.com",
        "currentMonth": "Jan",
        "months": ["Jan"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
        },
        "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
    }
    """;

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyJson_SetsIsError()
    {
        var path = WriteFile("data.json", "   \n\t  ");

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonWithBom_DeserializesCorrectly()
    {
        var bom = "\uFEFF";
        var json = bom + GetMinimalValidJson("BOM Test");
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("BOM Test");
    }

    [Fact]
    public async Task LoadAsync_UnicodeContent_DeserializesCorrectly()
    {
        var json = """
        {
            "title": "日本語テスト",
            "subtitle": "Ñoño",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockLogger.Object);
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
            "subtitle": "Test Subtitle",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
            },
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

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.Data!.Heatmap.Shipped.Should().HaveCount(4);
        service.Data.Heatmap.Shipped["apr"].Should().HaveCount(4);
        service.Data.Heatmap.InProgress["jan"].Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_MissingRequiredFields_SetsValidationError()
    {
        // Only timeline provided, missing title/subtitle/etc. - fails validation
        var json = """
        {
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-01",
                "tracks": [
                    { "name": "Track", "label": "T1", "color": "#000", "milestones": [] }
                ]
            }
        }
        """;
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("title");
    }

    [Fact]
    public async Task LoadAsync_VeryLongTitle_HandlesGracefully()
    {
        var longTitle = new string('A', 10000);
        var json = $$"""
        {
            "title": "{{longTitle}}",
            "subtitle": "Test Subtitle",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().HaveLength(10000);
    }

    [Fact]
    public async Task LoadAsync_NumericValueForStringField_SetsIsError()
    {
        var json = """{ "title": 12345 }""";
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public void Constructor_InitializesCleanState()
    {
        // Properties are not virtual by design - use real instances, not mocks
        var service = new DashboardDataService(_mockLogger.Object);

        service.Data.Should().BeNull();
        service.IsError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_JsonWithComments_AcceptedByParser()
    {
        // PR #595 uses ReadCommentHandling = JsonCommentHandling.Skip
        var json = """
        {
            // This is a comment
            "title": "Test",
            "subtitle": "Test Subtitle",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteFile("data.json", json);

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeFalse();
        service.Data!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task LoadAsync_JsonPrimitiveString_SetsIsError()
    {
        var path = WriteFile("data.json", "\"just a string\"");

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonNumber_SetsIsError()
    {
        var path = WriteFile("data.json", "42");

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonBoolean_SetsIsError()
    {
        var path = WriteFile("data.json", "true");

        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }
}