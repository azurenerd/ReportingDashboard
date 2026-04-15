using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DataServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private static string BuildValidJson() => JsonSerializer.Serialize(new
    {
        header = new
        {
            title = "Test Project",
            subtitle = "Test Org • Test Workstream • April 2026",
            backlogUrl = "https://example.com",
            currentMonth = "April 2026"
        },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-12-31",
            nowDate = "2026-04-15",
            tracks = new[]
            {
                new { id = "m1", label = "M1", description = "Core API", color = "#0078D4" }
            },
            milestones = new[]
            {
                new { trackId = "m1", date = "2026-03-26", label = "PoC", type = "poc", description = (string?)null }
            }
        },
        heatmap = new
        {
            columns = new[] { "Jan", "Feb", "Mar", "Apr" },
            highlightColumnIndex = 3,
            rows = new[]
            {
                new
                {
                    category = "Shipped",
                    colorTheme = "green",
                    cells = new[]
                    {
                        new { items = new[] { "Item A" } },
                        new { items = new[] { "Item B" } },
                        new { items = Array.Empty<string>() },
                        new { items = Array.Empty<string>() }
                    }
                }
            }
        }
    });

    [Fact]
    public void ValidJson_LoadsDataSuccessfully()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());

        using var service = new DataService(_tempDir);

        service.GetData().Should().NotBeNull();
        service.GetData()!.Header.Title.Should().Be("Test Project");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void MissingFile_ReturnsNullDataWithError()
    {
        // No data.json created in _tempDir
        using var service = new DataService(_tempDir);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("data.json not found");
        service.GetError().Should().Contain(_tempDir);
    }

    [Fact]
    public void MalformedJson_ReturnsNullDataWithParseError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{ invalid json !!! }");

        using var service = new DataService(_tempDir);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Failed to parse data.json");
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());

        var service = new DataService(_tempDir);
        var act = () => service.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void OnDataChanged_FiresAfterFileChange()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());
        using var service = new DataService(_tempDir);

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        // Modify the file to trigger watcher
        Thread.Sleep(50);
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());

        var fired = eventFired.Wait(TimeSpan.FromSeconds(3));
        fired.Should().BeTrue("OnDataChanged should fire after file modification");
    }
}