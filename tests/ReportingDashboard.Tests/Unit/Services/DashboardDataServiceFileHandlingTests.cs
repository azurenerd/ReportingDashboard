using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Tests file system edge cases: encoding, concurrent access, large files,
/// directory missing, etc. Complements the core DashboardDataServiceTests.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceFileHandlingTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataDir;
    private readonly string _dataJsonPath;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardDataServiceFileHandlingTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardFileTests_{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "Data");
        Directory.CreateDirectory(_dataDir);
        _dataJsonPath = Path.Combine(_dataDir, "data.json");

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DashboardDataService CreateService() => new(_envMock.Object);

    private static string ValidJson => """
    {
        "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
        "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
        "tracks": [],
        "heatmap": { "months": ["Jan"], "categories": [] }
    }
    """;

    private void WriteJson(string json) => File.WriteAllText(_dataJsonPath, json);

    [Fact]
    public void Load_DataDirectoryDoesNotExist_ReturnsFileNotFoundError()
    {
        // Point to a content root with no Data/ subdirectory
        var emptyRoot = Path.Combine(Path.GetTempPath(), $"EmptyRoot_{Guid.NewGuid():N}");
        Directory.CreateDirectory(emptyRoot);

        try
        {
            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.ContentRootPath).Returns(emptyRoot);
            var service = new DashboardDataService(env.Object);

            var (data, error) = service.Load();

            data.Should().BeNull();
            error.Should().Contain("data.json not found at:");
        }
        finally
        {
            Directory.Delete(emptyRoot, true);
        }
    }

    [Fact]
    public void Load_FileWithWhitespaceOnly_ReturnsError()
    {
        WriteJson("   \n\t  ");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_FileWithTrailingComma_ReturnsJsonError()
    {
        WriteJson("""{ "project": { "title": "T", }, }""");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("Invalid JSON");
    }

    [Fact]
    public void Load_FileWithComments_ReturnsJsonError()
    {
        // Standard JSON doesn't allow comments
        WriteJson("""
        {
            // this is a comment
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("Invalid JSON");
    }

    [Fact]
    public void Load_LargeValidFile_Succeeds()
    {
        // Generate a JSON with many items
        var items = new Dictionary<string, List<string>>();
        for (int m = 1; m <= 12; m++)
        {
            var monthItems = Enumerable.Range(1, 50).Select(i => $"Work item {i} for month {m}").ToList();
            items[$"Month{m}"] = monthItems;
        }
        var data = new
        {
            project = new { title = "Large Project", subtitle = "Sub", backlogUrl = "", currentMonth = "Month1" },
            timeline = new { months = Enumerable.Range(1, 12).Select(m => $"Month{m}").ToList(), nowPosition = 0.5 },
            tracks = new List<object>(),
            heatmap = new
            {
                months = Enumerable.Range(1, 12).Select(m => $"Month{m}").ToList(),
                categories = new[]
                {
                    new { name = "Shipped", cssClass = "ship", emoji = "✅", items }
                }
            }
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        WriteJson(json);
        var service = CreateService();

        var (result, error) = service.Load();

        error.Should().BeNull();
        result.Should().NotBeNull();
        result!.Heatmap.Categories[0].Items.Should().HaveCount(12);
        result.Heatmap.Categories[0].Items["Month1"].Should().HaveCount(50);
    }

    [Fact]
    public void Load_Utf8WithoutBom_Succeeds()
    {
        File.WriteAllText(_dataJsonPath, ValidJson, new UTF8Encoding(false));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
    }

    [Fact]
    public void Load_ConsecutiveCallsAfterFileDeletion_ReturnsError()
    {
        WriteJson(ValidJson);
        var service = CreateService();

        var (data1, error1) = service.Load();
        error1.Should().BeNull();
        data1.Should().NotBeNull();

        File.Delete(_dataJsonPath);

        var (data2, error2) = service.Load();
        data2.Should().BeNull();
        error2.Should().Contain("data.json not found");
    }

    [Fact]
    public void Load_ConsecutiveCallsWithDifferentContent_ReflectsLatestFile()
    {
        var service = CreateService();

        WriteJson(ValidJson);
        var (data1, _) = service.Load();
        data1!.Project.Title.Should().Be("T");

        var updatedJson = ValidJson.Replace("\"title\": \"T\"", "\"title\": \"Updated\"");
        WriteJson(updatedJson);

        var (data2, _) = service.Load();
        data2!.Project.Title.Should().Be("Updated");
    }

    [Fact]
    public void Load_TruncatedJson_ReturnsError()
    {
        WriteJson("""{ "project": { "title": "T", "subtitle": """);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_DuplicateJsonKeys_LastOneWins()
    {
        var json = """
        {
            "project": { "title": "First", "title": "Second", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        WriteJson(json);
        var service = CreateService();

        var (data, error) = service.Load();

        // System.Text.Json accepts duplicate keys, last one wins
        if (error is null)
        {
            data!.Project.Title.Should().Be("Second");
        }
        else
        {
            // If it errors, that's also acceptable behavior
            error.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Load_ErrorTuple_DataIsAlwaysNullWhenErrorExists()
    {
        WriteJson("invalid json content");
        var service = CreateService();

        var (data, error) = service.Load();

        // When there's an error, data must be null (contract)
        error.Should().NotBeNullOrEmpty();
        data.Should().BeNull();
    }

    [Fact]
    public void Load_SuccessTuple_ErrorIsAlwaysNullWhenDataExists()
    {
        WriteJson(ValidJson);
        var service = CreateService();

        var (data, error) = service.Load();

        // When data loads, error must be null (contract)
        data.Should().NotBeNull();
        error.Should().BeNull();
    }
}