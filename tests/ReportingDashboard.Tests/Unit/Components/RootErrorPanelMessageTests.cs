using Bunit;
using Xunit;
using ReportingDashboard.Components;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class RootErrorPanelMessageTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersErrorMessage_WhenProvided()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "data.json not found at /app/wwwroot/data.json"));

        cut.Markup.Contains("data.json not found at /app/wwwroot/data.json");
    }

    [Fact]
    public void ErrorPanel_RendersStaticTitle()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Some error"));

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersHelpText()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Some error"));

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersConsoleOutputText()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Some error"));

        Assert.Contains("See console output for details", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersRedIndicator()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Some error"));

        Assert.Contains("?", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HandlesNullMessage_WithoutCrashing()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, (string)null!));

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HandlesEmptyMessage_WithoutCrashing()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, string.Empty));

        Assert.Contains("error-panel", cut.Markup);
    }
}