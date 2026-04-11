using Bunit;
using ReportingDashboard.Components.Sections;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class ErrorPanelTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersErrorMessage()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "File not found"));

        Assert.Contains("File not found", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersStaticTitle()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "some error"));

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersHelpText()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "err"));

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersWarningIcon()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "err"));

        Assert.Contains("error-icon", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_NullErrorMessage_RendersWithoutCrash()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, null));

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_EmptyErrorMessage_RendersPanel()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, ""));

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("error-details", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HasCorrectCssClasses()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test"));

        var panel = cut.Find(".error-panel");
        Assert.NotNull(panel);

        var content = cut.Find(".error-panel-content");
        Assert.NotNull(content);
    }

    [Fact]
    public void ErrorPanel_LongErrorMessage_RendersCompletely()
    {
        var longMessage = new string('x', 2000);
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, longMessage));

        Assert.Contains(longMessage, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_SpecialCharacters_RendersEncoded()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Error: <script>alert('xss')</script>"));

        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }
}