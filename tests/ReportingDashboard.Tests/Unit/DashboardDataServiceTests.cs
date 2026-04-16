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
    private readonly string _webRootPath;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DashboardTests_" + Guid.NewGuid().ToString("N"));
        _webRootPath = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(Path.Combine(_webRootPath, "data"));

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_webRootPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private DashboardDataService CreateService() => new(_envMock.Object);

    private string BuildValidJson(
        string title = "Test Project",
        int milestoneStreamCount = 1,
        int columnCount = 3,
        int currentColumnIndex = 0,
        int rowCount = 1)
    {
        var streams = Enumerable.Range(0, milestoneStreamCount).Select(i => new
        {
            id = $"M{i + 1}",
            name = $"Stream {i + 1}",
            color = "#000",
            milestones = new[] { new { date = "2026-03-01", label = "v1", type = "checkpoint" } }
        }).ToArray();

        var columns = Enumerable.Range(0, columnCount).Select(i => $"Month{i + 1}").ToArray();
        var rows = Enumerable.Range(0, rowCount).Select(i => new
        {
            category = $"Cat{i + 1}",
            items = new Dictionary<string, object> { [columns[0]] = new[] { "Item A" } }
        }).ToArray();

        var obj = new
        {
            title,
            subtitle = "Sub",
            backlogUrl = "https://example.com",
            currentDate = "2026-04-01",
            timelineStart = "2026-01-01",
            timelineEnd = "2026-06-30",
            milestoneStreams = streams,
            heatmap = new { columns, currentColumnIndex, rows }
        };

        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    private async Task WriteDataJson(string content)
    {
        var path = Path.Combine(_webRootPath, "data", "data.json");
        await File.WriteAllTextAsync(path, content);
    }

    [Fact]
    public async Task LoadAsync_MissingFile_SetsHasErrorWithNotFoundMessage()
    {
        var service = CreateService();

        await service.LoadAsync();

        service.HasError.Should().BeTrue();
        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsHasErrorWithParseMessage()
    {
        await WriteDataJson("{ bad json }}");
        var service = CreateService();

        await service.LoadAsync();

        service.HasError.Should().BeTrue();
        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("malformed");
    }

    [Fact]
    public async Task LoadAsync_ValidJson_LoadsDataSuccessfully()
    {
        await WriteDataJson(BuildValidJson());
        var service = CreateService();

        await service.LoadAsync();

        service.HasError.Should().BeFalse();
        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("Test Project");
        service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyTitle_SetsHasErrorWithMissingFieldMessage()
    {
        await WriteDataJson(BuildValidJson(title: ""));
        var service = CreateService();

        await service.LoadAsync();

        service.HasError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("title");
    }

    [Fact]
    public async Task LoadAsync_CurrentColumnIndexOutOfRange_SetsHasErrorWithRangeMessage()
    {
        await WriteDataJson(BuildValidJson(columnCount: 3, currentColumnIndex: 5));
        var service = CreateService();

        await service.LoadAsync();

        service.HasError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("out of range");
    }
}