using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Unit;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataPath;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rd-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _dataPath = Path.Combine(_tempDir, "data.json");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best effort */ }
    }

    private DashboardDataService CreateSut(int debounceMs = 50)
    {
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.SetupGet(e => e.ContentRootPath).Returns(_tempDir);
        envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);

        var configValues = new Dictionary<string, string?>
        {
            { "Dashboard:DataFilePath", _dataPath },
            { "Dashboard:HotReloadDebounceMs", debounceMs.ToString() },
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var cache = new MemoryCache(new MemoryCacheOptions());

        return new DashboardDataService(
            envMock.Object, config, cache, NullLogger<DashboardDataService>.Instance);
    }

    private const string ValidJson = """
    {
      "project": { "title": "Test", "subtitle": "Sub" },
      "timeline": { "start": "2026-01-01", "end": "2026-06-30", "lanes": [] },
      "heatmap": {
        "months": ["Jan","Feb","Mar","Apr"],
        "currentMonthIndex": null,
        "maxItemsPerCell": 4,
        "rows": [
          {"category":"shipped","cells":[[],[],[],[]]},
          {"category":"inProgress","cells":[[],[],[],[]]},
          {"category":"carryover","cells":[[],[],[],[]]},
          {"category":"blockers","cells":[[],[],[],[]]}
        ]
      }
    }
    """;

    [Fact]
    public void GetCurrent_HappyPath_ReturnsData_NoError()
    {
        File.WriteAllText(_dataPath, ValidJson);
        using var sut = CreateSut();

        var result = sut.GetCurrent();

        result.Error.Should().BeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Project.Title.Should().Be("Test");
    }

    [Fact]
    public void GetCurrent_MissingFile_ReturnsNotFoundError_DoesNotThrow()
    {
        // No file written.
        using var sut = CreateSut();

        var result = sut.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("NotFound");
        result.Error.FilePath.Should().Be(_dataPath);
    }

    [Fact]
    public void GetCurrent_MalformedJson_ReturnsParseError_DoesNotThrow()
    {
        File.WriteAllText(_dataPath, "{ \"project\": { \"title\": \"x\" "); // unterminated
        using var sut = CreateSut();

        var result = sut.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("ParseError");
    }

    [Fact]
    public void GetCurrent_SecondCall_ReturnsCachedResult()
    {
        File.WriteAllText(_dataPath, ValidJson);
        using var sut = CreateSut();

        var a = sut.GetCurrent();
        var b = sut.GetCurrent();

        b.Should().BeSameAs(a);
    }

    [Fact]
    public async Task HotReload_FileSystemWatcher_ReloadsDataAfterFileChange()
    {
        File.WriteAllText(_dataPath, ValidJson);
        using var sut = CreateSut(debounceMs: 50);

        var initial = sut.GetCurrent();
        initial.Data.Should().NotBeNull();
        initial.Data!.Project.Title.Should().Be("Test");

        var changedSignaled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        sut.DataChanged += (_, _) => changedSignaled.TrySetResult(true);

        var updated = ValidJson.Replace("\"title\": \"Test\"", "\"title\": \"Updated\"");
        File.WriteAllText(_dataPath, updated);

        var completed = await Task.WhenAny(changedSignaled.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completed.Should().Be(changedSignaled.Task, "FileSystemWatcher should trigger a reload");

        var after = sut.GetCurrent();
        after.Data.Should().NotBeNull();
        after.Data!.Project.Title.Should().Be("Updated");
    }
}
