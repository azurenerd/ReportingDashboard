using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceConstructorTests
{
    [Fact]
    public void Constructor_InitialState_DataIsNull()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var svc = new DashboardDataService(logger);

        Assert.Null(svc.Data);
    }

    [Fact]
    public void Constructor_InitialState_IsErrorIsFalse()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var svc = new DashboardDataService(logger);

        Assert.False(svc.IsError);
    }

    [Fact]
    public void Constructor_InitialState_ErrorMessageIsNull()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var svc = new DashboardDataService(logger);

        Assert.Null(svc.ErrorMessage);
    }

    [Fact]
    public void Constructor_AcceptsLogger_DoesNotThrow()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var exception = Record.Exception(() => new DashboardDataService(logger));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LoadAsync_CalledMultipleTimes_LastCallWins()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"DashCtorTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
            var svc = new DashboardDataService(logger);

            // Call 1: file not found
            await svc.LoadAsync(Path.Combine(tempDir, "nope.json"));
            Assert.True(svc.IsError);

            // Call 2: also not found, different path
            await svc.LoadAsync(Path.Combine(tempDir, "also_nope.json"));
            Assert.True(svc.IsError);
            Assert.Contains("also_nope.json", svc.ErrorMessage!);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}