using Bunit;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Components/ErrorPanel.razor (root-level, non-Sections version).
/// This component has different markup from Components/Sections/ErrorPanel.razor:
/// no error-panel-content wrapper, uses error-icon/error-title/error-details/error-help classes.
/// </summary>
[Trait("Category", "Unit")]
public class RootErrorPanelTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersErrorPanelDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "some error"));

        var panel = cut.Find(".error-panel");
        Assert.NotNull(panel);
    }

    [Fact]
    public void ErrorPanel_RendersStaticTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test"));

        var title = cut.Find(".error-title");
        Assert.Equal("Dashboard data could not be loaded", title.TextContent);
    }

    [Fact]
    public void ErrorPanel_RendersErrorMessage()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "File not found at /path/data.json"));

        var details = cut.Find(".error-details");
        Assert.Equal("File not found at /path/data.json", details.TextContent);
    }

    [Fact]
    public void ErrorPanel_RendersHelpText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "err"));

        var help = cut.Find(".error-help");
        Assert.Contains("Check data.json for errors and restart the application", help.TextContent);
    }

    [Fact]
    public void ErrorPanel_RendersIconDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "err"));

        var icon = cut.Find(".error-icon");
        Assert.NotNull(icon);
        Assert.Equal("?", icon.TextContent);
    }

    [Fact]
    public void ErrorPanel_NullErrorMessage_RendersWithoutCrash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, (string?)null));

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("error-title", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_EmptyErrorMessage_RendersEmptyDetails()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, ""));

        var details = cut.Find(".error-details");
        Assert.Equal("", details.TextContent.Trim());
    }

    [Fact]
    public void ErrorPanel_HtmlEncodes_ScriptTags()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "<script>alert('xss')</script>"));

        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HtmlEncodes_AngleBrackets()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Error: <div>injected</div>"));

        Assert.DoesNotContain("<div>injected</div>", cut.Markup);
        Assert.Contains("&lt;div&gt;", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_LongMessage_RendersCompletely()
    {
        var longMessage = new string('A', 3000);
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, longMessage));

        Assert.Contains(longMessage, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MultilineMessage_Renders()
    {
        var multiline = "Line 1\nLine 2\nLine 3";
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, multiline));

        var details = cut.Find(".error-details");
        Assert.Contains("Line 1", details.TextContent);
        Assert.Contains("Line 3", details.TextContent);
    }

    [Fact]
    public void ErrorPanel_SpecialCharacters_PreservedInDetails()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Path: C:\\Users\\test & file=\"data.json\""));

        var details = cut.Find(".error-details");
        Assert.Contains("C:\\Users\\test", details.TextContent);
        Assert.Contains("data.json", details.TextContent);
    }
}