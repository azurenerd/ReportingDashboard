using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Web.Services;
using ReportingDashboard.Web.Tests.Unit;
using Xunit;

namespace ReportingDashboard.Web.Tests.Integration;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataFilePath;

    public DashboardDataServiceTests()
    {
        _tempDir = Directory.CreateTempSubdirectory("rd-tests-").FullName;
        _dataFilePath = Path.Combine(_tempDir, "data.json");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* ignore */ }
    }

    private DashboardDataService CreateService()
    {
        var env = new FakeWebHostEnvironment { ContentRootPath = _tempDir };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dashboard:DataFilePath"] = "data.json"
            })
            .Build();
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new DashboardDataService(cache, env, config, NullLogger<DashboardDataService>.Instance);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Construction_WithValidFile_CachesDataAndGetCurrentReturnsIt()
    {
        File.WriteAllText(_dataFilePath, TestDataFactory.ValidJson);

        using var svc = CreateService();
        var result = svc.GetCurrent();

        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Project.Title.Should().Be("My Project");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Construction_WithMissingFile_ProducesNotFoundError()
    {
        using var svc = CreateService();

        var result = svc.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("NotFound");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Construction_WithMalformedJson_ProducesParseError()
    {
        File.WriteAllText(_dataFilePath, "{ \"project\": { \"title\": \"X\" "); // unterminated

        using var svc = CreateService();
        var result = svc.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("ParseError");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Construction_WithMalformedJsonUsingFixture_PopulatesLineAndColumn()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "malformed-data.json");
        File.Copy(fixturePath, _dataFilePath, overwrite: true);

        using var svc = CreateService();
        var result = svc.GetCurrent();

        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("ParseError");
        (result.Error.Line.HasValue || result.Error.Column.HasValue)
            .Should().BeTrue("System.Text.Json should provide at least one of line/column on parse failure");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Construction_WithValidationFailure_ProducesValidationError()
    {
        var bad = TestDataFactory.ValidJson.Replace("\"#0078D4\"", "\"#ZZZZZZ\"");
        File.WriteAllText(_dataFilePath, bad);

        using var svc = CreateService();
        var result = svc.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("ValidationError");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FileEdit_TriggersReloadAndFiresDataChanged()
    {
        File.WriteAllText(_dataFilePath, TestDataFactory.ValidJson);

        using var svc = CreateService();
        svc.GetCurrent().Data!.Project.Title.Should().Be("My Project");

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        svc.DataChanged += (_, _) => tcs.TrySetResult(true);

        var updated = TestDataFactory.ValidJson.Replace("\"My Project\"", "\"Renamed Project\"");
        // Small delay to ensure FSW is armed, then write.
        await Task.Delay(100);
        File.WriteAllText(_dataFilePath, updated);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        completed.Should().Be(tcs.Task, "DataChanged should fire within 5s of file edit");

        // Allow cache swap to settle.
        await Task.Delay(50);
        svc.GetCurrent().Data!.Project.Title.Should().Be("Renamed Project");
    }
}