using System.Text.Json;
using Microsoft.Extensions.Options;
using Xunit;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private IOptions<DashboardOptions> CreateOptions(string filePath)
    {
        return Options.Create(new DashboardOptions { DataFilePath = filePath });
    }

    [Fact]
    public void GetData_ValidFile_ReturnsDeserializedData()
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        var json = JsonSerializer.Serialize(new
        {
            title = "Test Project",
            subtitle = "Test Subtitle",
            backlogUrl = "https://example.com",
            currentDate = "2026-04-10",
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-06-30",
                tracks = new object[0]
            },
            heatmap = new
            {
                months = new[] { "March", "April" },
                currentMonth = "April",
                categories = new object[0]
            }
        });
        File.WriteAllText(filePath, json);

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.NotNull(service.GetData());
        Assert.Null(service.GetError());
        Assert.Equal("Test Project", service.GetData()!.Title);
        Assert.Equal("Test Subtitle", service.GetData()!.Subtitle);
        Assert.Equal(2, service.GetData()!.Heatmap.Months.Count);
    }

    [Fact]
    public void GetData_MissingFile_ReturnsError()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.json");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("file not found", service.GetError()!);
        Assert.Contains("data.sample.json", service.GetError()!);
    }

    [Fact]
    public void GetData_MalformedJson_ReturnsError()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "{ this is not valid json }}}}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("Could not parse data.json", service.GetError()!);
    }

    [Fact]
    public void GetData_EmptyJsonObject_ReturnsDefaultData()
    {
        var filePath = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(filePath, "{}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.NotNull(service.GetData());
        Assert.Null(service.GetError());
        Assert.Equal("", service.GetData()!.Title);
    }

    [Fact]
    public void GetData_WithCategories_DeserializesItems()
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        var json = """
        {
            "title": "Test",
            "heatmap": {
                "months": ["March"],
                "currentMonth": "March",
                "categories": [
                    {
                        "name": "Shipped",
                        "colorClass": "ship",
                        "items": {
                            "March": ["Item 1", "Item 2"]
                        }
                    }
                ]
            }
        }
        """;
        File.WriteAllText(filePath, json);

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.NotNull(service.GetData());
        var cat = service.GetData()!.Heatmap.Categories[0];
        Assert.Equal("Shipped", cat.Name);
        Assert.Equal(2, cat.Items["March"].Count);
    }
}
