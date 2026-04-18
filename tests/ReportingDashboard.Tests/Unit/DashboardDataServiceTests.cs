using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests
{
    private static IConfiguration BuildConfig(string? path = null)
    {
        var dict = new Dictionary<string, string?>();
        if (path != null)
            dict["DashboardDataPath"] = path;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task GetDataAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        var service = new DashboardDataService(BuildConfig("nonexistent/data.json"));

        var act = async () => await service.GetDataAsync();

        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*Dashboard data file not found at:*");
    }

    [Fact]
    public async Task GetDataAsync_ValidJson_ReturnsDashboardReport()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
                {
                  "header": { "title": "Test Project", "backlogLink": "#" },
                  "timelineTracks": [],
                  "heatmap": { "columns": [], "highlightColumnIndex": 0, "rows": [] }
                }
                """;
            await File.WriteAllTextAsync(tempFile, json);

            var service = new DashboardDataService(BuildConfig(tempFile));
            var result = await service.GetDataAsync();

            result.Should().NotBeNull();
            result.Header.Title.Should().Be("Test Project");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetDataAsync_NullJson_ThrowsInvalidOperationException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "null");
            var service = new DashboardDataService(BuildConfig(tempFile));

            var act = async () => await service.GetDataAsync();

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("data.json deserialized to null");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Constructor_NoConfigKey_UsesDefaultPath()
    {
        var service = new DashboardDataService(BuildConfig());

        // Service should construct without error; default path is "Data/data.json"
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDataAsync_CaseInsensitiveJson_DeserializesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var json = """
                {
                  "Header": { "Title": "Case Test", "BacklogLink": "https://example.com" },
                  "TimelineTracks": [],
                  "Heatmap": { "Columns": ["Jan"], "HighlightColumnIndex": 0, "Rows": [] }
                }
                """;
            await File.WriteAllTextAsync(tempFile, json);

            var service = new DashboardDataService(BuildConfig(tempFile));
            var result = await service.GetDataAsync();

            result.Header.Title.Should().Be("Case Test");
            result.Heatmap.Columns.Should().ContainSingle("Jan");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}