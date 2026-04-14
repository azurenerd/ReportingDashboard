using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempWebRoot;
    private readonly DashboardDataService _service;

    public DashboardDataServiceTests()
    {
        _tempWebRoot = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempWebRoot);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempWebRoot);

        _service = new DashboardDataService(mockEnv.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWebRoot))
            Directory.Delete(_tempWebRoot, recursive: true);
    }

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("..\\windows\\system32\\config")]
    [InlineData("subfolder/file.json")]
    [InlineData("sub\\file.json")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LoadAsync_InvalidFilenames_ReturnsInvalidFilenameError(string filename)
    {
        var (data, error) = await _service.LoadAsync(filename);

        data.Should().BeNull();
        error.Should().StartWith("Invalid filename:");
    }

    [Fact]
    public async Task LoadAsync_NonexistentFile_ReturnsCouldNotLoadError()
    {
        var (data, error) = await _service.LoadAsync("nonexistent.json");

        data.Should().BeNull();
        error.Should().Be("Could not load data file: nonexistent.json");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_ReturnsDeserializedData()
    {
        var json = """
        {
            "title": "Test Dashboard",
            "currentDate": "2026-04-14",
            "months": ["Jan", "Feb"],
            "currentMonthIndex": 1,
            "milestoneTracks": [],
            "heatmap": {
                "shipped": { "items": {} },
                "inProgress": { "items": {} },
                "carryover": { "items": {} },
                "blockers": { "items": {} }
            }
        }
        """;
        await File.WriteAllTextAsync(Path.Combine(_tempWebRoot, "test.json"), json);

        var (data, error) = await _service.LoadAsync("test.json");

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().Be("Test Dashboard");
        data.Months.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ReturnsErrorLoadingDashboardData()
    {
        await File.WriteAllTextAsync(
            Path.Combine(_tempWebRoot, "bad.json"),
            "{ this is not valid json }}}");

        var (data, error) = await _service.LoadAsync("bad.json");

        data.Should().BeNull();
        error.Should().Contain("Error loading dashboard data:");
        error.Should().Contain("bad.json");
    }

    [Fact]
    public async Task LoadAsync_DefaultFilename_IsDataJson()
    {
        var json = """
        {
            "title": "Default File",
            "currentDate": "2026-01-01",
            "months": [],
            "currentMonthIndex": 0,
            "milestoneTracks": [],
            "heatmap": {
                "shipped": { "items": {} },
                "inProgress": { "items": {} },
                "carryover": { "items": {} },
                "blockers": { "items": {} }
            }
        }
        """;
        await File.WriteAllTextAsync(Path.Combine(_tempWebRoot, "data.json"), json);

        var (data, error) = await _service.LoadAsync();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().Be("Default File");
    }
}