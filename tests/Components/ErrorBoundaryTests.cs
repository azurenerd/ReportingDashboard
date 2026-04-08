using Xunit;
using Bunit;
using AgentSquad.Dashboard.Components;
using Microsoft.AspNetCore.Components;

namespace AgentSquad.Dashboard.Tests.Components;

public class ErrorBoundaryTests : TestContext
{
    [Fact]
    public void ErrorBoundary_DisplaysChildContentByDefault()
    {
        var component = RenderComponent<ErrorBoundary>(
            parameters => parameters
                .Add(p => p.ChildContent, (RenderFragment)(_ => _.AddMarkupContent(0, "<div>Child Content</div>")))
        );

        Assert.Contains("Child Content", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_DisplaysErrorMessageWhenSet()
    {
        var component = RenderComponent<ErrorBoundary>(
            parameters => parameters
                .Add(p => p.ChildContent, (RenderFragment)(_ => _.AddMarkupContent(0, "<div>Child Content</div>")))
        );

        component.Instance.SetError("Test error message");
        component.Render();

        Assert.Contains("Error Loading Dashboard", component.Markup);
        Assert.Contains("Test error message", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_HidesChildContentOnError()
    {
        var component = RenderComponent<ErrorBoundary>(
            parameters => parameters
                .Add(p => p.ChildContent, (RenderFragment)(_ => _.AddMarkupContent(0, "<div>Original Content</div>")))
        );

        component.Instance.SetError("Something went wrong");
        component.Render();

        Assert.DoesNotContain("Original Content", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_DisplaysRefreshButton()
    {
        var component = RenderComponent<ErrorBoundary>(
            parameters => parameters
                .Add(p => p.ChildContent, (RenderFragment)(_ => _.AddMarkupContent(0, "<div>Content</div>")))
        );

        component.Instance.SetError("Error occurred");
        component.Render();

        var refreshButton = component.Find("button.btn.btn-primary");
        Assert.NotNull(refreshButton);
        Assert.Contains("Refresh", refreshButton.InnerHtml);
    }

    [Fact]
    public void ErrorBoundary_RefreshButtonResetsError()
    {
        var component = RenderComponent<ErrorBoundary>(
            parameters => parameters
                .Add(p => p.ChildContent, (RenderFragment)(_ => _.AddMarkupContent(0, "<div>Test Content</div>")))
        );

        component.Instance.SetError("Test error");
        component.Render();
        Assert.Contains("Error Loading Dashboard", component.Markup);

        var refreshButton = component.Find("button.btn.btn-primary");
        refreshButton.Click();
        component.Render();

        Assert.DoesNotContain("Error Loading Dashboard", component.Markup);
    }

    [Fact]
    public void ErrorBoundary_DisplaysDismissableAlert()
    {
        var component = RenderComponent<ErrorBoundary>(
            parameters => parameters
                .Add(p => p.ChildContent, (RenderFragment)(_ => _.AddMarkupContent(0, "<div>Content</div>")))
        );

        component.Instance.SetError("Dismissable error");
        component.Render();

        var alert = component.Find(".alert.alert-dismissible");
        Assert.NotNull(alert);
    }

    [Fact]
    public void ErrorBoundary_DisplaysErrorHeading()
    {
        var component = RenderComponent<ErrorBoundary>(
            parameters => parameters
                .Add(p => p.ChildContent, (RenderFragment)(_ => _.AddMarkupContent(0, "<div>Content</div>")))
        );

        component.Instance.SetError("Test error details");
        component.Render();

        var heading = component.Find(".alert-heading");
        Assert.Contains("Error Loading Dashboard", heading.InnerHtml);
    }
}