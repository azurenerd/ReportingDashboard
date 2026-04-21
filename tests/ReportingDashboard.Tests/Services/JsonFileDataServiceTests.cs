using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rdtest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private JsonFileDataService CreateService(string filePath)
    {
        var options = Options.Create(new DashboardOptions { DataFilePath = filePath });
        return new JsonFileDataService(options);
    }

    [Fact]
    public void ValidFile_ReturnsData_AndNoError()
    {
        var dataPath = Path.Combine(_tempDir, "data.json");
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-10"
        };
        File.WriteAllText(dataPath, JsonSerializer.Serialize(data));

        var service = CreateService(dataPath);

        Assert.NotNull(service.GetData());
        Assert.Equal("Test Project", service.GetData()!.Title);
        Assert.Null(service.GetError());
    }

    [Fact]
    public void MissingFile_ReturnsNullData_AndErrorMessage()
    {
        var service = CreateService(Path.Combine(_tempDir, "nonexistent.json"));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("file not found", service.GetError()!);
    }

    [Fact]
    public void MalformedJson_ReturnsNullData_AndErrorMessage()
    {
        var dataPath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(dataPath, "not valid json!!!");

        var service = CreateService(dataPath);

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("Could not parse", service.GetError()!);
    }

    [Fact]
    public void EmptyJsonObject_ReturnsDataWithDefaults()
    {
        var dataPath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(dataPath, "{}");

        var service = CreateService(dataPath);

        Assert.NotNull(service.GetData());
        Assert.Null(service.GetError());
        Assert.Equal("", service.GetData()!.Title);
    }
}