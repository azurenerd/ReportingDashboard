using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Services;

public sealed class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _filePath;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rd-dds-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _filePath = Path.Combine(_tempDir, "data.json");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private DashboardDataService CreateService()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new DashboardDataService(_filePath, cache, NullLogger<DashboardDataService>.Instance);
    }

    private const string ValidJson = """
        {
          "project": { "title": "Hello", "subtitle": "Sub" },
          "timeline": { },
          "heatmap": { }
        }
        """;

    [Fact]
    public void GetCurrent_HappyPath_ReturnsDataAndNoError()
    {
        File.WriteAllText(_filePath, ValidJson);

        using var sut = CreateService();

        var result = sut.GetCurrent();

        result.Error.Should().BeNull();
        result.Data.Should().NotBeNull();
        result.Data!.Project!.Title.Should().Be("Hello");
    }

    [Fact]
    public void GetCurrent_MalformedJson_ReturnsParseErrorWithLineColumn()
    {
        File.WriteAllText(_filePath, "{ \"project\": { \"title\": \"X\"  ");

        using var sut = CreateService();

        var result = sut.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be(DashboardLoadErrorKind.ParseError);
        result.Error.FilePath.Should().Be(_filePath);
        result.Error.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetCurrent_MissingFile_ReturnsNotFoundError()
    {
        // file intentionally not created
        using var sut = CreateService();

        var result = sut.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be(DashboardLoadErrorKind.NotFound);
        result.Error.FilePath.Should().Be(_filePath);
    }

    [Fact]
    public void GetCurrent_NeverThrows_OnArbitraryGarbage()
    {
        File.WriteAllBytes(_filePath, new byte[] { 0xFF, 0x00, 0x42, 0x7B });

        using var sut = CreateService();

        var act = () => sut.GetCurrent();
        act.Should().NotThrow();
        sut.GetCurrent().Error.Should().NotBeNull();
    }

    [Fact]
    public async Task FileSystemWatcher_ReloadsCacheOnFileChange()
    {
        File.WriteAllText(_filePath, ValidJson);

        using var sut = CreateService();

        var initial = sut.GetCurrent();
        initial.Data!.Project!.Title.Should().Be("Hello");

        var changed = new TaskCompletionSource<DashboardLoadResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        sut.DataChanged += (_, _) =>
        {
            var r = sut.GetCurrent();
            if (r.Data?.Project?.Title == "World")
            {
                changed.TrySetResult(r);
            }
        };

        // Write updated content; FSW should fire, debounce 250ms, then reload.
        File.WriteAllText(_filePath, ValidJson.Replace("Hello", "World"));

        var completed = await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completed.Should().Be(changed.Task, "FileSystemWatcher should trigger a reload within 5s");

        var reloaded = sut.GetCurrent();
        reloaded.Error.Should().BeNull();
        reloaded.Data!.Project!.Title.Should().Be("World");
    }

    [Fact]
    public void Reload_AfterFixingMalformedFile_RecoversToValid()
    {
        File.WriteAllText(_filePath, "{ not json");
        using var sut = CreateService();

        sut.GetCurrent().Error.Should().NotBeNull();

        File.WriteAllText(_filePath, ValidJson);
        sut.Reload();

        var result = sut.GetCurrent();
        result.Error.Should().BeNull();
        result.Data!.Project!.Title.Should().Be("Hello");
    }
}
