using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Pages;
using IndexPage = ReportingDashboard.Web.Pages.Index;

namespace ReportingDashboard.Tests;

public class SmokeTests
{
    [Fact]
    public void SolutionBuilds_Smoke()
    {
        true.Should().BeTrue();
    }

    [Fact]
    public void DashboardDataService_Stub_Current_IsNotNull()
    {
        var svc = new DashboardDataService();
        svc.Current.Should().NotBeNull();
        svc.Current.Model.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_Stub_Reload_RaisesOnChangedAtMostOnce()
    {
        var svc = new DashboardDataService();
        var count = 0;
        ((IDashboardDataService)svc).OnChanged += () => count++;

        svc.Reload();

        count.Should().Be(1);
    }

    [Fact]
    public void Index_Renders_WithoutThrowing()
    {
        using var ctx = new TestContext();
        ctx.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

        var cut = ctx.RenderComponent<IndexPage>();

        cut.Markup.Should().NotBeNullOrWhiteSpace();
        cut.Markup.Should().Contain("hdr");
        cut.Markup.Should().Contain("tl-area");
        cut.Markup.Should().Contain("hm-wrap");
    }

    [Fact]
    public void ParseError_Record_RoundTrip()
    {
        var pe = new ParseError(42, 7, "msg", "$.path");
        pe.Line.Should().Be(42);
        pe.Column.Should().Be(7);
        pe.Message.Should().Be("msg");
        pe.JsonPath.Should().Be("$.path");
    }
}