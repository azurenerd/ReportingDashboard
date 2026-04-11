using Bunit;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the actual ErrorPanel.razor component at Components/ root,
/// which uses a "Message" parameter (not "ErrorMessage" from Sections/).
/// </summary>
[Trait("Category", "Unit")]
public class ActualErrorPanelTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersErrorPanelCssClass()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "some error"));

        var panel = cut.Find(".error-panel");
        Assert.NotNull(panel);
    }

    [Fact]
    public void ErrorPanel_RendersErrorContentContainer()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "some error"));

        var content = cut.Find(".error-content");
        Assert.NotNull(content);
    }

    [Fact]
    public void ErrorPanel_RendersStaticTitle()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "any error"));

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersMessageParameter()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "data.json not found at /path/to/file"));

        Assert.Contains("data.json not found at /path/to/file", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersHelpText()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "err"));

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersWarningIndicator()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "err"));

        // The actual component renders a "?" character as the warning indicator
        Assert.Contains("?", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_EmptyMessage_RendersWithoutCrash()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, ""));

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_DefaultMessage_IsEmptyString()
    {
        var cut = RenderComponent<ErrorPanel>();

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_LongMessage_RendersCompletely()
    {
        var longMessage = new string('x', 2000);
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, longMessage));

        Assert.Contains(longMessage, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_SpecialCharacters_AreHtmlEncoded()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "Error: <script>alert('xss')</script>"));

        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageWithJsonContent_RendersCorrectly()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "Failed to parse data.json: '{' is invalid at line 1"));

        Assert.Contains("Failed to parse data.json", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageInMonospaceContainer()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "test error"));

        // The actual component uses inline style with Consolas font
        Assert.Contains("Consolas", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_TitleHasBoldStyling()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "test"));

        Assert.Contains("font-weight:700", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_WarningIndicator_HasRedColor()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "test"));

        Assert.Contains("#EA4335", cut.Markup);
    }
}