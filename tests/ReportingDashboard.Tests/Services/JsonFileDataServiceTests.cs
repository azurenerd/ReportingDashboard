using System.Text.Json;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;
using FluentAssertions;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests
{
    private static IOptions<DashboardOptions> CreateOptions(string path)
    {
        return Options.Create(new DashboardOptions { DataFilePath = path });
    }

    [Fact]
    public void GetData_ValidFile_ReturnsData()
    {
        // Arrange - use the sample data file
        var samplePath = FindSampleDataPath();
        var service = new JsonFileDataService(CreateOptions(samplePath));

        // Act
        var data = service.GetData();
        var error = service.GetError();

        // Assert
        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().NotBeEmpty();
        data.Timeline.Tracks.Should().NotBeEmpty();
        data.Heatmap.Months.Should().NotBeEmpty();
        data.Heatmap.Categories.Should().NotBeEmpty();
    }

    [Fact]
    public void GetData_MissingFile_ReturnsError()
    {
        var service = new JsonFileDataService(CreateOptions("nonexistent-file.json"));

        var data = service.GetData();
        var error = service.GetError();

        data.Should().BeNull();
        error.Should().NotBeNull();
        error.Should().Contain("file not found");
        error.Should().Contain("data.sample.json");
    }

    [Fact]
    public void GetData_MalformedJson_ReturnsError()
    {
        // Create a temp file with bad JSON
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{ invalid json content }}}");
            var service = new JsonFileDataService(CreateOptions(tempFile));

            var data = service.GetData();
            var error = service.GetError();

            data.Should().BeNull();
            error.Should().NotBeNull();
            error.Should().Contain("Could not parse data.json");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetData_ValidFile_DeserializesAllFields()
    {
        var samplePath = FindSampleDataPath();
        var service = new JsonFileDataService(CreateOptions(samplePath));

        var data = service.GetData();

        data.Should().NotBeNull();
        data!.Title.Should().Be("Privacy Automation Release Roadmap");
        data.Subtitle.Should().Contain("Trusted Platform");
        data.BacklogUrl.Should().StartWith("https://");
        data.CurrentDate.Should().Be("2026-04-10");

        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Timeline.EndDate.Should().Be("2026-06-30");
        data.Timeline.Tracks.Should().HaveCount(3);

        var m1 = data.Timeline.Tracks[0];
        m1.Id.Should().Be("M1");
        m1.Color.Should().Be("#0078D4");
        m1.Milestones.Should().HaveCountGreaterThan(0);

        data.Heatmap.Months.Should().HaveCount(4);
        data.Heatmap.CurrentMonth.Should().Be("April");
        data.Heatmap.Categories.Should().HaveCount(4);
    }

    [Fact]
    public void GetData_EmptyJsonObject_ReturnsDataWithDefaults()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "{}");
            var service = new JsonFileDataService(CreateOptions(tempFile));

            var data = service.GetData();
            var error = service.GetError();

            error.Should().BeNull();
            data.Should().NotBeNull();
            data!.Title.Should().BeEmpty();
            data.Timeline.Tracks.Should().BeEmpty();
            data.Heatmap.Categories.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static string FindSampleDataPath()
    {
        // Walk up from test bin directory to find data.sample.json
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, "src", "ReportingDashboard.Web", "data.sample.json");
            if (File.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir)!;
        }
        throw new FileNotFoundException("Could not find data.sample.json");
    }
}
