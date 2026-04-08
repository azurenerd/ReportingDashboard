using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using AgentSquad.Dashboard.Services;

namespace AgentSquad.Dashboard.Tests.Services;

public class DashboardDataServiceTests
{
    [Fact]
    public async Task DashboardDataService_ShouldStartAndStop()
    {
        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(mockLogger.Object);
        var cts = new CancellationTokenSource();

        var task = service.StartAsync(cts.Token);
        await Task.Delay(100);
        
        cts.Cancel();
        await task;

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DashboardDataService started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DashboardDataService_ShouldRunInBackground()
    {
        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(mockLogger.Object);
        var cts = new CancellationTokenSource();

        var task = service.StartAsync(cts.Token);
        var isRunning = !task.IsCompleted;

        cts.Cancel();

        Assert.True(isRunning, "Service should run in background");
    }
}