using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using ReportingDashboard.Components.Layout;

namespace ReportingDashboard.Tests;

/// <summary>
/// Integration tests verifying DashboardLayout renders correctly with child components.
/// Uses a LayoutWrapper pattern instead of directly setting Body (which is not a [Parameter]).
/// </summary>
public class DashboardLayoutIntegrationTests : TestContext
{
    [Fact]
    public void DashboardLayout_WithHtmlContent_RendersCorrectly()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, "<h1>Project Dashboard</h1><div class=\"hdr\">Header</div>"));

        cut.Markup.Should().Contain("Project Dashboard");
        cut.Markup.Should().Contain("hdr");
    }

    [Fact]
    public void DashboardLayout_WithEmptyBody_RendersViewport()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, ""));

        var viewport = cut.Find(".dashboard-viewport");
        viewport.Should().NotBeNull();
        viewport.InnerHtml.Trim().Should().BeEmpty();
    }

    [Fact]
    public void DashboardLayout_WithMultipleChildren_RendersAll()
    {
        var markup = "<div class=\"hdr\">Header</div><div class=\"tl-area\">Timeline</div><div class=\"hm-wrap\">Heatmap</div>";
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, markup));

        cut.FindAll(".dashboard-viewport > div").Should().HaveCount(3);
    }

    [Fact]
    public void DashboardLayout_DoesNotIncludeDefaultBlazorChrome()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, "<p>content</p>"));

        cut.FindAll(".sidebar").Should().BeEmpty();
        cut.FindAll(".top-row").Should().BeEmpty();
        cut.FindAll("nav").Should().BeEmpty();
        cut.FindAll("footer").Should().BeEmpty();
    }

    /// <summary>
    /// Helper that renders child markup inside DashboardLayout by setting Body
    /// through the component render tree (not as a [Parameter] on LayoutComponentBase).
    /// </summary>
    private class LayoutWrapper : ComponentBase
    {
        [Parameter] public string ChildMarkup { get; set; } = "";

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenComponent<DashboardLayout>(0);
            builder.AddAttribute(1, "Body", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddMarkupContent(0, ChildMarkup);
            }));
            builder.CloseComponent();
        }
    }
}