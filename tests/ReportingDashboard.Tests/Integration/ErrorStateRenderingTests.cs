using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class ErrorStateRenderingTests
{
    [Fact]
    public async Task Dashboard_RendersErrorPanel_WhenFileNotFound()
    {
        var service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);
        await service.LoadAsync(Path.Combine(Path.GetTempPath(), "__nonexistent_data__.json"));

        using var ctx = new TestContext();
        ctx.Services.AddSingleton(service);

        var cut = ctx.RenderComponent<Dashboard>();

        var errorPanel = cut.Find(".error-panel");
        Assert.NotNull(errorPanel);
        Assert.Contains("not found", cut.Markup);
        Assert.Empty(cut.FindAll(".hdr"));
        Assert.Empty(cut.FindAll(".tl-area"));
        Assert.Empty(cut.FindAll(".hm-wrap"));
    }

    [Fact]
    public async Task Dashboard_RendersErrorPanel_WhenJsonMalformed()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_bad_{Guid.NewGuid()}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "{ this is not valid json }}}");
            var service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);
            await service.LoadAsync(tempFile);

            using var ctx = new TestContext();
            ctx.Services.AddSingleton(service);

            var cut = ctx.RenderComponent<Dashboard>();

            cut.Find(".error-panel");
            Assert.Contains("Failed to parse", cut.Markup);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Dashboard_RendersErrorPanel_WhenValidationFails()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_invalid_{Guid.NewGuid()}.json");
        try
        {
            // Valid JSON but missing required fields
            await File.WriteAllTextAsync(tempFile, """
            {
                "title": "",
                "subtitle": "",
                "backlogLink": "",
                "currentMonth": "",
                "months": [],
                "timeline": { "startDate": "", "endDate": "", "nowDate": "", "tracks": [] },
                "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
            }
            """);
            var service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);
            await service.LoadAsync(tempFile);

            using var ctx = new TestContext();
            ctx.Services.AddSingleton(service);

            var cut = ctx.RenderComponent<Dashboard>();

            cut.Find(".error-panel");
            Assert.Contains("validation", cut.Markup);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task Dashboard_RendersSections_WhenDataIsValid()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_valid_{Guid.NewGuid()}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, GetValidDataJson());
            var service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);
            await service.LoadAsync(tempFile);

            using var ctx = new TestContext();
            ctx.Services.AddSingleton(service);

            var cut = ctx.RenderComponent<Dashboard>();

            cut.Find(".hdr");
            cut.Find(".tl-area");
            cut.Find(".hm-wrap");
            Assert.Empty(cut.FindAll(".error-panel"));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ErrorPanel_RendersAllExpectedElements()
    {
        using var ctx = new TestContext();
        var cut = ctx.RenderComponent<ErrorPanel>(parameters => parameters
            .Add(p => p.ErrorMessage, "data.json not found at /tmp/missing.json"));

        var panel = cut.Find(".error-panel");
        Assert.NotNull(panel);

        cut.Find(".error-icon");
        cut.Find(".error-title");
        cut.Find(".error-details");
        cut.Find(".error-help");

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
        Assert.Contains("data.json not found at /tmp/missing.json", cut.Markup);
        Assert.Contains("Check data.json for errors and restart the application.", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersNullMessageGracefully()
    {
        using var ctx = new TestContext();
        var cut = ctx.RenderComponent<ErrorPanel>(parameters => parameters
            .Add(p => p.ErrorMessage, (string?)null));

        cut.Find(".error-panel");
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    private static string GetValidDataJson() => """
    {
      "title": "Test Dashboard",
      "subtitle": "Test Team · Test Workstream · April 2026",
      "backlogLink": "https://example.com/backlog",
      "currentMonth": "Apr",
      "months": ["Mar", "Apr"],
      "timeline": {
        "startDate": "2026-01-01",
        "endDate": "2026-06-30",
        "nowDate": "2026-04-10",
        "tracks": [
          {
            "name": "M1",
            "label": "Test Track",
            "color": "#0078D4",
            "milestones": [
              { "date": "2026-03-01", "type": "checkpoint", "label": "Mar 1" }
            ]
          }
        ]
      },
      "heatmap": {
        "shipped": { "mar": ["Item A"], "apr": [] },
        "inProgress": { "mar": [], "apr": ["Item B"] },
        "carryover": { "mar": [], "apr": [] },
        "blockers": { "mar": [], "apr": [] }
      }
    }
    """;
}