using Bunit;
using Xunit;
using ReportingDashboard.Components;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Unit")]
public class ErrorPanelRootComponentIntegrationTests : TestContext
{
    [Fact]
    public void ErrorPanel_FileNotFound_RendersMessage()
    {
        var message = "data.json not found at C:\\app\\wwwroot\\data.json";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        Assert.Contains(message, cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_ParseError_RendersMessage()
    {
        var message = "Failed to parse data.json: Unexpected character at line 5, position 12";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        Assert.Contains(message, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_ValidationError_RendersMessage()
    {
        var message = "data.json validation failed: 'title' is required";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        Assert.Contains(message, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_DoesNotExposeStackTrace()
    {
        var message = "data.json not found at /path/data.json";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        Assert.DoesNotContain("at System.", cut.Markup);
        Assert.DoesNotContain("StackTrace", cut.Markup);
        Assert.DoesNotContain("NullReferenceException", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_RendersAllRequiredSections()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        // Indicator
        Assert.Contains("?", cut.Markup);
        // Title
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
        // Error message
        Assert.Contains("test error", cut.Markup);
        // Help text
        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
        // Console hint
        Assert.Contains("See console output for details", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_LongErrorMessage_RendersWithoutCrash()
    {
        var longMessage = new string('x', 2000);
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, longMessage));

        Assert.Contains(longMessage, cut.Markup);
    }

    [Fact]
    public void ErrorPanel_SpecialCharactersInMessage_AreEncoded()
    {
        var message = "<script>alert('xss')</script>";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        // Blazor should HTML-encode the message
        Assert.DoesNotContain("<script>", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MultilineErrorMessage_Renders()
    {
        var message = "Line 1\nLine 2\nLine 3";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        Assert.Contains("Line 1", cut.Markup);
    }
}