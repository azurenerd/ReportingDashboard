using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DashboardDataServiceIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;

    public DashboardDataServiceIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_integ_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_tempDir);

        _loggerMock = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private async Task WriteJsonFile(string filename, string content)
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, filename), content);
    }

    private static string CreateFullDataJson() => """
    {
        "title": "Executive Reporting Dashboard",
        "subtitle": "Cloud Platform Team · Azure Workstream · April 2026",
        "backlogLink": "https://dev.azure.com/org/project/_backlogs",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Core Platform",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-15", "label": "Feb 15", "type": "poc" },
                        { "date": "2026-04-01", "label": "Apr 1", "type": "production" },
                        { "date": "2026-03-01", "label": "Mar 1", "type": "checkpoint" }
                    ]
                },
                {
                    "id": "M2",
                    "name": "API Gateway",
                    "color": "#34A853",
                    "milestones": [
                        { "date": "2026-03-15", "label": "Mar 15", "type": "poc" },
                        { "date": "2026-05-30", "label": "May 30", "type": "production" }
                    ]
                },
                {
                    "id": "M3",
                    "name": "UX Refresh",
                    "color": "#EA4335",
                    "milestones": [
                        { "date": "2026-04-15", "label": "Apr 15", "type": "checkpoint" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": {
                "Jan": ["Auth service v2", "Logging pipeline"],
                "Feb": ["Config management"],
                "Mar": ["Rate limiting", "Circuit breaker"],
                "Apr": ["Dashboard MVP"]
            },
            "inProgress": {
                "Mar": ["API docs"],
                "Apr": ["Perf testing", "Load balancer config"]
            },
            "carryover": {
                "Feb": ["Legacy migration"],
                "Mar": ["Legacy migration"],
                "Apr": ["Legacy migration"]
            },
            "blockers": {
                "Mar": ["Vendor SDK delay"],
                "Apr": ["License approval pending"]
            }
        }
    }
    """;

    [Fact]
    public async Task LoadAsync_RealJsonFile_ShouldDeserializeCompleteDataGraph()
    {
        await WriteJsonFile("data.json", CreateFullDataJson());
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();

        var data = service.Data!;
        data.Title.Should().Be("Executive Reporting Dashboard");
        data.Subtitle.Should().Contain("Cloud Platform Team");
        data.BacklogLink.Should().StartWith("https://");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
    }

    [Fact]
    public async Task LoadAsync_RealJsonFile_TimelineTracksFullyPopulated()
    {
        await WriteJsonFile("data.json", CreateFullDataJson());
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        var timeline = service.Data!.Timeline;
        timeline.StartDate.Should().Be("2026-01-01");
        timeline.EndDate.Should().Be("2026-06-30");
        timeline.NowDate.Should().Be("2026-04-10");
        timeline.Tracks.Should().HaveCount(3);

        var track1 = timeline.Tracks[0];
        track1.Id.Should().Be("M1");
        track1.Name.Should().Be("Core Platform");
        track1.Color.Should().Be("#0078D4");
        track1.Milestones.Should().HaveCount(3);
        track1.Milestones.Should().Contain(m => m.Type == "poc");
        track1.Milestones.Should().Contain(m => m.Type == "production");
        track1.Milestones.Should().Contain(m => m.Type == "checkpoint");
    }

    [Fact]
    public async Task LoadAsync_RealJsonFile_HeatmapCategoriesFullyPopulated()
    {
        await WriteJsonFile("data.json", CreateFullDataJson());
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        var heatmap = service.Data!.Heatmap;
        heatmap.Shipped.Should().HaveCount(4);
        heatmap.Shipped["Jan"].Should().HaveCount(2);
        heatmap.Shipped["Apr"].Should().ContainSingle().Which.Should().Be("Dashboard MVP");

        heatmap.InProgress.Should().HaveCount(2);
        heatmap.InProgress["Apr"].Should().HaveCount(2);

        heatmap.Carryover.Should().HaveCount(3);
        heatmap.Carryover["Apr"].Should().ContainSingle().Which.Should().Contain("Legacy");

        heatmap.Blockers.Should().HaveCount(2);
        heatmap.Blockers["Apr"].Should().ContainSingle().Which.Should().Contain("License");
    }

    [Fact]
    public async Task LoadAsync_CustomPathToExistingFile_ShouldLoadSuccessfully()
    {
        var subDir = Path.Combine(_tempDir, "custom", "path");
        Directory.CreateDirectory(subDir);
        var filePath = Path.Combine(subDir, "dashboard-data.json");
        await File.WriteAllTextAsync(filePath, CreateFullDataJson());

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync(filePath);

        service.IsError.Should().BeFalse();
        service.Data!.Title.Should().Be("Executive Reporting Dashboard");
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ShouldSetErrorWithPath()
    {
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);
        var missingPath = Path.Combine(_tempDir, "nonexistent.json");

        await service.LoadAsync(missingPath);

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("not found");
        service.ErrorMessage.Should().Contain(missingPath);
        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_CorruptJson_ShouldSetParseError()
    {
        await WriteJsonFile("data.json", "{ invalid json content !!! }");
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("Failed to parse data.json");
        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ShouldSetError()
    {
        await WriteJsonFile("data.json", "");
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_NullJsonLiteral_ShouldSetError()
    {
        await WriteJsonFile("data.json", "null");
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("empty or could not be parsed");
    }

    [Fact]
    public async Task LoadAsync_MinimalValidJson_ShouldLoadWithDefaults()
    {
        await WriteJsonFile("data.json", "{}");
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().BeEmpty();
        service.Data.Months.Should().BeEmpty();
        service.Data.Timeline.Tracks.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_JsonWithExtraFields_ShouldIgnoreAndLoad()
    {
        var json = """
        {
            "title": "Test",
            "extraField1": "ignored",
            "extraField2": 42,
            "nested": { "also": "ignored" }
        }
        """;
        await WriteJsonFile("data.json", json);
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task LoadAsync_JsonWithMixedCase_ShouldDeserializeCaseInsensitively()
    {
        var json = """
        {
            "Title": "Mixed Case Title",
            "SUBTITLE": "UPPER",
            "backloglink": "https://example.com"
        }
        """;
        await WriteJsonFile("data.json", json);
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data!.Title.Should().Be("Mixed Case Title");
    }

    [Fact]
    public async Task LoadAsync_LargeDataset_ShouldHandleManyTracks()
    {
        var tracks = string.Join(",\n", Enumerable.Range(1, 50).Select(i => $$"""
            {
                "id": "M{{i}}",
                "name": "Track {{i}}",
                "color": "#0078D4",
                "milestones": [
                    { "date": "2026-03-01", "label": "MS {{i}}", "type": "poc" }
                ]
            }
        """));

        var json = $$"""
        {
            "title": "Large Dataset Test",
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-12-31",
                "nowDate": "2026-06-15",
                "tracks": [{{tracks}}]
            }
        }
        """;
        await WriteJsonFile("data.json", json);
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data!.Timeline.Tracks.Should().HaveCount(50);
    }

    [Fact]
    public async Task LoadAsync_LargeHeatmap_ShouldHandleManyItemsPerMonth()
    {
        var items = string.Join(", ", Enumerable.Range(1, 100).Select(i => $"\"Item {i}\""));
        var json = $$"""
        {
            "title": "Many Items",
            "heatmap": {
                "shipped": { "Jan": [{{items}}] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        await WriteJsonFile("data.json", json);
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data!.Heatmap.Shipped["Jan"].Should().HaveCount(100);
    }

    [Fact]
    public async Task LoadAsync_UnicodeContent_ShouldPreserveCharacters()
    {
        var json = """
        {
            "title": "日本語ダッシュボード",
            "subtitle": "Ünïcödë — Spëcîal Characters™",
            "backlogLink": "https://example.com/path?q=hello%20world&lang=日本語"
        }
        """;
        await WriteJsonFile("data.json", json);
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data!.Title.Should().Be("日本語ダッシュボード");
        service.Data.Subtitle.Should().Contain("Ünïcödë");
        service.Data.BacklogLink.Should().Contain("日本語");
    }

    [Fact]
    public async Task LoadAsync_DefaultPath_ShouldUseWebRootPath()
    {
        await WriteJsonFile("data.json", """{ "title": "From WebRoot" }""");
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync(); // null filePath -> uses WebRootPath

        service.IsError.Should().BeFalse();
        service.Data!.Title.Should().Be("From WebRoot");
    }

    [Fact]
    public async Task LoadAsync_TruncatedJson_ShouldReturnParseError()
    {
        await WriteJsonFile("data.json", """{ "title": "Test", "months": ["Jan" """);
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("Failed to parse data.json");
    }

    [Fact]
    public async Task LoadAsync_JsonArray_ShouldReturnError()
    {
        await WriteJsonFile("data.json", """[{"title": "wrong shape"}]""");
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_MissingTitle_ShouldLogWarningButSucceed()
    {
        await WriteJsonFile("data.json", """{ "subtitle": "No title here" }""");
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("title")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_EmptyTracks_ShouldLogWarningButSucceed()
    {
        var json = """
        {
            "title": "Has Title",
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-04-01", "tracks": [] }
        }
        """;
        await WriteJsonFile("data.json", json);
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        service.IsError.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("no timeline tracks")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_SuccessfulLoad_ShouldLogInformation()
    {
        await WriteJsonFile("data.json", CreateFullDataJson());
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_AllMilestoneTypes_ShouldDeserializeCorrectly()
    {
        await WriteJsonFile("data.json", CreateFullDataJson());
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        var allMilestones = service.Data!.Timeline.Tracks.SelectMany(t => t.Milestones).ToList();
        allMilestones.Should().Contain(m => m.Type == "poc");
        allMilestones.Should().Contain(m => m.Type == "production");
        allMilestones.Should().Contain(m => m.Type == "checkpoint");

        foreach (var ms in allMilestones)
        {
            ms.Date.Should().NotBeNullOrEmpty();
            ms.Label.Should().NotBeNullOrEmpty();
            DateTime.TryParse(ms.Date, out _).Should().BeTrue($"milestone date '{ms.Date}' should be parseable");
        }
    }

    [Fact]
    public async Task LoadAsync_TrackColors_ShouldBePreserved()
    {
        await WriteJsonFile("data.json", CreateFullDataJson());
        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        await service.LoadAsync();

        var tracks = service.Data!.Timeline.Tracks;
        tracks[0].Color.Should().Be("#0078D4");
        tracks[1].Color.Should().Be("#34A853");
        tracks[2].Color.Should().Be("#EA4335");
    }
}