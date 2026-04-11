using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Tests for DashboardDataService initial state and construction behavior.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceInitialStateTests
{
    [Fact]
    public void Constructor_InitialState_DataIsNull()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);

        service.Data.Should().BeNull();
    }

    [Fact]
    public void Constructor_InitialState_IsErrorIsFalse()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);

        service.IsError.Should().BeFalse();
    }

    [Fact]
    public void Constructor_InitialState_ErrorMessageIsNull()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);

        service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Constructor_RequiresLogger()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);

        service.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_CalledTwice_SecondLoadOverwritesFirst()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"InitState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var logger = new Mock<ILogger<DashboardDataService>>();
            var service = new DashboardDataService(logger.Object);

            // First load: valid
            var validPath = Path.Combine(tempDir, "valid.json");
            File.WriteAllText(validPath, """
            {
                "title": "First",
                "subtitle": "Sub",
                "backlogLink": "https://test.com",
                "currentMonth": "Jan",
                "months": ["Jan"],
                "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-04-10", "tracks": [{ "name": "T", "label": "L", "milestones": [] }] },
                "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
            }
            """);
            await service.LoadAsync(validPath);
            service.IsError.Should().BeFalse();
            service.Data!.Title.Should().Be("First");

            // Second load: error
            await service.LoadAsync(Path.Combine(tempDir, "nonexistent.json"));
            service.IsError.Should().BeTrue();
            service.Data.Should().BeNull("error state should clear previous data");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task LoadAsync_AfterError_CanReloadSuccessfully()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"InitState2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var logger = new Mock<ILogger<DashboardDataService>>();
            var service = new DashboardDataService(logger.Object);

            // First: error
            await service.LoadAsync(Path.Combine(tempDir, "nonexistent.json"));
            service.IsError.Should().BeTrue();

            // Second: valid
            var path = Path.Combine(tempDir, "valid.json");
            File.WriteAllText(path, """
            {
                "title": "Recovered",
                "subtitle": "Sub",
                "backlogLink": "https://test.com",
                "currentMonth": "Jan",
                "months": ["Jan"],
                "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-04-10", "tracks": [{ "name": "T", "label": "L", "milestones": [] }] },
                "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
            }
            """);
            await service.LoadAsync(path);
            service.IsError.Should().BeFalse();
            service.Data!.Title.Should().Be("Recovered");
            service.ErrorMessage.Should().BeNull();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}