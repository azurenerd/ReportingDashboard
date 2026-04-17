using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardDataServiceEdgeCaseTests
{
    [Fact]
    public async Task GetDataAsync_WithEmptyMilestonesList_SuccessfullyDeserializes()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"empty-milestones-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan"]},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col"], "highlightColumnIndex": 0, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[]]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var result = await service.GetDataAsync();

        result.TimelineTracks[0].Milestones.Should().BeEmpty();
        result.Header.Title.Should().Be("Test");

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithHighlightColumnIndexClamping_ClampsNegativeToZero()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"clamp-negative-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan"]},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col1", "Col2"], "highlightColumnIndex": -1, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[], []]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var result = await service.GetDataAsync();

        result.Heatmap.HighlightColumnIndex.Should().Be(0);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithHighlightColumnIndexOutOfBounds_ClampsToLastValidIndex()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"clamp-oob-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan"]},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col1", "Col2"], "highlightColumnIndex": 10, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[], []]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var result = await service.GetDataAsync();

        result.Heatmap.HighlightColumnIndex.Should().Be(1);

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithMissingOptionalBacklogLink_DefaultsToHash()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"backlog-default-{Guid.NewGuid()}.json");
        var json = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "", "reportDate": "2026-04-17", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": ["Jan"]},
              "timelineTracks": [{"id": "T1", "name": "Track", "description": "", "color": "#999", "milestones": []}],
              "heatmap": {"columns": ["Col"], "highlightColumnIndex": 0, "rows": [{"category": "shipped", "label": "Shipped", "cellItems": [[]]}]}
            }
            """;
        await File.WriteAllTextAsync(tempFile, json);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "DashboardDataPath", tempFile } })
            .Build();
        var service = new DashboardDataService(config);

        var result = await service.GetDataAsync();

        result.Header.BacklogLink.Should().Be("#");

        File.Delete(tempFile);
    }
}