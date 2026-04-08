using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Moq;

namespace AgentSquad.Runner.Tests.Fixtures;

public static class MockDataProviderFactory
{
    public static Mock<IDataProvider> CreateSuccessfulMock(Project? project = null)
    {
        var mock = new Mock<IDataProvider>();
        mock.Setup(x => x.LoadProjectDataAsync())
            .ReturnsAsync(project ?? InvalidDataFixtures.ValidProject);
        return mock;
    }

    public static Mock<IDataProvider> CreateFileNotFoundMock()
    {
        var mock = new Mock<IDataProvider>();
        mock.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(new FileNotFoundException("File not found"));
        return mock;
    }

    public static Mock<IDataProvider> CreateInvalidJsonMock()
    {
        var mock = new Mock<IDataProvider>();
        mock.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(new System.Text.Json.JsonException("Invalid JSON"));
        return mock;
    }

    public static Mock<IDataProvider> CreateValidationFailureMock(string errorMessage)
    {
        var mock = new Mock<IDataProvider>();
        mock.Setup(x => x.LoadProjectDataAsync())
            .ThrowsAsync(new InvalidOperationException(errorMessage));
        return mock;
    }
}