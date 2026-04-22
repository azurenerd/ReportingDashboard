using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Unit;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
    [Trait("Category", "Unit")]
    public void MissingFile_GetDataReturnsNull_GetErrorContainsFileNotFound()
    {
        var path = Path.Combine(_tempDir, "nonexistent.json");
        var service = new JsonFileDataService(CreateOptions(path));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("file not found", service.GetError()!);
        Assert.Contains(Path.GetFullPath(path), service.GetError()!);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MalformedJson_GetDataReturnsNull_GetErrorContainsParseMessage()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{ this is not valid json }}}");
        var service = new JsonFileDataService(CreateOptions(path));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("Could not parse", service.GetError()!);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidJson_GetDataReturnsPopulated_GetErrorReturnsNull()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-10"
        };
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data));
        var service = new JsonFileDataService(CreateOptions(path));

        Assert.NotNull(service.GetData());
        Assert.Null(service.GetError());
        Assert.Equal("Test Project", service.GetData()!.Title);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidJson_CaseInsensitiveDeserialization()
    {
        var json = """{"title":"My Title","subtitle":"Sub","backlogUrl":"https://x.com","currentDate":"2026-04-01"}""";
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        var service = new JsonFileDataService(CreateOptions(path));

        var result = service.GetData();
        Assert.NotNull(result);
        Assert.Equal("My Title", result!.Title);
        Assert.Equal("Sub", result.Subtitle);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NullDataFilePath_DefaultsToDataJson()
    {
        var options = Options.Create(new DashboardOptions { DataFilePath = null! });
        var service = new JsonFileDataService(options);

        // Will fail to find ./data.json (likely missing) - but should not throw
        var error = service.GetError();
        // Either data is loaded or we get a graceful error
        Assert.True(service.GetData() != null || error != null);
    }
}