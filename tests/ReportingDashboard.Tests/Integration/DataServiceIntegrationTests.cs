using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DataServiceIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DataServiceIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DataSvcInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task LoadAsync_ValidFile_SetsDataCorrectly()
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, TestDataHelper.CreateValidDataJsonString());

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();
        svc.Data!.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoadAsync_MissingFile_SetsError()
    {
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{ broken json {{{");

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().Contain("parse");
    }

    [Fact]
    public async Task LoadAsync_ErrorThenValid_Recovers()
    {
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        svc.IsError.Should().BeTrue();

        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, TestDataHelper.CreateValidDataJsonString());
        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();
    }
}