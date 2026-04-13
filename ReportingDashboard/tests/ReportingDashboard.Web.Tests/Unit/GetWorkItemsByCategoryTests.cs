using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class GetWorkItemsByCategoryTests : IDisposable
{
    private readonly string _tempDir;

    public GetWorkItemsByCategoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"CategoryTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        Directory.CreateDirectory(Path.Combine(_tempDir, "Data"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private DashboardDataService CreateService()
    {
        var env = new TestWebHostEnvironment(_tempDir);
        var logger = new Mock<ILogger<DashboardDataService>>();
        return new DashboardDataService(env, logger.Object);
    }

    private void WriteJson(string json)
    {
        File.WriteAllText(Path.Combine(_tempDir, "Data", "data.json"), json);
    }

    private static string BuildJsonWithCategories()
    {
        return """
        {
            "project": { "projectName": "Test", "overallStatus": "OnTrack", "summary": "s" },
            "milestones": [],
            "workItems": [
                { "id": 1, "title": "A", "description": "d", "category": "Shipped", "milestoneId": 1, "owner": "O", "priority": "High", "notes": null, "statusIndicator": "Done" },
                { "id": 2, "title": "B", "description": "d", "category": "Shipped", "milestoneId": 1, "owner": "O", "priority": "High", "notes": null, "statusIndicator": "Done" },
                { "id": 3, "title": "C", "description": "d", "category": "InProgress", "milestoneId": 1, "owner": "O", "priority": "Medium", "notes": null, "statusIndicator": "50%" },
                { "id": 4, "title": "D", "description": "d", "category": "CarriedOver", "milestoneId": 1, "owner": "O", "priority": "Low", "notes": null, "statusIndicator": "Blocked" }
            ]
        }
        """;
    }

    [Fact]
    public async Task GetWorkItemsByCategoryAsync_ReturnsMatchingItems()
    {
        WriteJson(BuildJsonWithCategories());
        var service = CreateService();

        var shipped = await service.GetWorkItemsByCategoryAsync("Shipped");

        Assert.Equal(2, shipped.Count);
        Assert.All(shipped, w => Assert.Equal("Shipped", w.Category));
    }

    [Fact]
    public async Task GetWorkItemsByCategoryAsync_IsCaseInsensitive()
    {
        WriteJson(BuildJsonWithCategories());
        var service = CreateService();

        var result = await service.GetWorkItemsByCategoryAsync("shipped");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetWorkItemsByCategoryAsync_NoMatches_ReturnsEmptyList()
    {
        WriteJson(BuildJsonWithCategories());
        var service = CreateService();

        var result = await service.GetWorkItemsByCategoryAsync("NonExistent");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDashboardDataAsync_EmptyJsonObject_ThrowsInvalidOperationException()
    {
        WriteJson("null");
        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardDataAsync());
    }

    [Fact]
    public async Task GetDashboardDataAsync_LogsErrorOnMalformedJson()
    {
        WriteJson("{ broken json !!!}}}");
        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        var env = new TestWebHostEnvironment(_tempDir);
        var service = new DashboardDataService(env, mockLogger.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardDataAsync());

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<JsonException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath) => ContentRootPath = contentRootPath;
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "TestApp";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
    }
}