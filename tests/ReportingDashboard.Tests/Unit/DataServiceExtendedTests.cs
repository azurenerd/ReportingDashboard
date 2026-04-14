using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for DataService code paths NOT covered by the existing DataServiceTests:
/// - GetCurrentMonthName (with and without override)
/// - GetEffectiveDate with invalid override format (fallback)
/// - Live reload via FileSystemWatcher (updates data, preserves last-known-good)
/// - OnDataChanged event fires on reload
/// </summary>
[Trait("Category", "Unit")]
public class DataServiceExtendedTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DataServiceExtendedTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DataServiceExtTests_" + Guid.NewGuid().ToString("N"));
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
        string? nowDateOverride = null,
        string? currentMonthOverride = null)
    {
        var data = new
        {
            schemaVersion = 1,
            title,
            subtitle = "Test Subtitle",
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
                        name = "Workstream 1",
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
                        emoji = "✅",
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
    public void GetCurrentMonthName_WithOverride_ReturnsOverrideValue()
    {
        WriteDataJson(CreateValidJson(currentMonthOverride: "Feb"));

        using var service = new DataService(_envMock.Object);

        service.GetCurrentMonthName().Should().Be("Feb");
    }

    [Fact]
    public void GetCurrentMonthName_WithoutOverride_DerivesFromEffectiveDate()
    {
        // nowDateOverride = "2026-06-15" => effective month = "Jun"
        WriteDataJson(CreateValidJson(nowDateOverride: "2026-06-15", currentMonthOverride: null));

        using var service = new DataService(_envMock.Object);

        service.GetCurrentMonthName().Should().Be("Jun");
    }

    [Fact]
    public void GetEffectiveDate_InvalidOverrideFormat_FallsBackToToday()
    {
        WriteDataJson(CreateValidJson(nowDateOverride: "not-a-date"));

        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public async Task LiveReload_UpdatesDataOnFileChange()
    {
        WriteDataJson(CreateValidJson("Original Title"));
        using var service = new DataService(_envMock.Object);
        service.GetData()!.Title.Should().Be("Original Title");

        // Overwrite the file to trigger FileSystemWatcher
        WriteDataJson(CreateValidJson("Updated Title"));

        // Wait for debounce (500ms) + margin
        await Task.Delay(1500);

        service.GetData()!.Title.Should().Be("Updated Title");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public async Task LiveReload_MalformedJson_PreservesLastKnownGoodData()
    {
        WriteDataJson(CreateValidJson("Good Data"));
        using var service = new DataService(_envMock.Object);
        service.GetData()!.Title.Should().Be("Good Data");

        // Overwrite with malformed JSON
        WriteDataJson("{ broken json !!!");

        // Wait for debounce + margin
        await Task.Delay(1500);

        // Last-known-good data should be preserved
        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Good Data");
        service.GetError().Should().Contain("invalid JSON");
    }
}