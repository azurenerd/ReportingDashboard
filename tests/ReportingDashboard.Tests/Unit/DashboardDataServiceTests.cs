using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests
{
    private static ILogger<DashboardDataService> CreateLogger() =>
        new Mock<ILogger<DashboardDataService>>().Object;

    [Fact]
    public void Constructor_MissingDataJson_SetsIsLoadedFalse()
    {
        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            Directory.SetCurrentDirectory(tempDir);
            var svc = new DashboardDataService(CreateLogger());
            Assert.False(svc.IsLoaded);
            Assert.NotNull(svc.ErrorMessage);
            Assert.Contains("not found", svc.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Constructor_InvalidJson_SetsErrorMessage()
    {
        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            Directory.SetCurrentDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "data.json"), "{ not valid json }}}");
            var svc = new DashboardDataService(CreateLogger());
            Assert.False(svc.IsLoaded);
            Assert.NotNull(svc.ErrorMessage);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Constructor_ValidJson_SetsIsLoadedTrue()
    {
        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            Directory.SetCurrentDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "data.json"), BuildValidJson());
            var svc = new DashboardDataService(CreateLogger());
            Assert.True(svc.IsLoaded);
            Assert.Null(svc.ErrorMessage);
            Assert.NotNull(svc.Data);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Constructor_EmptyTitle_SetsValidationError()
    {
        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            Directory.SetCurrentDirectory(tempDir);
            var json = BuildValidJson().Replace("\"Test Project\"", "\"\"");
            File.WriteAllText(Path.Combine(tempDir, "data.json"), json);
            var svc = new DashboardDataService(CreateLogger());
            Assert.False(svc.IsLoaded);
            Assert.Contains("project.title", svc.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Constructor_EndDateBeforeStartDate_SetsValidationError()
    {
        var originalDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            Directory.SetCurrentDirectory(tempDir);
            var json = BuildValidJson()
                .Replace("\"2026-01-01\"", "\"2026-12-31\"")
                .Replace("\"2026-12-31\"", "\"2026-01-01\"");
            File.WriteAllText(Path.Combine(tempDir, "data.json"), json);
            var svc = new DashboardDataService(CreateLogger());
            Assert.False(svc.IsLoaded);
            Assert.Contains("endDate", svc.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Directory.Delete(tempDir, true);
        }
    }

    private static string BuildValidJson() => """
        {
          "project": {
            "title": "Test Project",
            "subtitle": "Test Subtitle",
            "adoUrl": null,
            "workstream": "Test Workstream"
          },
          "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-12-31",
            "nowDate": "2026-04-17",
            "milestones": []
          },
          "heatmap": {
            "columns": ["Jan"],
            "currentColumn": "Jan",
            "rows": []
          }
        }
        """;
}