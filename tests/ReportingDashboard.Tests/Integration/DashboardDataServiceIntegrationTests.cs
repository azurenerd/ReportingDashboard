using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardDataServiceIntegrationTests
{
    [Fact]
    public async Task GetDataAsync_WithRealDataFile_DeserializesCompleteStructure()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dataFile = Path.Combine(tempDir, "data.json");

        var sampleData = """
            {
              "header": {
                "title": "Project Phoenix",
                "subtitle": "Engineering Division",
                "backlogLink": "https://ado.example.com/backlog",
                "reportDate": "2026-04",
                "timelineStartDate": "2026-01-01",
                "timelineEndDate": "2026-06-30",
                "timelineMonths": ["Jan", "Feb", "Mar", "Apr", "May", "Jun"]
              },
              "timelineTracks": [
                {
                  "id": "M1",
                  "name": "Platform",
                  "description": "Core platform work",
                  "color": "#4285F4",
                  "milestones": [
                    {"label": "Alpha", "date": "2026-02-15", "type": "poc", "labelPosition": "top"}
                  ]
                }
              ],
              "heatmap": {
                "columns": ["Jan", "Feb", "Mar", "Apr"],
                "highlightColumnIndex": 3,
                "rows": [
                  {"category": "shipped", "label": "Shipped Items", "cellItems": [["item1"], ["item2"], [], []]}
                ]
              }
            }
            """;

        await File.WriteAllTextAsync(dataFile, sampleData);

        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DashboardDataPath", dataFile }
            });
        var config = configBuilder.Build();
        var service = new DashboardDataService(config);

        // Act
        var result = await service.GetDataAsync();

        // Assert
        result.Header.Title.Should().Be("Project Phoenix");
        result.Header.Subtitle.Should().Be("Engineering Division");
        result.Header.TimelineMonths.Should().HaveCount(6);
        result.TimelineTracks.Should().HaveCount(1);
        result.TimelineTracks[0].Id.Should().Be("M1");
        result.TimelineTracks[0].Milestones.Should().HaveCount(1);
        result.Heatmap.Columns.Should().HaveCount(4);
        result.Heatmap.HighlightColumnIndex.Should().Be(3);
        result.Heatmap.Rows.Should().HaveCount(1);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetDataAsync_WithConfigurationFromBuilder_SuccessfullyLoadsData()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var appSettingsFile = Path.Combine(tempDir, "appsettings.json");
        var dataFile = Path.Combine(tempDir, "data.json");

        var appSettingsJson = $$"""
            {
              "DashboardDataPath": "{{dataFile.Replace("\\", "\\\\")}}"
            }
            """;

        var dataJson = """
            {
              "header": {"title": "Config Test", "subtitle": "", "backlogLink": "#", "reportDate": "", "timelineStartDate": "", "timelineEndDate": "", "timelineMonths": []},
              "timelineTracks": [],
              "heatmap": {"columns": [], "highlightColumnIndex": 0, "rows": []}
            }
            """;

        await File.WriteAllTextAsync(appSettingsFile, appSettingsJson);
        await File.WriteAllTextAsync(dataFile, dataJson);

        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile(appSettingsFile);
        var config = configBuilder.Build();
        var service = new DashboardDataService(config);

        // Act
        var result = await service.GetDataAsync();

        // Assert
        result.Header.Title.Should().Be("Config Test");

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetDataAsync_WithNestedComplexData_PreservesHierarchy()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "complex-data.json");

        var complexData = """
            {
              "header": {"title": "Complex", "subtitle": "Test", "backlogLink": "link", "reportDate": "date", "timelineStartDate": "start", "timelineEndDate": "end", "timelineMonths": ["M1", "M2"]},
              "timelineTracks": [
                {
                  "id": "T1",
                  "name": "Track 1",
                  "description": "Description",
                  "color": "#ABC",
                  "milestones": [
                    {"label": "M1", "date": "2026-01-01", "type": "production", "labelPosition": "bottom"},
                    {"label": "M2", "date": "2026-02-01", "type": "checkpoint"}
                  ]
                },
                {
                  "id": "T2",
                  "name": "Track 2",
                  "description": "Desc2",
                  "color": "#DEF",
                  "milestones": []
                }
              ],
              "heatmap": {
                "columns": ["Col1", "Col2", "Col3"],
                "highlightColumnIndex": 1,
                "rows": [
                  {"category": "shipped", "label": "Shipped", "cellItems": [["a", "b"], ["c"], []]},
                  {"category": "blocked", "label": "Blocked", "cellItems": [[], ["x"], ["y", "z"]]}
                ]
              }
            }
            """;

        await File.WriteAllTextAsync(tempFile, complexData);

        var configMock = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DashboardDataPath", tempFile }
            })
            .Build();
        var service = new DashboardDataService(configMock);

        // Act
        var result = await service.GetDataAsync();

        // Assert
        result.TimelineTracks.Should().HaveCount(2);
        result.TimelineTracks[0].Milestones.Should().HaveCount(2);
        result.TimelineTracks[0].Milestones[0].Type.Should().Be("production");
        result.TimelineTracks[0].Milestones[0].LabelPosition.Should().Be("bottom");
        result.TimelineTracks[1].Milestones.Should().BeEmpty();
        result.Heatmap.Rows.Should().HaveCount(2);
        result.Heatmap.Rows[0].CellItems[0].Should().HaveCount(2);
        result.Heatmap.Rows[1].CellItems[2].Should().HaveCount(2);

        File.Delete(tempFile);
    }
}