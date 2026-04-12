using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Configuration;

[Trait("Category", "Unit")]
public class ConfigurationTests
{
    private string GetProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectRoot = currentDirectory;
        
        while (!Directory.Exists(Path.Combine(projectRoot, "src", "AgentSquad.Runner")))
        {
            var parent = Directory.GetParent(projectRoot);
            if (parent == null || parent.FullName == projectRoot)
                break;
            projectRoot = parent.FullName;
        }
        
        return Path.Combine(projectRoot, "src", "AgentSquad.Runner");
    }

    [Fact]
    public void Appsettings_ContainsDashboardDataPath()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var dataPath = config.GetValue<string>("DashboardDataPath");

        dataPath.Should().NotBeNullOrEmpty();
        dataPath.Should().Contain("data.json");
    }

    [Fact]
    public void AppsettingsDevelopment_ContainsLoggingConfiguration()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var loggingSection = config.GetSection("Logging");

        loggingSection.Exists().Should().BeTrue();
    }

    [Fact]
    public void AppsettingsDevelopment_DefaultLogLevelIsDebug()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var defaultLogLevel = config["Logging:LogLevel:Default"];

        if (!string.IsNullOrEmpty(defaultLogLevel))
        {
            defaultLogLevel.Should().Be("Debug");
        }
    }
}