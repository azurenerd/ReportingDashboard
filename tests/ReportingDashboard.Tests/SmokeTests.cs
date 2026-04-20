using Microsoft.Extensions.DependencyInjection;

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
        using var svc = new DashboardDataService();

        svc.Current.Should().NotBeNull();
        svc.Current.Model.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_Stub_Reload_RaisesOnChanged()
    {
        using var svc = new DashboardDataService();
        IDashboardDataService iface = svc;
        var fired = 0;
        iface.OnChanged += () => fired++;

        iface.Reload();

        fired.Should().Be(1);
    }

    [Fact]
    public void ParseError_Record_RoundTrip()
    {
        var err = new ParseError(42, 7, "msg", "$.path");

        err.Line.Should().Be(42);
        err.Column.Should().Be(7);
        err.Message.Should().Be("msg");
        err.JsonPath.Should().Be("$.path");
    }

    [Fact]
    public void Index_Renders_WithoutThrowing()
    {
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<IDashboardDataService, DashboardDataService>();

        var cut = ctx.RenderComponent<ReportingDashboard.Web.Pages.Index>();

        cut.Markup.Should().NotBeNullOrWhiteSpace();
        cut.Markup.Should().Contain("hdr");
        cut.Markup.Should().Contain("tl-area");
        cut.Markup.Should().Contain("hm-wrap");
    }
}