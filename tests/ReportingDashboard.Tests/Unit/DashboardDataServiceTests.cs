using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    private DashboardDataService CreateService(string filePath)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["DashboardDataPath"] = filePath })
            .Build();
        return new DashboardDataService(config);
    }

    [Fact]
    public async Task GetDataAsync_ThrowsFileNotFoundException_WhenFileMissing()
    {
        var svc = CreateService(Path.Combine(_tempDir, "nonexistent.json"));

        var act = () => svc.GetDataAsync();

        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*Dashboard data file not found at:*");
    }

    [Fact]
    public async Task GetDataAsync_ThrowsJsonException_WhenJsonMalformed()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        await File.WriteAllTextAsync(path, "{ this is not valid json }");
        var svc = CreateService(path);

        var act = () => svc.GetDataAsync();

        await act.Should().ThrowAsync<JsonException>();
    }

    [Fact]
    public async Task GetDataAsync_ThrowsInvalidOperationException_WhenDeserializesToNull()
    {
        var path = Path.Combine(_tempDir, "null.json");
        await File.WriteAllTextAsync(path, "null");
        var svc = CreateService(path);

        var act = () => svc.GetDataAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*deserialized to null*");
    }

    [Fact]
    public async Task GetDataAsync_ReturnsData_WhenValidJsonProvided()
    {
        var path = Path.Combine(_tempDir, "valid.json");
        var json = """
        {
          "header": { "title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04-01",
                      "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-06-30", "timelineMonths": [] },
          "timelineTracks": [],
          "heatmap": { "columns": [], "highlightColumnIndex": 0, "rows": [] }
        }
        """;
        await File.WriteAllTextAsync(path, json);
        var svc = CreateService(path);

        var result = await svc.GetDataAsync();

        result.Should().NotBeNull();
        result.Header.Title.Should().Be("Test");
    }

    [Fact]
    public async Task GetDataAsync_UsesFallbackPath_WhenConfigKeyAbsent()
    {
        var config = new ConfigurationBuilder().Build();
        var svc = new DashboardDataService(config);

        // File won't exist at default path; verify FileNotFoundException (not NullRef)
        var act = () => svc.GetDataAsync();

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}