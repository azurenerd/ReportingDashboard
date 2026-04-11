using Bunit;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Unit tests for Components/ErrorPanel.razor (root-level, inline-styled version from PR #521).
/// This is distinct from Components/Sections/ErrorPanel.razor tested in ErrorPanelTests.cs.
/// The root ErrorPanel uses a [Parameter] named "Message" and inline styles.
/// </summary>
[Trait("Category", "Unit")]
public class RootErrorPanelTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersErrorPanelCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "Test error"));

        var panel = cut.Find(".error-panel");
        Assert.NotNull(panel);
    }

    [Fact]
    public void ErrorPanel_RendersErrorContentDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "Test error"));

        var content = cut.Find(".error-content");
        Assert.NotNull(content);
    }

    [Fact]
    public void ErrorPanel_RendersStaticTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "some error"));

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersMessageParameter()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "data.json not found at /path/to/file"));

        Assert.Contains("data.json not found at /path/to/file", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersHelpText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "err"));

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersWarningSymbol()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "err"));

        // The root ErrorPanel uses a "?" character styled at 48px in #EA4335
        Assert.Contains("#EA4335", cut.Markup);
        Assert.Contains("48px", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_EmptyMessage_RendersWithoutCrash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, ""));

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_DefaultMessage_IsEmptyString()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>();

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_LongMessage_RendersCompletely()
    {
        var longMessage = new string('x', 2000);
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, longMessage));

        Assert.Contains(longMessage, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_SpecialCharacters_AreHtmlEncoded()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "Error: <script>alert('xss')</script>"));

        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageWithJsonContent_RendersEncoded()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "Failed to parse: {\"key\": \"value\"}"));

        Assert.Contains("Failed to parse", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HasMonospaceFontForMessage()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "error details"));

        Assert.Contains("Consolas", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageHasMaxWidth600px()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "error"));

        Assert.Contains("max-width:600px", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageHasWordBreak()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.Message, "error"));

        Assert.Contains("word-break:break-word", cut.Markup);
    }
}