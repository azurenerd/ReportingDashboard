using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    private string CreateTempJson(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    private static IOptions<DashboardOptions> CreateOptions(string path)
    {
        return Options.Create(new DashboardOptions { DataFilePath = path });
    }

    [Fact]
    public void ValidFile_LoadsData()
    {
        var json = JsonSerializer.Serialize(new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle"
        });
        var path = CreateTempJson(json);
        var service = new JsonFileDataService(CreateOptions(path));

        Assert.NotNull(service.GetData());
        Assert.Null(service.GetError());
        Assert.Equal("Test Project", service.GetData()!.Title);
    }

    [Fact]
    public void MissingFile_ReturnsError()
    {
        var service = new JsonFileDataService(CreateOptions("/nonexistent/path/data.json"));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("file not found", service.GetError()!);
    }

    [Fact]
    public void MalformedJson_ReturnsError()
    {
        var path = CreateTempJson("{ invalid json content!!!");
        var service = new JsonFileDataService(CreateOptions(path));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("Could not parse data.json", service.GetError()!);
    }

    [Fact]
    public void EmptyJsonObject_LoadsWithDefaults()
    {
        var path = CreateTempJson("{}");
        var service = new JsonFileDataService(CreateOptions(path));

        Assert.NotNull(service.GetData());
        Assert.Null(service.GetError());
        Assert.Equal("", service.GetData()!.Title);
    }

    [Fact]
    public void FullSampleData_DeserializesCorrectly()
    {
        var data = new DashboardData
        {
            Title = "Sample",
            CurrentDate = "2026-04-10",
            Timeline = new TimelineData
            {
                StartDate = "2025-12-19",
                EndDate = "2026-06-30",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Id = "M1",
                        Label = "Test",
                        Color = "#0078D4",
                        Milestones = new List<Milestone>
                        {
                            new() { Date = "2026-01-12", Type = "checkpoint", Label = "Jan 12" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "March", "April" },
                CurrentMonth = "April",
                Categories = new List<HeatmapCategory>
                {
                    new()
                    {
                        Name = "Shipped",
                        ColorClass = "ship",
                        Items = new Dictionary<string, List<string>>
                        {
                            ["March"] = new() { "Item 1" },
                            ["April"] = new() { "Item 2", "Item 3" }
                        }
                    }
                }
            }
        };
        var json = JsonSerializer.Serialize(data);
        var path = CreateTempJson(json);
        var service = new JsonFileDataService(CreateOptions(path));

        var result = service.GetData();
        Assert.NotNull(result);
        Assert.Single(result!.Timeline.Tracks);
        Assert.Equal("M1", result.Timeline.Tracks[0].Id);
        Assert.Single(result.Timeline.Tracks[0].Milestones);
        Assert.Equal(2, result.Heatmap.Months.Count);
        Assert.Single(result.Heatmap.Categories);
        Assert.Equal(2, result.Heatmap.Categories[0].Items["April"].Count);
    }

    public void Dispose()
    {
        foreach (var f in _tempFiles)
        {
            try { File.Delete(f); } catch { }
        }
    }
}
