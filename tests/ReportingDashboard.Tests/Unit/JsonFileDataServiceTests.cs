using FluentAssertions;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class JsonFileDataServiceTests
{
    private static IOptions<DashboardOptions> CreateOptions(string path)
    {
        return Options.Create(new DashboardOptions { DataFilePath = path });
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MissingFile_ReturnsErrorWithPath()
    {
        var service = new JsonFileDataService(CreateOptions("nonexistent_test_file.json"));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("file not found");
        service.GetError().Should().Contain("data.sample.json");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MalformedJson_ReturnsParseError()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{invalid json content!!}");
            var service = new JsonFileDataService(CreateOptions(tempFile));

            service.GetData().Should().BeNull();
            service.GetError().Should().NotBeNull();
            service.GetError().Should().Contain("Could not parse data.json");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EmptyJsonObject_ReturnsDefaultData()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{}");
            var service = new JsonFileDataService(CreateOptions(tempFile));

            service.GetError().Should().BeNull();
            var data = service.GetData();
            data.Should().NotBeNull();
            data!.Title.Should().BeEmpty();
            data.Subtitle.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidJson_DeserializesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
            {
                "title": "Test Project",
                "subtitle": "Test Subtitle",
                "backlogUrl": "https://example.com",
                "currentDate": "2026-04-10",
                "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "tracks": [] },
                "heatmap": { "months": ["Jan"], "currentMonth": "Jan", "categories": [] }
            }
            """;
            File.WriteAllText(tempFile, json);
            var service = new JsonFileDataService(CreateOptions(tempFile));

            service.GetError().Should().BeNull();
            var data = service.GetData();
            data.Should().NotBeNull();
            data!.Title.Should().Be("Test Project");
            data.Subtitle.Should().Be("Test Subtitle");
            data.BacklogUrl.Should().Be("https://example.com");
            data.Timeline.Tracks.Should().BeEmpty();
            data.Heatmap.Months.Should().ContainSingle().Which.Should().Be("Jan");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NullDataFilePath_DefaultsToDataJson()
    {
        var options = Options.Create(new DashboardOptions { DataFilePath = null! });
        var service = new JsonFileDataService(options);

        // With no data.json in cwd, should get file-not-found error
        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("file not found");
    }
}