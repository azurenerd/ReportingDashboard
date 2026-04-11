using Bunit;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the root-level Components/ErrorPanel.razor (uses "Message" parameter),
/// distinct from Sections/ErrorPanel.razor (uses "ErrorMessage" parameter).
/// </summary>
[Trait("Category", "Unit")]
public class ErrorPanelComponentTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersErrorPanelContainer()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "Something went wrong"));

        var panel = cut.Find(".error-panel");
        Assert.NotNull(panel);
    }

    [Fact]
    public void ErrorPanel_RendersErrorContentContainer()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "test error"));

        var content = cut.Find(".error-content");
        Assert.NotNull(content);
    }

    [Fact]
    public void ErrorPanel_RendersWarningIconCharacter()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "error"));

        // The component renders a "?" character at 48px in #EA4335
        Assert.Contains("48px", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersStaticTitle()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "any error"));

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersTitleWith20pxBoldFont()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "err"));

        Assert.Contains("font-size:20px", cut.Markup);
        Assert.Contains("font-weight:700", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersMessageInMonospaceFont()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "custom error detail"));

        Assert.Contains("Consolas", cut.Markup);
        Assert.Contains("custom error detail", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersHelpText()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "err"));

        Assert.Contains("Check data.json for errors and restart the application.", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HelpTextHas12pxFont()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "err"));

        Assert.Contains("font-size:12px", cut.Markup);
        Assert.Contains("color:#888", cut.Markup);
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
        var longMsg = new string('A', 3000);
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, longMsg));

        Assert.Contains(longMsg, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageWithSpecialChars_HtmlEncoded()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "<script>alert('xss')</script>"));

        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageWithFilePath_RendersPath()
    {
        var path = @"C:\wwwroot\data.json";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, $"data.json not found at {path}"));

        Assert.Contains("data.json not found at", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageWith14pxFontSize()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "detail text"));

        Assert.Contains("font-size:14px", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageAreaHasWordBreak()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "test"));

        Assert.Contains("word-break:break-word", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageContainingJsonError_Renders()
    {
        var jsonError = "Failed to parse data.json: '$' is an invalid start of a property name. Expected a '\"'. Path: $ | LineNumber: 0 | BytePositionInLine: 1.";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, jsonError));

        Assert.Contains("Failed to parse data.json", cut.Markup);
    }
}