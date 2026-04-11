using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Layout;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardLayoutTests : TestContext
{
    [Fact]
    public void Renders_BodyContent()
    {
        var cut = RenderComponent<DashboardLayout>(parameters =>
            parameters.AddChildContent("<p>Test Content</p>"));

        cut.Markup.Should().Contain("Test Content");
    }

    [Fact]
    public void DoesNot_RenderNavMenu()
    {
        var cut = RenderComponent<DashboardLayout>(parameters =>
            parameters.AddChildContent("<p>Content</p>"));

        cut.Markup.Should().NotContain("NavMenu");
        cut.Markup.Should().NotContain("sidebar");
    }

    [Fact]
    public void DoesNot_RenderFooter()
    {
        var cut = RenderComponent<DashboardLayout>(parameters =>
            parameters.AddChildContent("<p>Content</p>"));

        cut.Markup.Should().NotContain("footer");
    }

    [Fact]
    public void Renders_OnlyBody()
    {
        var cut = RenderComponent<DashboardLayout>(parameters =>
            parameters.AddChildContent("<div class='my-content'>Hello</div>"));

        // The layout should contain only the child content (Body)
        cut.Find(".my-content").TextContent.Should().Be("Hello");
    }
}