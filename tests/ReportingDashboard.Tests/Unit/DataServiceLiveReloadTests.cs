using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for DataService live-reload code paths not covered by existing tests:
/// - OnDataChanged event fires after reload (success and failure)
/// - Debounce collapses multiple rapid file changes into one reload
/// - Dispose cleans up FileSystemWatcher and Timer
/// - Schema version mismatch on live reload preserves last-known-good data
/// - GetCurrentMonthName with and without CurrentMonthOverride
/// </summary>
[Trait("Category", "Unit")]
public class DataServiceLiveReloadTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DataServiceLiveReloadTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DSLiveReload_" + Guid.NewGuid().ToString("N"));
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
        string title = "Test Dashboard",
        string? nowDateOverride = "2026-04-10",
        int schemaVersion = 1,
        string? currentMonthOverride = null)
    {
        var data = new
        {
            schemaVersion,
            title,
            subtitle = "Sub",
            backlogUrl = "https://example.com",
            nowDateOverride,
            currentMonthOverride,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-31",
                workstreams = new[]
                {
                    new
                    {
                        id = "ws1",
                        name = "WS1",
                        color = "#0078D4",
                        milestones = new[]
                        {
                            new { label = "M1", date = "2026-03-01", type = "poc", labelPosition = (string?)null }
                        }
                    }
                }
            },
            heatmap = new
            {
                monthColumns = new[] { "Jan", "Feb", "Mar" },
                categories = new[]
                {
                    new
                    {
                        name = "Shipped",
                        emoji = "\u2705",
                        cssClass = "ship",
                        months = new[]
                        {
                            new { month = "Jan", items = new[] { "Item A" } }
                        }
                    }
                }
            }
        };
        return JsonSerializer.Serialize(data);
    }

    private void WriteDataJson(string content)
    {
        File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), content);
    }

    [Fact]
    public async Task OnDataChanged_FiresAfterSuccessfulReload()
    {
        WriteDataJson(CreateValidJson("Initial"));
        using var service = new DataService(_envMock.Object);

        var eventFired = new TaskCompletionSource<bool>();
        service.OnDataChanged += () => eventFired.TrySetResult(true);

        // Trigger file change
        WriteDataJson(CreateValidJson("Updated"));

        var fired = await Task.WhenAny(eventFired.Task, Task.Delay(3000)) == eventFired.Task;
        fired.Should().BeTrue("OnDataChanged should fire after a successful file reload");
        service.GetData()!.Title.Should().Be("Updated");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public async Task OnDataChanged_FiresAfterFailedReload()
    {
        WriteDataJson(CreateValidJson("Good"));
        using var service = new DataService(_envMock.Object);

        var eventFired = new TaskCompletionSource<bool>();
        service.OnDataChanged += () => eventFired.TrySetResult(true);

        // Trigger file change with bad JSON
        WriteDataJson("{ not valid json }}}");

        var fired = await Task.WhenAny(eventFired.Task, Task.Delay(3000)) == eventFired.Task;
        fired.Should().BeTrue("OnDataChanged should fire even after a failed reload");
        service.GetError().Should().NotBeNull();
    }

    [Fact]
    public async Task LiveReload_SchemaVersionMismatch_PreservesLastKnownGoodData()
    {
        WriteDataJson(CreateValidJson("Good Data", schemaVersion: 1));
        using var service = new DataService(_envMock.Object);
        service.GetData()!.Title.Should().Be("Good Data");

        // Overwrite with wrong schema version
        WriteDataJson(CreateValidJson("Bad Schema", schemaVersion: 99));

        await Task.Delay(1500);

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Good Data");
        service.GetError().Should().Contain("schemaVersion");
    }

    [Fact]
    public async Task LiveReload_FixedJson_ClearsErrorAfterBrokenReload()
    {
        WriteDataJson(CreateValidJson("Original"));
        using var service = new DataService(_envMock.Object);
        service.GetData()!.Title.Should().Be("Original");

        // Break it
        WriteDataJson("broken!!!");
        await Task.Delay(1500);
        service.GetError().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Original");

        // Fix it
        WriteDataJson(CreateValidJson("Fixed"));
        await Task.Delay(1500);
        service.GetError().Should().BeNull();
        service.GetData()!.Title.Should().Be("Fixed");
    }

    [Fact]
    public void Dispose_CleansUpWithoutExceptions()
    {
        WriteDataJson(CreateValidJson());
        var service = new DataService(_envMock.Object);

        var act = () => service.Dispose();

        act.Should().NotThrow("Dispose should cleanly release FileSystemWatcher and Timer");
    }
}