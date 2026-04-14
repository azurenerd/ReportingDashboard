using ReportingDashboard.Services;
using ReportingDashboard.Tests.Helpers;
using Xunit;

namespace ReportingDashboard.Tests;

public class FileSystemWatcherTests : IDisposable
{
    private readonly string _tempDir;

    public FileSystemWatcherTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(),
            "RD_Tests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    private (DashboardDataService Service, TestLogger<DashboardDataService> Logger) CreateService()
    {
        var env = new TestWebHostEnvironment { ContentRootPath = _tempDir };
        var logger = new TestLogger<DashboardDataService>();
        var service = new DashboardDataService(env, logger);
        return (service, logger);
    }

    private void WriteDataJson(string content, string filename = "data.json")
    {
        var path = Path.Combine(_tempDir, filename);
        File.WriteAllText(path, content);
    }

    [Fact]
    public async Task OnDataChanged_Fires_WhenDataJsonModified()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson("Original Title"));
        var (service, _) = CreateService();
        service.Initialize();

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        // Act - modify the file
        await Task.Delay(100); // Let the watcher settle
        WriteDataJson(TestDataHelper.GenerateValidJson("Updated Title"));

        // Assert
        var fired = eventFired.Wait(TimeSpan.FromSeconds(5));
        Assert.True(fired, "OnDataChanged should fire when data.json is modified");

        service.Dispose();
    }

    [Fact]
    public async Task OnDataChanged_FiresOnce_ForRapidSuccessiveSaves()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson());
        var (service, _) = CreateService();
        service.Initialize();

        var fireCount = 0;
        service.OnDataChanged += () => Interlocked.Increment(ref fireCount);

        // Act - rapid successive saves within the debounce window
        await Task.Delay(100);
        for (int i = 0; i < 5; i++)
        {
            WriteDataJson(TestDataHelper.GenerateValidJson($"Rapid Save {i}"));
            await Task.Delay(50); // 50ms between saves, all within 300ms debounce
        }

        // Wait for debounce to complete plus buffer
        await Task.Delay(1000);

        // Assert - should fire once (or at most twice if timing is borderline)
        Assert.InRange(fireCount, 1, 2);

        service.Dispose();
    }

    [Fact]
    public async Task Cache_IsCleared_AfterFileChange()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson("Original"));
        var (service, _) = CreateService();
        service.Initialize();

        var originalData = service.GetData();
        Assert.Equal("Original", originalData.Title);

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        // Act
        await Task.Delay(100);
        WriteDataJson(TestDataHelper.GenerateValidJson("Updated"));

        var fired = eventFired.Wait(TimeSpan.FromSeconds(5));
        Assert.True(fired);

        // Assert - cache was cleared, new data is loaded
        var updatedData = service.GetData();
        Assert.Equal("Updated", updatedData.Title);

        service.Dispose();
    }

    [Fact]
    public async Task Cache_IsRetained_WhenReloadFailsDueToMalformedJson()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson("Valid Title"));
        var (service, logger) = CreateService();
        service.Initialize();

        // Populate cache
        var validData = service.GetData();
        Assert.Equal("Valid Title", validData.Title);

        // Act - write malformed JSON
        await Task.Delay(100);
        WriteDataJson("{broken json content that is not valid");

        // Wait for debounce + processing
        await Task.Delay(1000);

        // Assert - cache retains valid data
        var cachedData = service.GetData();
        Assert.Equal("Valid Title", cachedData.Title);

        // Verify warning was logged
        Assert.True(logger.HasWarning("retaining cached data"),
            "A warning should be logged when reload fails");

        service.Dispose();
    }

    [Fact]
    public void Dispose_StopsWatcher_AndDisposesTimer()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson());
        var (service, _) = CreateService();
        service.Initialize();

        var eventFired = false;
        service.OnDataChanged += () => eventFired = true;

        // Act
        service.Dispose();

        // Modify file after dispose
        WriteDataJson(TestDataHelper.GenerateValidJson("After Dispose"));
        Thread.Sleep(1000);

        // Assert - event should not fire after dispose
        Assert.False(eventFired, "OnDataChanged should not fire after Dispose");
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson());
        var (service, _) = CreateService();
        service.Initialize();

        // Act & Assert - calling Dispose twice should not throw
        service.Dispose();
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public async Task RenamedEvent_TriggersReload()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson("Original"));
        var (service, _) = CreateService();
        service.Initialize();

        _ = service.GetData(); // populate cache

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        // Act - simulate editor rename pattern: write to temp file then rename
        await Task.Delay(100);
        var tempPath = Path.Combine(_tempDir, "data.json.tmp");
        var targetPath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(tempPath, TestDataHelper.GenerateValidJson("Renamed"));
        File.Delete(targetPath);
        File.Move(tempPath, targetPath);

        // Assert
        var fired = eventFired.Wait(TimeSpan.FromSeconds(5));
        Assert.True(fired, "OnDataChanged should fire when a file is renamed to data.json");

        service.Dispose();
    }

    [Fact]
    public void ExecuteReload_ClearsCache_WhenFileIsValid()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson("Before"));
        var (service, _) = CreateService();
        service.Initialize();

        _ = service.GetData();

        // Overwrite with new valid data
        WriteDataJson(TestDataHelper.GenerateValidJson("After"));

        var eventFired = false;
        service.OnDataChanged += () => eventFired = true;

        // Act
        service.ExecuteReload();

        // Assert
        Assert.True(eventFired);
        var data = service.GetData();
        Assert.Equal("After", data.Title);

        service.Dispose();
    }

    [Fact]
    public void ExecuteReload_RetainsCache_WhenFileIsMalformed()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson("Cached"));
        var (service, logger) = CreateService();
        service.Initialize();

        _ = service.GetData(); // populate cache

        // Corrupt the file
        WriteDataJson("NOT VALID JSON {{{");

        var eventFired = false;
        service.OnDataChanged += () => eventFired = true;

        // Act
        service.ExecuteReload();

        // Assert
        Assert.False(eventFired, "OnDataChanged should not fire when reload fails");
        var data = service.GetData();
        Assert.Equal("Cached", data.Title);
        Assert.True(logger.HasWarning("retaining cached data"));

        service.Dispose();
    }

    [Fact]
    public void ExecuteReload_RetainsCache_WhenValidationFails()
    {
        // Arrange
        WriteDataJson(TestDataHelper.GenerateValidJson("Valid"));
        var (service, logger) = CreateService();
        service.Initialize();

        _ = service.GetData();

        // Write JSON that parses but fails validation (empty title)
        WriteDataJson("{\"title\":\"\",\"subtitle\":\"\",\"backlogUrl\":\"\",\"currentDate\":\"2026-01-01\",\"months\":[\"Jan\"],\"currentMonthIndex\":0,\"timelineStart\":\"2026-01-01\",\"timelineEnd\":\"2026-06-30\",\"milestones\":[{\"id\":\"M1\",\"label\":\"M1\",\"description\":\"Test\",\"color\":\"#000000\",\"markers\":[]}],\"categories\":[{\"name\":\"S\",\"key\":\"shipped\",\"items\":{}},{\"name\":\"I\",\"key\":\"inProgress\",\"items\":{}},{\"name\":\"C\",\"key\":\"carryover\",\"items\":{}},{\"name\":\"B\",\"key\":\"blockers\",\"items\":{}}]}");

        var eventFired = false;
        service.OnDataChanged += () => eventFired = true;

        // Act
        service.ExecuteReload();

        // Assert
        Assert.False(eventFired);
        var data = service.GetData();
        Assert.Equal("Valid", data.Title);

        service.Dispose();
    }
}