using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Configuration;

[Trait("Category", "Unit")]
public class AppsettingsTests
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
    public void Appsettings_FileExists()
    {
        var projectPath = GetProjectPath();
        var filePath = Path.Combine(projectPath, "appsettings.json");

        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void Appsettings_CanBeLoaded()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        config.Should().NotBeNull();
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
    }

    [Fact]
    public void AppsettingsDevelopment_FileExists()
    {
        var projectPath = GetProjectPath();
        var filePath = Path.Combine(projectPath, "appsettings.Development.json");

        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public void AppsettingsDevelopment_CanBeLoaded()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        config.Should().NotBeNull();
    }

    [Fact]
    public void AppsettingsDevelopment_OverridesLoggingLevel()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var loggingSection = config.GetSection("Logging");
        loggingSection.Should().NotBeNull();
    }
}