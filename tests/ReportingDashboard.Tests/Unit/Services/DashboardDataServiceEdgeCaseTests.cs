using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceEdgeCaseTests : IDisposable
{
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceEdgeCaseTests()
    {
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(_loggerMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardEdge_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteTempJson(string json)
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(filePath, json);
        return filePath;
    }

    [Fact]
    public async Task LoadAsync_JsonWithTrailingComma_SetsError()
    {
        var json = """{ "title": "Test", }""";
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("Failed to parse");
    }

    [Fact]
    public async Task LoadAsync_JsonWithBom_HandlesGracefully()
    {
        var json = """
        {
            "title": "Test BOM",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = Path.Combine(_tempDir, "bom.json");
        await File.WriteAllTextAsync(filePath, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await _service.LoadAsync(filePath);

        // Should either succeed or provide a clear error, not throw unhandled
        // BOM is handled by .NET's JSON reader
        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Test BOM");
    }

    [Fact]
    public async Task LoadAsync_EmptyPath_SetsError()
    {
        await _service.LoadAsync("");

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_LargeJsonPayload_HandlesWithoutTimeout()
    {
        var items = string.Join(",", Enumerable.Range(0, 100).Select(i => $"\"Item {i}\""));
        var json = $$"""
        {
            "title": "Large Data",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            },
            "heatmap": {
                "shipped": { "jan": [{{items}}] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeFalse();
        _service.Data!.Heatmap!.Shipped["jan"].Should().HaveCount(100);
    }

    [Fact]
    public async Task LoadAsync_UnicodeContent_PreservesCharacters()
    {
        var json = """
        {
            "title": "Dashboard für Projekte — Übersicht",
            "subtitle": "Équipe — Données",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "Caractères spéciaux", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Contain("für");
        _service.Data.Subtitle.Should().Contain("Équipe");
    }

    [Fact]
    public async Task LoadAsync_OnlyWhitespace_SetsError()
    {
        var filePath = WriteTempJson("   \n\t  ");

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_ValidJson_WithExtraFields_IgnoresThem()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "extraField": "ignored",
            "anotherExtra": 42,
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        _service.IsError.Should().BeFalse();
        _service.Data!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task LoadAsync_NumberAsTitle_HandlesTypeCorrectly()
    {
        // JSON number where string is expected
        var json = """{ "title": 12345 }""";
        var filePath = WriteTempJson(json);

        await _service.LoadAsync(filePath);

        // Should either handle gracefully or set error
        // System.Text.Json will throw JsonException for type mismatch
        _service.IsError.Should().BeTrue();
    }
}