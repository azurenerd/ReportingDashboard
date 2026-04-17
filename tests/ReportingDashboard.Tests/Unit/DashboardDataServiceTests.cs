using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataDir;
    private readonly DashboardDataService _service;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dash-test-{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "data");
        Directory.CreateDirectory(_dataDir);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
        _service = new DashboardDataService(mockEnv.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private void WriteJsonFile(string filename, object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllText(Path.Combine(_dataDir, filename), json);
    }

    private object CreateValidData(int schemaVersion = 1) => new
    {
        schemaVersion,
        header = new { title = "Test Project", subtitle = "Test Subtitle", backlogLink = "https://example.com", currentDate = "2026-04-01" },
        timeline = new { startDate = "2026-01-01", endDate = "2026-06-30", tracks = new List<object>() },
        heatmap = new { columns = new[] { "Jan" }, rows = new List<object>() }
    };

    [Fact]
    public async Task LoadAsync_ValidDataJson_ReturnsDeserializedData()
    {
        WriteJsonFile("data.json", CreateValidData());

        var result = await _service.LoadAsync();

        result.Should().NotBeNull();
        result.Header.Title.Should().Be("Test Project");
        result.Header.Subtitle.Should().Be("Test Subtitle");
        result.SchemaVersion.Should().Be(1);
    }

    [Fact]
    public async Task LoadAsync_MissingFile_ThrowsFileNotFoundWithExpectedMessage()
    {
        var act = () => _service.LoadAsync("missing.json");

        var ex = await act.Should().ThrowAsync<FileNotFoundException>();
        ex.WithMessage("missing.json not found. Place your data file at wwwroot/data/missing.json.");
    }

    [Fact]
    public async Task LoadAsync_SchemaVersionMismatch_ThrowsInvalidOperationException()
    {
        WriteJsonFile("data.json", CreateValidData(schemaVersion: 99));

        var act = () => _service.LoadAsync();

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Unsupported schema version: 99. Expected: 1.");
    }

    [Theory]
    [InlineData("../evil.json", "Path separators and '..' are not allowed")]
    [InlineData("sub/file.json", "Path separators and '..' are not allowed")]
    [InlineData("sub\\file.json", "Path separators and '..' are not allowed")]
    [InlineData("notjson.txt", "Only .json files are supported")]
    public async Task LoadAsync_InvalidFilenames_ThrowsArgumentException(string filename, string expectedFragment)
    {
        var act = () => _service.LoadAsync(filename);

        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage($"*{expectedFragment}*");
    }

    [Fact]
    public async Task LoadAsync_CustomFilename_LoadsCorrectFile()
    {
        WriteJsonFile("custom.json", CreateValidData());

        var result = await _service.LoadAsync("custom.json");

        result.Should().NotBeNull();
        result.Header.Title.Should().Be("Test Project");
    }
}