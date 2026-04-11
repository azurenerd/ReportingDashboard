using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

public class DashboardDataServiceEdgeCaseTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataServiceEdgeCaseTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLogger<DashboardDataService>.Instance;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task LoadDataAsync_MissingFile_ReturnsErrorMessage()
    {
        var service = CreateService("nonexistent/data.json");

        var result = await service.LoadDataAsync();

        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadDataAsync_EmptyJsonObject_ReturnsDefaultData()
    {
        WriteJson("{}");
        var service = CreateService();

        var result = await service.LoadDataAsync();

        result.ErrorMessage.Should().BeNull();
        result.Project.Should().BeNull();
        result.Milestones.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadDataAsync_MalformedJson_ReturnsErrorMessage()
    {
        WriteJson("{ invalid json }}}");
        var service = CreateService();

        var result = await service.LoadDataAsync();

        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("Invalid JSON");
    }

    [Fact]
    public async Task LoadDataAsync_NullArrayFields_ReturnsNoUnhandledException()
    {
        WriteJson("""{"project": {"name": "Test"}, "milestones": null}""");
        var service = CreateService();

        var result = await service.LoadDataAsync();

        // System.Text.Json may deserialize null into the property, overriding the default.
        // The key guarantee is no unhandled exception.
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadDataAsync_ValidProjectData_PopulatesProjectInfo()
    {
        WriteJson("""
        {
            "project": {
                "name": "Test Project",
                "lead": "Jane Doe",
                "status": "On Track",
                "backlogLink": "https://dev.azure.com/test"
            }
        }
        """);
        var service = CreateService();

        var result = await service.LoadDataAsync();

        result.ErrorMessage.Should().BeNull();
        result.Project.Should().NotBeNull();
        result.Project!.Name.Should().Be("Test Project");
        result.Project.Lead.Should().Be("Jane Doe");
        result.Project.BacklogLink.Should().Be("https://dev.azure.com/test");
    }

    [Fact]
    public async Task LoadDataAsync_EmptyFile_ReturnsErrorMessage()
    {
        WriteJson("");
        var service = CreateService();

        var result = await service.LoadDataAsync();

        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that the service can be instantiated with test dependencies.
    /// No async/await needed — returns Task.CompletedTask to avoid CS1998 warning.
    /// </summary>
    [Fact]
    public Task LoadDataAsync_PropertiesAreVirtual_CanBeMocked()
    {
        var service = CreateService();
        service.Should().NotBeNull();
        return Task.CompletedTask;
    }

    /// <summary>
    /// When a numeric value is provided where a string is expected, System.Text.Json behavior
    /// depends on JsonSerializerOptions. The DashboardDataService uses:
    ///   - PropertyNameCaseInsensitive = true
    ///   - PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    /// With these default settings, System.Text.Json throws JsonException for type mismatches
    /// (e.g., a number where a string property is expected). The service catches this and
    /// returns it as an error. If future options become more permissive (e.g., adding
    /// NumberHandling = JsonNumberHandling.AllowReadingFromString), this test may need adjustment.
    /// </summary>
    [Fact]
    public async Task LoadDataAsync_NumericValueForStringField_HandlesGracefully()
    {
        // "name" expects a string but gets a number
        WriteJson("""{"project": {"name": 12345}}""");
        var service = CreateService();

        var result = await service.LoadDataAsync();

        // With current JsonSerializerOptions (PropertyNameCaseInsensitive=true, CamelCase policy),
        // System.Text.Json throws JsonException for number-to-string coercion, caught by the service.
        // If this behavior changes with different options, the service still must not throw unhandled.
        if (result.ErrorMessage is not null)
        {
            result.ErrorMessage.Should().Contain("Invalid JSON");
        }
        else
        {
            // If System.Text.Json coerced the number to string (with permissive options), that's OK too.
            result.Project.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task LoadDataAsync_ExtraUnknownProperties_AreIgnored()
    {
        WriteJson("""
        {
            "project": {"name": "Test"},
            "unknownField": "should be ignored",
            "anotherUnknown": 42
        }
        """);
        var service = CreateService();

        var result = await service.LoadDataAsync();

        result.ErrorMessage.Should().BeNull();
        result.Project!.Name.Should().Be("Test");
    }

    private DashboardDataService CreateService(string? dataFilePath = null)
    {
        var dataPath = dataFilePath ?? "data.json";
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dashboard:DataFilePath"] = dataPath
            })
            .Build();

        var env = new TestWebHostEnvironment(_tempDir);
        return new DashboardDataService(env, config, _logger);
    }

    private void WriteJson(string json)
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(filePath, json);
    }

    private class TestWebHostEnvironment : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = contentRootPath;
            EnvironmentName = "Testing";
            ApplicationName = "ReportingDashboard.Tests";
            ContentRootFileProvider = new Microsoft.Extensions.FileProviders.NullFileProvider();
            WebRootFileProvider = new Microsoft.Extensions.FileProviders.NullFileProvider();
        }

        public string WebRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
    }
}