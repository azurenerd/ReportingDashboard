using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Components;

[Trait("Category", "Unit")]
public class DashboardRenderTests
{
    [Fact]
    public void Dashboard_WithValidData_RendersHeaderTimelineAndHeatmap()
    {
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<IDashboardDataService>(FakeDashboardDataService.WithSample());

        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Find("header.hdr").Should().NotBeNull();
        cut.Find(".tl-area").Should().NotBeNull();
        cut.Find(".hm-grid").Should().NotBeNull();
        cut.Find(".hdr h1").TextContent.Should().Contain("Sample Project");
        cut.FindAll(".error-banner").Should().BeEmpty();
        cut.FindAll("div.hm-grid > div").Count.Should().Be(25);
    }

    [Fact]
    public void Dashboard_WithError_RendersErrorBanner_AndPlaceholderLayout()
    {
        var err = new DashboardLoadError(
            FilePath: "wwwroot/data.json",
            Message: "Unexpected end of JSON input",
            Line: 42, Column: 3,
            Kind: DashboardLoadErrorKind.ParseError);

        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<IDashboardDataService>(FakeDashboardDataService.WithError(err));

        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        var banner = cut.Find(".error-banner");
        banner.GetAttribute("role").Should().Be("alert");
        banner.TextContent.Should().Contain("wwwroot/data.json");
        banner.TextContent.Should().Contain("parse error");
        banner.TextContent.Should().Contain("line 42");
        banner.TextContent.Should().Contain("Unexpected end of JSON input");

        // Layout bands still present so page preserves 1920x1080.
        cut.Find("header.hdr").Should().NotBeNull();
        cut.Find(".tl-area").Should().NotBeNull();
        cut.Find(".hm-grid").Should().NotBeNull();
        cut.Find(".hdr h1").TextContent.Should().Contain("(data.json error)");
    }

    [Fact]
    public void Dashboard_WithNotFoundError_RendersNotFoundKind()
    {
        var err = new DashboardLoadError(
            FilePath: "wwwroot/data.json",
            Message: "data.json not found at wwwroot/data.json.",
            Line: null, Column: null,
            Kind: DashboardLoadErrorKind.NotFound);

        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<IDashboardDataService>(FakeDashboardDataService.WithError(err));

        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();
        var banner = cut.Find(".error-banner");
        banner.TextContent.Should().Contain("not found");
        cut.FindAll(".error-location").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_HasNoInteractiveArtifacts()
    {
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<IDashboardDataService>(FakeDashboardDataService.WithSample());

        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();
        cut.Markup.Should().NotContain("blazor.server.js");
        cut.Markup.Should().NotContain("render-mode=");
    }
}
