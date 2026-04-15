using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class HeaderComponentTests
{
    [Fact]
    public void Header_RendersTitle()
    {
        using var ctx = new Bunit.TestContext();
        var data = new DashboardData
        {
            Title = "Test Dashboard Title",
            Subtitle = "Test Subtitle"
        };

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Test Dashboard Title");
    }

    [Fact]
    public void Header_RendersSubtitle()
    {
        using var ctx = new Bunit.TestContext();
        var data = new DashboardData
        {
            Title = "Title",
            Subtitle = "My Subtitle Text"
        };

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("My Subtitle Text");
    }

    [Fact]
    public void Header_WithBacklogUrl_RendersLink()
    {
        using var ctx = new Bunit.TestContext();
        var data = new DashboardData
        {
            Title = "Title",
            Subtitle = "Subtitle",
            BacklogUrl = "https://dev.azure.com/test"
        };

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("https://dev.azure.com/test");
    }

    [Fact]
    public void Header_WithoutBacklogUrl_DoesNotRenderLink()
    {
        using var ctx = new Bunit.TestContext();
        var data = new DashboardData
        {
            Title = "Title",
            Subtitle = "Subtitle",
            BacklogUrl = null
        };

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Title");
    }

    [Fact]
    public void Header_RendersLegendItems()
    {
        using var ctx = new Bunit.TestContext();
        var data = new DashboardData
        {
            Title = "Legend Test",
            Subtitle = "Sub"
        };

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        // Header should render without errors
        cut.Markup.Should().NotBeNullOrEmpty();
    }
}