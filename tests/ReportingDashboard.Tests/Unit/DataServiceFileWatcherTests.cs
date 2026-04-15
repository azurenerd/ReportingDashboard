using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;
using FluentAssertions;

namespace ReportingDashboard.Tests.Unit;

public class DataServiceFileWatcherTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly string _dataJsonPath;

    public DataServiceFileWatcherTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DataServiceTests_" + Guid.NewGuid().ToString("N"));
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);
        _dataJsonPath = Path.Combine(_wwwrootDir, "data.json");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DataService CreateService(string? json = null)
    {
        json ??= MakeValidJson();
        File.WriteAllText(_dataJsonPath, json);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);

        return new DataService(envMock.Object);
    }

    private static string MakeValidJson(string title = "Test Project", int schemaVersion = 1, string? nowOverride = null)
    {
        var nowVal = nowOverride is null ? "null" : $"\"{nowOverride}\"";
        return $$"""
        {
            "schemaVersion": {{schemaVersion}},
            "title": "{{title}}",
            "subtitle": "Test Subtitle",
            "backlogUrl": "https://example.com/backlog",
            "nowDateOverride": {{nowVal}},
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "workstreams": []
            },
            "heatmap": {
                "monthColumns": ["Jan", "Feb", "Mar", "Apr"],
                "categories": []
            }
        }
        """;
    }

    [Fact]
    public void LiveReload_ValidJson_UpdatesData()
    {
        using var svc = CreateService();
        svc.GetData().Should().NotBeNull();
        svc.GetData()!.Title.Should().Be("Test Project");

        var changedEvent = new ManualResetEventSlim(false);
        svc.OnDataChanged += () => changedEvent.Set();

        File.WriteAllText(_dataJsonPath, MakeValidJson("Updated Title"));
        changedEvent.Wait(TimeSpan.FromSeconds(3)).Should().BeTrue("OnDataChanged should fire within 3s");

        svc.GetData()!.Title.Should().Be("Updated Title");
        svc.GetError().Should().BeNull();
    }

    [Fact]
    public void LiveReload_MalformedJson_PreservesLastGoodData()
    {
        using var svc = CreateService();
        var originalTitle = svc.GetData()!.Title;

        var changedEvent = new ManualResetEventSlim(false);
        svc.OnDataChanged += () => changedEvent.Set();

        File.WriteAllText(_dataJsonPath, "{ broken json !!!");
        changedEvent.Wait(TimeSpan.FromSeconds(3)).Should().BeTrue();

        svc.GetData().Should().NotBeNull();
        svc.GetData()!.Title.Should().Be(originalTitle);
        svc.GetError().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void LiveReload_FixedJson_ClearsError()
    {
        using var svc = CreateService();

        var changedEvent = new ManualResetEventSlim(false);
        svc.OnDataChanged += () => changedEvent.Set();

        // Break it
        File.WriteAllText(_dataJsonPath, "{ broken }");
        changedEvent.Wait(TimeSpan.FromSeconds(3)).Should().BeTrue();
        svc.GetError().Should().NotBeNull();

        // Fix it
        changedEvent.Reset();
        File.WriteAllText(_dataJsonPath, MakeValidJson("Fixed Title"));
        changedEvent.Wait(TimeSpan.FromSeconds(3)).Should().BeTrue();

        svc.GetError().Should().BeNull();
        svc.GetData()!.Title.Should().Be("Fixed Title");
    }

    [Fact]
    public void LiveReload_SchemaVersionMismatch_PreservesLastGoodData()
    {
        using var svc = CreateService();
        var originalTitle = svc.GetData()!.Title;

        var changedEvent = new ManualResetEventSlim(false);
        svc.OnDataChanged += () => changedEvent.Set();

        File.WriteAllText(_dataJsonPath, MakeValidJson(schemaVersion: 99));
        changedEvent.Wait(TimeSpan.FromSeconds(3)).Should().BeTrue();

        svc.GetData()!.Title.Should().Be(originalTitle);
        svc.GetError().Should().Contain("expected 1");
    }

    [Fact]
    public void Debounce_CollapsesRapidFireEvents()
    {
        using var svc = CreateService();
        int fireCount = 0;
        svc.OnDataChanged += () => Interlocked.Increment(ref fireCount);

        for (int i = 0; i < 5; i++)
        {
            File.WriteAllText(_dataJsonPath, MakeValidJson($"Rapid {i}"));
            Thread.Sleep(50);
        }

        Thread.Sleep(1500);
        fireCount.Should().BeLessOrEqualTo(3, "debounce should collapse rapid-fire events");
    }

    [Fact]
    public void OnDataChanged_SubscriberException_DoesNotKillPipeline()
    {
        using var svc = CreateService();
        bool secondCalled = false;

        svc.OnDataChanged += () => throw new InvalidOperationException("Boom!");
        svc.OnDataChanged += () => secondCalled = true;

        var changedEvent = new ManualResetEventSlim(false);
        svc.OnDataChanged += () => changedEvent.Set();

        File.WriteAllText(_dataJsonPath, MakeValidJson("After Exception"));
        changedEvent.Wait(TimeSpan.FromSeconds(3)).Should().BeTrue();

        secondCalled.Should().BeTrue("second subscriber should still be called");
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        var svc = CreateService();
        svc.Dispose();
        svc.Dispose(); // Second dispose should not throw
    }

    [Fact]
    public void GetEffectiveDate_WithValidOverride_ReturnsParsedDate()
    {
        using var svc = CreateService(MakeValidJson(nowOverride: "2026-02-15"));
        svc.GetEffectiveDate().Should().Be(new DateOnly(2026, 2, 15));
    }

    [Fact]
    public void GetEffectiveDate_WithInvalidOverride_FallsBackToToday()
    {
        using var svc = CreateService(MakeValidJson(nowOverride: "not-a-date"));
        svc.GetEffectiveDate().Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public void GetCurrentMonthName_DerivesFromEffectiveDate()
    {
        using var svc = CreateService(MakeValidJson(nowOverride: "2026-06-15"));
        svc.GetCurrentMonthName().Should().Be("Jun");
    }
}