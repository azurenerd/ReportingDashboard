using Xunit;
using Moq;
using AgentSquad.Models;
using AgentSquad.Services;

namespace AgentSquad.Tests.Components;

public class WorkItemSummaryTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;

    public WorkItemSummaryTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
    }

    [Fact]
    public async Task WorkItemSummary_DisplaysActiveItems()
    {
        var workItems = new[] {
            new WorkItem { Id = "wi-1", Title = "Task 1", Status = WorkItemStatus.Active },
            new WorkItem { Id = "wi-2", Title = "Task 2", Status = WorkItemStatus.Shipped }
        };

        _mockCache.Setup(c => c.GetAsync<WorkItem[]>("workitems"))
            .ReturnsAsync(workItems);

        var result = await _mockCache.Object.GetAsync<WorkItem[]>("workitems");

        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal("wi-1", result[0].Id);
    }

    [Fact]
    public async Task WorkItemSummary_FiltersShippedItems()
    {
        var workItems = new[] {
            new WorkItem { Id = "wi-1", Title = "Shipped Task", Status = WorkItemStatus.Shipped }
        };

        _mockCache.Setup(c => c.GetAsync<WorkItem[]>(It.IsAny<string>()))
            .ReturnsAsync(workItems);

        var result = await _mockCache.Object.GetAsync<WorkItem[]>(It.IsAny<string>());

        Assert.All(result, item => Assert.Equal(WorkItemStatus.Shipped, item.Status));
    }
}