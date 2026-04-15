using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for DataService code paths not covered by existing test files:
/// - Subscriber exception isolation in NotifySubscribers
/// - Debounce collapsing rapid-fire file writes
/// - Thread-safe concurrent access to GetData/GetError
/// - GetEffectiveDate with valid override returns parsed date
/// - GetEffectiveDate with whitespace-only override falls back to today
/// </summary>
[Trait("Category", "Unit")]
public class DataServiceEdgeCaseTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DataServiceEdgeCaseTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DSEdgeCase_" + Guid.NewGuid().ToString("N"));
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private static string CreateValidJson(
        string title = "Test",
        string? nowDateOverride = null,
        int schemaVersion = 1)
    {
        var data = new
        {
            schemaVersion,
            title,
            subtitle = "Sub",
            backlogUrl = "https://example.com",
            nowDateOverride,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-31",
                workstreams = Array.Empty<object>()
            },
            heatmap = new
            {
                monthColumns = Array.Empty<string>(),
                categories = Array.Empty<object>()
            }
        };
        return JsonSerializer.Serialize(data);
    }

    private void WriteDataJson(string content) =>
        File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), content);

    [Fact]
    public async Task OnDataChanged_SubscriberException_DoesNotPreventOtherSubscribers()
    {
        WriteDataJson(CreateValidJson("Initial"));
        using var service = new DataService(_envMock.Object);

        var secondSubscriberCalled = new TaskCompletionSource<bool>();

        // First subscriber throws
        service.OnDataChanged += () => throw new InvalidOperationException("Boom!");
        // Second subscriber should still be called
        service.OnDataChanged += () => secondSubscriberCalled.TrySetResult(true);

        WriteDataJson(CreateValidJson("Updated"));

        var completed = await Task.WhenAny(secondSubscriberCalled.Task, Task.Delay(3000));
        completed.Should().Be(secondSubscriberCalled.Task,
            "second subscriber should be called even when first subscriber throws");
    }

    [Fact]
    public async Task Debounce_RapidFireWrites_CollapsesIntoFewerReloads()
    {
        WriteDataJson(CreateValidJson("Initial"));
        using var service = new DataService(_envMock.Object);

        var fireCount = 0;
        service.OnDataChanged += () => Interlocked.Increment(ref fireCount);

        // Write 5 times within ~200ms — debounce at 500ms should collapse them
        for (int i = 0; i < 5; i++)
        {
            WriteDataJson(CreateValidJson($"Rapid {i}"));
            await Task.Delay(40);
        }

        // Wait for debounce to settle (500ms) plus margin
        await Task.Delay(2000);

        fireCount.Should().BeGreaterThanOrEqualTo(1, "at least one reload should occur");
        fireCount.Should().BeLessThan(5, "debounce should collapse rapid writes into fewer reloads");
    }

    [Fact]
    public void ConcurrentAccess_GetDataAndGetError_DoNotThrow()
    {
        WriteDataJson(CreateValidJson("Concurrent"));
        using var service = new DataService(_envMock.Object);

        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        Parallel.For(0, 100, _ =>
        {
            try
            {
                var data = service.GetData();
                var error = service.GetError();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        exceptions.Should().BeEmpty("concurrent reads should be thread-safe under lock");
    }

    [Fact]
    public void GetEffectiveDate_WithValidOverride_ReturnsParsedDate()
    {
        WriteDataJson(CreateValidJson(nowDateOverride: "2026-02-15"));
        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(new DateOnly(2026, 2, 15));
    }

    [Fact]
    public void GetEffectiveDate_WithWhitespaceOverride_FallsBackToToday()
    {
        WriteDataJson(CreateValidJson(nowDateOverride: "   "));
        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }
}