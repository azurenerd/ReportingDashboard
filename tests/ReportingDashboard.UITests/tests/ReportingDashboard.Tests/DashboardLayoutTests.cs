using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using ReportingDashboard.Components.Layout;

namespace ReportingDashboard.Tests;

/// <summary>
/// Tests for DashboardLayout.razor.
/// LayoutComponentBase.Body is NOT a [Parameter] — it is framework-injected.
/// We use a LayoutWrapper that renders DashboardLayout with Body set via the render tree.
/// </summary>
public class DashboardLayoutTests : TestContext
{
    [Fact]
    public void DashboardLayout_RendersBodyContent()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, "<p>Test Content</p>"));

        cut.Markup.Should().Contain("Test Content");
    }

    [Fact]
    public void DashboardLayout_ContainsViewportDiv()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, "<span>inner</span>"));

        var viewport = cut.Find(".dashboard-viewport");
        viewport.Should().NotBeNull();
    }

    [Fact]
    public void DashboardLayout_RendersWithoutNavSidebar()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, "<div>dashboard</div>"));

        cut.FindAll("nav").Should().BeEmpty();
    }

    [Fact]
    public void DashboardLayout_RendersWithoutFooter()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, "<div>dashboard</div>"));

        cut.FindAll("footer").Should().BeEmpty();
    }

    [Fact]
    public void DashboardLayout_BodyContentInsideViewportDiv()
    {
        var cut = RenderComponent<LayoutWrapper>(parameters => parameters
            .Add(p => p.ChildMarkup, "<div class=\"test-child\">hello</div>"));

        var viewport = cut.Find(".dashboard-viewport");
        viewport.InnerHtml.Should().Contain("test-child");
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