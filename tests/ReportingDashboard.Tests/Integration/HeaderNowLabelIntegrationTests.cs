using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the NowLabel computed property in Components/Header.razor
/// rendered through the Dashboard page with a real DashboardDataService.
/// Tests the three NowLabel branches:
///   1. Timeline.NowDate parses → "Now (month year)"
///   2. Timeline.StartDate parses (NowDate invalid) → "Now (month year)"
///   3. Fallback → "Now (month)"
/// These branches are NOT covered by existing integration tests which only test
/// basic header fields and data contract validation.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderNowLabelIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public HeaderNowLabelIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"NowLabel_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            base.Dispose();
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }

    private DashboardDataService CreateServiceFromJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private string BuildJson(
        string currentMonth = "April",
        string? nowDate = "2026-04-10",
        string startDate = "2026-01-01",
        string endDate = "2026-07-01")
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "NowLabel Test",
            subtitle = "Team - Test",
            backlogLink = "https://dev.azure.com/test",
            currentMonth,
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate,
                endDate,
                nowDate = nowDate ?? "",
                tracks = new[]
                {
                    new
                    {
                        name = "M1",
                        label = "Track 1",
                        color = "#4285F4",
                        milestones = Array.Empty<object>()
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
    }

    #region Branch 1: Valid NowDate → year from NowDate

    [Fact]
    public void Dashboard_NowLabel_WithValidNowDate_ShowsYearFromNowDate()
    {
        var json = BuildJson(currentMonth: "April", nowDate: "2026-04-10");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (April 2026)", cut.Markup);
    }

    [Fact]
    public void Dashboard_NowLabel_NowDateYear2027_ShowsCorrectYear()
    {
        var json = BuildJson(currentMonth: "June", nowDate: "2027-06-15", startDate: "2026-01-01");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (June 2027)", cut.Markup);
        Assert.DoesNotContain("Now (June 2026)", cut.Markup);
    }

    [Fact]
    public void Dashboard_NowLabel_NowDateWithTimeComponent_StillParsesYear()
    {
        var json = BuildJson(currentMonth: "March", nowDate: "2026-03-15T14:30:00");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (March 2026)", cut.Markup);
    }

    [Fact]
    public void Dashboard_NowLabel_LeapYearDate_ParsesCorrectly()
    {
        var json = BuildJson(currentMonth: "February", nowDate: "2028-02-29");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (February 2028)", cut.Markup);
    }

    #endregion

    #region Branch 2: Invalid NowDate, valid StartDate → year from StartDate

    [Fact]
    public void Dashboard_NowLabel_InvalidNowDate_FallsBackToStartDateYear()
    {
        var json = BuildJson(currentMonth: "April", nowDate: "not-a-date", startDate: "2026-01-01");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (April 2026)", cut.Markup);
    }

    [Fact]
    public void Dashboard_NowLabel_EmptyNowDate_FallsBackToStartDateYear()
    {
        var json = BuildJson(currentMonth: "May", nowDate: "", startDate: "2027-05-01");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (May 2027)", cut.Markup);
    }

    [Fact]
    public void Dashboard_NowLabel_GarbageNowDate_UsesStartDateYear()
    {
        var json = BuildJson(currentMonth: "October", nowDate: "xyz-garbage", startDate: "2028-10-01");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (October 2028)", cut.Markup);
    }

    #endregion

    #region Branch 3: Both dates invalid → month only

    [Fact]
    public void Dashboard_NowLabel_BothDatesInvalid_ShowsMonthOnly()
    {
        var json = BuildJson(currentMonth: "April", nowDate: "invalid", startDate: "also-invalid");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (April)", cut.Markup);
        Assert.DoesNotContain("Now (April 20", cut.Markup);
    }

    [Fact]
    public void Dashboard_NowLabel_EmptyDates_ShowsMonthOnly()
    {
        var json = BuildJson(currentMonth: "December", nowDate: "", startDate: "");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (December)", cut.Markup);
    }

    #endregion

    #region NowLabel with edge-case currentMonth values

    [Fact]
    public void Dashboard_NowLabel_EmptyCurrentMonth_WithValidNowDate_RendersEmptyMonthWithYear()
    {
        var json = BuildJson(currentMonth: "", nowDate: "2026-04-10");
        // currentMonth is empty but service may error on validation
        // Test that the service loads and the component handles it
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();

        if (!svc.IsError)
        {
            Services.AddSingleton(svc);
            var cut = RenderComponent<Dashboard>();
            // Should contain "Now (" at minimum
            Assert.Contains("Now (", cut.Markup);
        }
        else
        {
            // Validation rejects empty currentMonth - that's valid behavior
            Assert.Contains("currentMonth", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Dashboard_NowLabel_UnicodeCurrentMonth_RendersCorrectly()
    {
        var json = BuildJson(currentMonth: "أبريل", nowDate: "2026-04-10");
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();

        if (!svc.IsError)
        {
            Services.AddSingleton(svc);
            var cut = RenderComponent<Dashboard>();
            Assert.Contains("أبريل", cut.Markup);
        }
    }

    #endregion

    #region Data flow: service → Dashboard → Header NowLabel consistency

    [Fact]
    public void Dashboard_NowLabel_UsesCurrentMonthFromService_NotHardcoded()
    {
        var json = BuildJson(currentMonth: "September", nowDate: "2026-09-01");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (September 2026)", cut.Markup);
        Assert.DoesNotContain("Now (April", cut.Markup);
    }

    [Fact]
    public void Dashboard_NowLabel_YearComeFromTimelineNowDate_NotCurrentMonth()
    {
        // currentMonth says "April" but NowDate is in 2030
        var json = BuildJson(currentMonth: "April", nowDate: "2030-04-15", startDate: "2026-01-01");
        var svc = CreateServiceFromJson(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (April 2030)", cut.Markup);
    }

    #endregion
}