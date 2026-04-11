using Bunit;
using ReportingDashboard.Components.Layout;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardLayoutTests : TestContext
{
    [Fact]
    public void DashboardLayout_RendersChildContent()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.AddChildContent("<p>Test child content</p>"));

        Assert.Contains("Test child content", cut.Markup);
    }

    [Fact]
    public void DashboardLayout_DoesNotRenderNavSidebar()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.AddChildContent("<p>Content</p>"));

        Assert.DoesNotContain("<nav", cut.Markup);
    }

    [Fact]
    public void DashboardLayout_DoesNotRenderFooter()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.AddChildContent("<p>Content</p>"));

        Assert.DoesNotContain("<footer", cut.Markup);
    }

    [Fact]
    public void DashboardLayout_DoesNotRenderDefaultBlazorChrome()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.AddChildContent("<p>Content</p>"));

        // Should not have default Blazor sidebar, top-row, etc.
        Assert.DoesNotContain("sidebar", cut.Markup.ToLower());
        Assert.DoesNotContain("top-row", cut.Markup);
    }

    [Fact]
    public void DashboardLayout_RendersEmptyBody()
    {
        var cut = RenderComponent<DashboardLayout>(p =>
            p.AddChildContent(""));

        // Should render without error even with empty content
        Assert.NotNull(cut.Markup);
    }
}