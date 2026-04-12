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
    public async Task DashboardDataService_QuartersHaveValidYears()
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

        result.Quarters.Should().AllSatisfy(q => q.Year.Should().BeGreaterThanOrEqualTo(2000));
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
    public async Task DashboardDataService_MilestonesHaveValidDates()
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

        result.Milestones.Should().AllSatisfy(m => 
            DateTime.TryParse(m.Date, out _).Should().BeTrue()
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

    [Fact]
    public async Task DashboardDataService_RefreshAsync_ReloadsConfiguration()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(config, logger);

        var result1 = await service.GetDashboardConfigAsync();
        await service.RefreshAsync();
        var result2 = await service.GetDashboardConfigAsync();

        result1.ProjectName.Should().Be(result2.ProjectName);
    }

    [Fact]
    public void JsonFile_ExistsAtConfiguredPath()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var dataPath = config.GetValue<string>("DashboardDataPath") ?? "./wwwroot/data/data.json";
        var fullPath = Path.Combine(projectPath, dataPath);

        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public void JsonFile_IsValidJson()
    {
        var projectPath = GetProjectPath();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var dataPath = config.GetValue<string>("DashboardDataPath") ?? "./wwwroot/data/data.json";
        var fullPath = Path.Combine(projectPath, dataPath);

        var json = File.ReadAllText(fullPath);
        var action = () => JsonDocument.Parse(json);

        action.Should().NotThrow();
    }

    [Fact]
    public void CssFile_Exists()
    {
        var projectPath = GetProjectPath();
        var cssPath = Path.Combine(projectPath, "wwwroot", "css", "dashboard.css");

        File.Exists(cssPath).Should().BeTrue();
    }

    [Fact]
    public void CssFile_ContainsRequiredSelectors()
    {
        var projectPath = GetProjectPath();
        var cssPath = Path.Combine(projectPath, "wwwroot", "css", "dashboard.css");

        var css = File.ReadAllText(cssPath);

        var requiredSelectors = new[] { ".hdr", ".hm-grid", ".hm-cell", ".ship-cell", ".prog-cell", ".carry-cell", ".block-cell" };
        foreach (var selector in requiredSelectors)
        {
            css.Should().Contain(selector);
        }
    }

    [Fact]
    public void CssFile_ContainsColorPalette()
    {
        var projectPath = GetProjectPath();
        var cssPath = Path.Combine(projectPath, "wwwroot", "css", "dashboard.css");

        var css = File.ReadAllText(cssPath);

        var colors = new[] { "#111", "#0078D4", "#34A853", "#F4B400", "#EA4335" };
        foreach (var color in colors)
        {
            css.Should().Contain(color);
        }
    }

    [Fact]
    public void CssFile_BodyHasCorrectDimensions()
    {
        var projectPath = GetProjectPath();
        var cssPath = Path.Combine(projectPath, "wwwroot", "css", "dashboard.css");

        var css = File.ReadAllText(cssPath);

        css.Should().Contain("1920px");
        css.Should().Contain("1080px");
    }
}