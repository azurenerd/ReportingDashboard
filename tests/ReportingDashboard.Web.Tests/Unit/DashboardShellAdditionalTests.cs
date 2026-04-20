using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardShellAdditionalTests
{
    [Fact]
    public void Dashboard_RendersHeaderH1AndSubtitle()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Find("div.hdr h1").TextContent.Should().Be("Reporting Dashboard");
        cut.Find("div.sub").TextContent.Should().Contain("Static SSR shell");
    }

    [Fact]
    public void Dashboard_HasSingleTlSvgBoxInsideTlArea()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.FindAll("div.tl-area div.tl-svg-box").Count.Should().Be(1);
    }

    [Fact]
    public void Dashboard_HasCornerCellWithStatusText()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        var corners = cut.FindAll("div.hm-corner");
        corners.Count.Should().Be(1);
        corners[0].TextContent.Should().Be("Status");
    }

    [Fact]
    public void Dashboard_TotalHeatmapGridChildren_Equals25()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        // 1 corner + 4 col-hdr + (1 row-hdr + 4 cells) * 4 rows = 25
        cut.FindAll("div.hm-grid > div").Count.Should().Be(25);
    }

    [Fact]
    public void Dashboard_DoesNotContainAprLegacyClass()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().NotContain("apr-hdr");
        cut.Markup.Should().NotContain(" apr ");
        cut.Markup.Should().NotContain("\"apr\"");
    }
}