using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardComponentTests : TestContext
{
    private static IConfiguration BuildConfig(string? path = null)
    {
        var dict = new Dictionary<string, string?>();
        if (path != null) dict["DashboardDataPath"] = path;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static async Task<string> WriteTempJsonAsync(string json)
    {
        var f = Path.GetTempFileName();
        await File.WriteAllTextAsync(f, json);
        return f;
    }

    [Fact]
    public void Dashboard_WhenServiceThrowsFileNotFoundException_RendersErrorContainer()
    {
        var service = new DashboardDataService(BuildConfig("nonexistent/data.json"));
        Services.AddScoped(_ => service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("error-container");
        cut.Markup.Should().Contain("Unable to load dashboard data.");
        cut.Markup.Should().Contain("Please check that data.json exists and contains valid JSON.");
    }

    [Fact]
    public async Task Dashboard_WhenServiceThrowsJsonException_RendersErrorDetailWithMessage()
    {
        var tempFile = await WriteTempJsonAsync("{invalid json,,}");
        try
        {
            var service = new DashboardDataService(BuildConfig(tempFile));
            Services.AddScoped(_ => service);

            var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

            cut.WaitForAssertion(() => cut.Markup.Should().Contain("error-container"));
            cut.Markup.Should().Contain("error-detail");
        }
        finally { File.Delete(tempFile); }
    }

    [Fact]
    public async Task Dashboard_WhenServiceReturnsData_DoesNotRenderErrorContainer()
    {
        var json = """
            {
              "header": { "title": "Test Project", "backlogLink": "#" },
              "timelineTracks": [],
              "heatmap": { "columns": [], "highlightColumnIndex": 0, "rows": [] }
            }
            """;
        var tempFile = await WriteTempJsonAsync(json);
        try
        {
            var service = new DashboardDataService(BuildConfig(tempFile));
            Services.AddScoped(_ => service);

            var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

            // Wait for OnInitializedAsync to complete (render count reaches 2)
            cut.WaitForState(() => cut.RenderCount >= 2);

            cut.Markup.Should().NotContain("error-container");
            cut.Markup.Should().NotContain("Unable to load dashboard data.");
        }
        finally { File.Delete(tempFile); }
    }

    [Fact]
    public void Dashboard_WhenFileNotFound_ErrorDetailContainsExceptionMessage()
    {
        var service = new DashboardDataService(BuildConfig("missing/path/data.json"));
        Services.AddScoped(_ => service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".error-detail").TextContent.Should().NotBeNullOrWhiteSpace();
        cut.FindAll(".error-container").Should().HaveCount(1);
    }

    [Fact]
    public async Task Dashboard_WhenServiceReturnsData_NoH2ErrorHeading()
    {
        var json = """
            {
              "header": { "title": "Test Project", "backlogLink": "#" },
              "timelineTracks": [],
              "heatmap": { "columns": [], "highlightColumnIndex": 0, "rows": [] }
            }
            """;
        var tempFile = await WriteTempJsonAsync(json);
        try
        {
            var service = new DashboardDataService(BuildConfig(tempFile));
            Services.AddScoped(_ => service);

            var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

            // Wait for OnInitializedAsync to complete (render count reaches 2)
            cut.WaitForState(() => cut.RenderCount >= 2);

            cut.FindAll("h2").Should().NotContain(h => h.TextContent.Contains("Unable to load dashboard data."));
        }
        finally { File.Delete(tempFile); }
    }
}