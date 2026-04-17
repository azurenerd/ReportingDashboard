using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceValidationTests
{
    [Fact]
    public async Task GetDataAsync_WithValidData_ReturnsSuccessfully()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"valid-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test Project", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan", "Feb"]},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col1", "Col2"], "highlightColumnIndex": 0, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[], []]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var result = await service.GetDataAsync();

        result.Should().NotBeNull();
        result.Header.Title.Should().Be("Test Project");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithEmptyTitle_ThrowsInvalidOperationException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"empty-title-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan"]},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col"], "highlightColumnIndex": 0, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[]]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var act = () => service.GetDataAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*header.title is required*");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithInvalidReportDate_ThrowsInvalidOperationException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"invalid-date-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-13-01", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan"]},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col"], "highlightColumnIndex": 0, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[]]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var act = () => service.GetDataAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ISO 8601*");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithEmptyTimelineMonths_ThrowsInvalidOperationException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"empty-months-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": []},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col"], "highlightColumnIndex": 0, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[]]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var act = () => service.GetDataAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*timelineMonths*");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithEmptyTimelineTracks_ThrowsInvalidOperationException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"empty-tracks-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan"]},
              "timelineTracks": [],
              "heatmap": {"columns": ["Col"], "highlightColumnIndex": 0, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[]]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var act = () => service.GetDataAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*timelineTracks*");

        File.Delete(tempFile);
    }
}