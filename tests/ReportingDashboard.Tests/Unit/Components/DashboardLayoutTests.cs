using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Layout;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardLayoutTests : TestContext
{
    [Fact]
    public void DashboardLayout_RendersChildContent()
    {
        // DashboardLayout inherits LayoutComponentBase which uses Body, not ChildContent
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<div class='test-child'>Hello</div>");
            }));

        cut.Markup.Should().Contain("test-child");
        cut.Markup.Should().Contain("Hello");
    }

    [Fact]
    public void DashboardLayout_DoesNotRenderNavMenu()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<p>Content</p>");
            }));

        cut.Markup.Should().NotContain("NavMenu");
        cut.Markup.Should().NotContain("sidebar");
    }

    [Fact]
    public void DashboardLayout_DoesNotRenderFooter()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<p>Content</p>");
            }));

        cut.Markup.Should().NotContain("footer");
    }

    [Fact]
    public void DashboardLayout_NoDefaultBlazorChrome()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<p>Content</p>");
            }));

        cut.Markup.Should().NotContain("top-row");
        cut.Markup.Should().NotContain("main-layout");
    }
}