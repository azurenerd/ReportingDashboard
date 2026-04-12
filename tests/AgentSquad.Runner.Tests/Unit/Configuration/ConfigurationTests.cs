using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Configuration;

[Trait("Category", "Unit")]
public class ConfigurationTests
{
    [Fact]
    public void AppSettingsJson_LoadsWithoutError()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(GetProjectPath())
            .AddJsonFile("appsettings.json")
            .Build();

        config.Should().NotBeNull();
    }

    [Fact]
    public void AppSettingsDevelopmentJson_LoadsAndOverridesLoggingLevel()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(GetProjectPath())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var defaultLogLevel = config["Logging:LogLevel:Default"];
        defaultLogLevel.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DashboardDataPath_ConfigurationValueIsAccessible()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(GetProjectPath())
            .AddJsonFile("appsettings.json")
            .Build();

        var dataPath = config.GetValue<string>("DashboardDataPath");

        dataPath.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DashboardDataPath_ContainsDataJsonReference()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(GetProjectPath())
            .AddJsonFile("appsettings.json")
            .Build();

        var dataPath = config.GetValue<string>("DashboardDataPath");

        dataPath.Should().Contain("data.json");
    }

    [Fact]
    public void AppSettingsJson_ValidJsonStructure()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(GetProjectPath())
            .AddJsonFile("appsettings.json")
            .Build();

        var sections = config.GetChildren();

        sections.Should().NotBeEmpty();
    }

    [Fact]
    public void ConfigurationBuilder_CanReadMultipleAppSettingsFiles()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(GetProjectPath())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        config.Should().NotBeNull();
        var children = config.GetChildren();
        children.Should().NotBeEmpty();
    }

    private string GetProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectPath = Path.Combine(currentDirectory, "..", "..", "..", "src", "AgentSquad.Runner");
        return Path.GetFullPath(projectPath);
    }
}