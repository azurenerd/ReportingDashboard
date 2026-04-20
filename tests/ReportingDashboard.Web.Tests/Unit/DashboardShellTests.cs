using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardShellTests
{
    private static Bunit.TestContext NewCtx()
    {
        var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<IDashboardDataService>(FakeDashboardDataService.WithSample());
        return ctx;
    }

    [Fact]
    public void Dashboard_Renders_AllLayoutBands()
    {
        using var ctx = NewCtx();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("class=\"hdr\"");
        cut.Markup.Should().Contain("class=\"tl-area\"");
        cut.Markup.Should().Contain("class=\"hm-wrap\"");
        cut.Markup.Should().Contain("class=\"hm-grid\"");
    }

    [Fact]
    public void Dashboard_Renders_ExactlyOneHmTitle_WithLiteralText()
    {
        using var ctx = NewCtx();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        var titles = cut.FindAll("div.hm-title");
        titles.Count.Should().Be(1);
        titles[0].TextContent.Should().Contain("Monthly Execution Heatmap");
        titles[0].TextContent.Should().Contain("Shipped");
        titles[0].TextContent.Should().Contain("In Progress");
        titles[0].TextContent.Should().Contain("Carryover");
        titles[0].TextContent.Should().Contain("Blockers");
    }

    [Fact]
    public void Dashboard_HasNo_InteractiveBlazorArtifacts()
    {
        using var ctx = NewCtx();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().NotContain("blazor.server.js");
        cut.Markup.Should().NotContain("blazor.web.js");
        cut.Markup.Should().NotContain("components-reconnect-modal");
        cut.Markup.Should().NotContain("@rendermode");
        cut.Markup.Should().NotContain("render-mode=");
    }

    [Fact]
    public void Dashboard_Heatmap_RendersExpectedCategoryCells()
    {
        using var ctx = NewCtx();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.FindAll("div.hm-cell.ship-cell").Count.Should().Be(4);
        cut.FindAll("div.hm-cell.prog-cell").Count.Should().Be(4);
        cut.FindAll("div.hm-cell.carry-cell").Count.Should().Be(4);
        cut.FindAll("div.hm-cell.block-cell").Count.Should().Be(4);
        cut.FindAll("div.hm-col-hdr").Count.Should().Be(4);
    }

    [Fact]
    public void Dashboard_RowHeaders_AllFourCategoriesPresent()
    {
        using var ctx = NewCtx();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Find("div.hm-row-hdr.ship-hdr").TextContent.Should().Be("Shipped");
        cut.Find("div.hm-row-hdr.prog-hdr").TextContent.Should().Be("In Progress");
        cut.Find("div.hm-row-hdr.carry-hdr").TextContent.Should().Be("Carryover");
        cut.Find("div.hm-row-hdr.block-hdr").TextContent.Should().Be("Blockers");
    }
}