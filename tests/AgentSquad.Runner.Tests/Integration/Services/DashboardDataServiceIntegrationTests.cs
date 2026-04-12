using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DashboardDataServiceIntegrationTests
{
    private string GetProjectPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var projectPath = Path.Combine(currentDirectory, "..", "..", "..", "src", "AgentSquad.Runner");
        return Path.GetFullPath(projectPath);
    }

    [Fact]
    public async Task DashboardDataService_LoadsSampleDataJsonSuccessfully()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result = await service.GetDashboardConfigAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardDataService_LoadedConfigHasValidProjectName()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result = await service.GetDashboardConfigAsync();

        result.ProjectName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DashboardDataService_LoadedConfigHasValidDescription()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result = await service.GetDashboardConfigAsync();

        result.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DashboardDataService_LoadedConfigContainsQuarters()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result = await service.GetDashboardConfigAsync();

        result.Quarters.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DashboardDataService_LoadedConfigContainsMilestones()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result = await service.GetDashboardConfigAsync();

        result.Milestones.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DashboardDataService_QuartersHaveValidMonths()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result = await service.GetDashboardConfigAsync();

        result.Quarters.Should().AllSatisfy(q => q.Month.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public async Task DashboardDataService_MilestonesHaveValidTypes()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result = await service.GetDashboardConfigAsync();

        var validTypes = new[] { "poc", "release", "checkpoint" };
        result.Milestones.Should().AllSatisfy(m => 
            validTypes.Should().Contain(m.Type.ToLower())
        );
    }

    [Fact]
    public async Task DashboardDataService_GetLastModifiedTime_ReturnsValidDateTime()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        await service.GetDashboardConfigAsync();
        var lastModified = service.GetLastModifiedTime();

        lastModified.Should().BeOnOrBefore(DateTime.UtcNow);
        lastModified.Year.Should().BeGreaterThanOrEqualTo(2000);
    }
}