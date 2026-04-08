using Bunit;
using AgentSquad.Runner.Components;
using Xunit;

namespace AgentSquad.Runner.Tests.Layout;

public class LayoutTests : TestContext
{
    [Fact]
    public void Layout_DisplaysHeaderWithBrand()
    {
        // Arrange & Act
        var component = RenderComponent<Layout>();

        // Assert
        var markup = component.Markup;
        Assert.Contains("Executive Dashboard", markup);
        Assert.Contains("navbar", markup);
    }

    [Fact]
    public void Layout_DisplaysFooter()
    {
        // Arrange & Act
        var component = RenderComponent<Layout>();

        // Assert
        var markup = component.Markup;
        Assert.Contains("Executive Dashboard © 2024", markup);
        Assert.Contains("footer", markup);
    }

    [Fact]
    public void Layout_HasBodyPlaceholder()
    {
        // Arrange & Act
        var component = RenderComponent<Layout>();

        // Assert
        var markup = component.Markup;
        Assert.Contains("<main", markup);
    }

    [Fact]
    public void Layout_DarkHeaderStyling()
    {
        // Arrange & Act
        var component = RenderComponent<Layout>();

        // Assert
        var header = component.Find("header");
        var classList = header.GetAttribute("class");
        Assert.Contains("navbar-dark", classList);
        Assert.Contains("bg-dark", classList);
    }
}