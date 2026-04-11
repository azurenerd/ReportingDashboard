using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Layout;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardLayoutIntegrationTests : TestContext
{
    [Fact]
    public void DashboardLayout_RendersChildContent()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<div id='test-content'>Dashboard Content</div>");
            }));

        cut.Markup.Should().Contain("test-content");
        cut.Markup.Should().Contain("Dashboard Content");
    }

    [Fact]
    public void DashboardLayout_DoesNotIncludeNavMenu()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<p>Content</p>");
            }));

        cut.Markup.Should().NotContain("NavMenu");
        cut.Markup.Should().NotContain("sidebar");
        cut.Markup.Should().NotContain("nav-menu");
    }

    [Fact]
    public void DashboardLayout_DoesNotIncludeFooter()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<p>Content</p>");
            }));

        cut.Markup.Should().NotContain("footer");
    }

    [Fact]
    public void DashboardLayout_DoesNotIncludeDefaultBlazorChrome()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0, "<p>Content</p>");
            }));

        cut.Markup.Should().NotContain("top-row");
        cut.Markup.Should().NotContain("main-layout");
        cut.Markup.Should().NotContain("page");
    }

    [Fact]
    public void DashboardLayout_MultipleChildElements_AllRendered()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.Add(c => c.Body, builder =>
            {
                builder.AddMarkupContent(0,
                    "<div class='hdr'>Header</div>" +
                    "<div class='tl-area'>Timeline</div>" +
                    "<div class='hm-wrap'>Heatmap</div>");
            }));

        cut.Find(".hdr").TextContent.Should().Be("Header");
        cut.Find(".tl-area").TextContent.Should().Be("Timeline");
        cut.Find(".hm-wrap").TextContent.Should().Be("Heatmap");
    }
}