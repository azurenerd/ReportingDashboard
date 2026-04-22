using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    private string CreateTempJsonFile(string content)
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
    public void GetData_ValidFile_ReturnsDeserializedData()
    {
        var json = JsonSerializer.Serialize(new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-10",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                Tracks = new List<TimelineTrack>
                {
                    new() { Id = "M1", Label = "Test Track", Color = "#0078D4", Milestones = new() }
                }
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "March", "April" },
                CurrentMonth = "April",
                Categories = new List<HeatmapCategory>()
            }
        });

        var path = CreateTempJsonFile(json);
        var service = new JsonFileDataService(CreateOptions(path));

        var data = service.GetData();
        var error = service.GetError();

        Assert.Null(error);
        Assert.NotNull(data);
        Assert.Equal("Test Dashboard", data!.Title);
        Assert.Equal("Test Subtitle", data.Subtitle);
        Assert.Single(data.Timeline.Tracks);
    }

    [Fact]
    public void GetData_MissingFile_ReturnsError()
    {
        var service = new JsonFileDataService(CreateOptions("/nonexistent/path/data.json"));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("file not found", service.GetError()!);
    }

    [Fact]
    public void GetData_MalformedJson_ReturnsError()
    {
        var path = CreateTempJsonFile("{ invalid json content!!!");
        var service = new JsonFileDataService(CreateOptions(path));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("Could not parse data.json", service.GetError()!);
    }

    [Fact]
    public void GetData_EmptyJsonObject_ReturnsDefaults()
    {
        var path = CreateTempJsonFile("{}");
        var service = new JsonFileDataService(CreateOptions(path));

        var data = service.GetData();
        Assert.NotNull(data);
        Assert.Equal("", data!.Title);
        Assert.NotNull(data.Timeline);
        Assert.NotNull(data.Heatmap);
    }

    [Fact]
    public void GetData_CaseInsensitiveDeserialization_Works()
    {
        var json = """{"Title":"CaseTest","SUBTITLE":"SubTest","backlogurl":"http://test.com"}""";
        var path = CreateTempJsonFile(json);
        var service = new JsonFileDataService(CreateOptions(path));

        var data = service.GetData();
        Assert.NotNull(data);
        Assert.Equal("CaseTest", data!.Title);
        Assert.Equal("SubTest", data.Subtitle);
        Assert.Equal("http://test.com", data.BacklogUrl);
    }

    public void Dispose()
    {
        foreach (var f in _tempFiles)
        {
            try { File.Delete(f); } catch { }
        }
    }
}
