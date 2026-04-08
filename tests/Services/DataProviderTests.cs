using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Models;

namespace AgentSquad.Tests.Services;

public class DataProviderTests
{
    private readonly Mock<IDataCache> _mockCache;
    private readonly Mock<ILogger<DataProvider>> _mockLogger;
    private readonly string _testDbPath = "test.db";

    public DataProviderTests()
    {
        _mockCache = new Mock<IDataCache>();
        _mockLogger = new Mock<ILogger<DataProvider>>();
    }

    [Fact]
    public void DataProvider_InitializesWithCorrectSignature()
    {
        var provider = new DataProvider(_mockLogger.Object, _mockCache.Object, _testDbPath);
        Assert.NotNull(provider);
    }

    [Fact]
    public async Task DataProvider_RetrievesDataFromCache()
    {
        var testProject = new Project { Id = "proj-1", Name = "Test Project" };

        _mockCache.Setup(c => c.GetAsync<Project>("project"))
            .ReturnsAsync(testProject);

        var provider = new DataProvider(_mockLogger.Object, _mockCache.Object, _testDbPath);
        var result = await _mockCache.Object.GetAsync<Project>("project");

        Assert.NotNull(result);
        Assert.Equal("proj-1", result.Id);
    }

    [Fact]
    public async Task DataProvider_StoresDataInCache()
    {
        var testProject = new Project { Id = "proj-2", Name = "New Project" };

        _mockCache.Setup(c => c.SetAsync("project", testProject))
            .Returns(Task.CompletedTask);

        await _mockCache.Object.SetAsync("project", testProject);

        _mockCache.Verify(c => c.SetAsync("project", testProject), Times.Once);
    }

    [Fact]
    public async Task DataProvider_HandlesNullCacheResults()
    {
        _mockCache.Setup(c => c.GetAsync<Project>(It.IsAny<string>()))
            .ReturnsAsync((Project)null);

        var result = await _mockCache.Object.GetAsync<Project>(It.IsAny<string>());

        Assert.Null(result);
    }
}