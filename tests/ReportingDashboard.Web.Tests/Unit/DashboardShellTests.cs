using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Web.Components.Pages;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Unit;

public class DashboardShellTests
{
    private static Bunit.TestContext CreateContext(DashboardLoadResult result)
    {
        var ctx = new Bunit.TestContext();
        var svc = new Mock<IDashboardDataService>();
        svc.Setup(s => s.GetCurrent()).Returns(result);
        ctx.Services.AddSingleton<IDashboardDataService>(svc.Object);
        return ctx;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_ErrorBanner_WhenErrorPresent()
    {
        var err = new DashboardLoadError("p.json", "boom", null, null, "NotFound");
        var result = new DashboardLoadResult(null, err, DateTimeOffset.UtcNow);
        using var ctx = CreateContext(result);

        var cut = ctx.RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DoesNotRender_ErrorBanner_WhenNoError()
    {
        var result = new DashboardLoadResult(null, null, DateTimeOffset.UtcNow);
        using var ctx = CreateContext(result);

        var cut = ctx.RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Renders_AllChildComponents_Unconditionally_OnError()
    {
        var err = new DashboardLoadError("p", "m", null, null, "ParseError");
        var result = new DashboardLoadResult(null, err, DateTimeOffset.UtcNow);
        using var ctx = CreateContext(result);

        var cut = ctx.RenderComponent<Dashboard>();

        cut.Markup.Should().NotBeNullOrWhiteSpace();
        cut.FindComponents<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>().Should().HaveCount(1);
        cut.FindComponents<ReportingDashboard.Web.Components.Pages.Partials.TimelineSvg>().Should().HaveCount(1);
        cut.FindComponents<ReportingDashboard.Web.Components.Pages.Partials.Heatmap>().Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardHeader_Receives_NowLabel_Parameter()
    {
        var result = new DashboardLoadResult(null, null, DateTimeOffset.UtcNow);
        using var ctx = CreateContext(result);

        var cut = ctx.RenderComponent<Dashboard>();
        var headerComp = cut.FindComponent<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>();

        headerComp.Instance.NowLabel.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UsesProjectPlaceholder_WhenDataIsNull()
    {
        var err = new DashboardLoadError("p", "m", null, null, "NotFound");
        var result = new DashboardLoadResult(null, err, DateTimeOffset.UtcNow);
        using var ctx = CreateContext(result);

        var cut = ctx.RenderComponent<Dashboard>();
        var headerComp = cut.FindComponent<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>();

        headerComp.Instance.Project.Should().BeSameAs(Project.Placeholder);
    }
}