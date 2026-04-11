using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class ErrorPanelTests : TestContext
{
    [Fact]
    public void Render_WithErrorMessage_DisplaysAllElements()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "File not found"));

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-icon").TextContent.Should().Contain("?");
        cut.Find(".error-title").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Find(".error-details").TextContent.Should().Be("File not found");
        cut.Find(".error-help").TextContent.Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Render_WithNullErrorMessage_HidesDetailsElement()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, (string)null!));

        cut.FindAll(".error-details").Should().BeEmpty();
        cut.Find(".error-icon").Should().NotBeNull();
        cut.Find(".error-title").Should().NotBeNull();
        cut.Find(".error-help").Should().NotBeNull();
    }

    [Fact]
    public void Render_WithEmptyErrorMessage_HidesDetailsElement()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, ""));

        cut.FindAll(".error-details").Should().BeEmpty();
    }

    [Fact]
    public void Render_WithWhitespaceOnlyErrorMessage_ShowsDetailsElement()
    {
        // string.IsNullOrEmpty returns false for whitespace-only strings
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "   "));

        cut.FindAll(".error-details").Should().HaveCount(1);
        cut.Find(".error-details").TextContent.Should().Be("   ");
    }

    [Fact]
    public void Render_WithNoParameterSet_HidesDetailsElement()
    {
        // Default string parameter is null
        var cut = RenderComponent<ErrorPanel>();

        cut.FindAll(".error-details").Should().BeEmpty();
        cut.Find(".error-title").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Find(".error-help").TextContent.Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Render_DisplaysExactErrorMessageText()
    {
        const string message = "data.json not found at /app/wwwroot/data.json";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        cut.Find(".error-details").TextContent.Should().Be(message);
    }

    [Fact]
    public void Render_WithLongErrorMessage_DisplaysFullText()
    {
        var longMessage = new string('x', 500) + " end-marker";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, longMessage));

        cut.Find(".error-details").TextContent.Should().Contain("end-marker");
        cut.Find(".error-details").TextContent.Length.Should().Be(longMessage.Length);
    }

    [Fact]
    public void Render_WithSpecialCharactersInMessage_EncodesCorrectly()
    {
        const string message = "Error: <invalid> JSON & missing \"field\"";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        cut.Find(".error-details").TextContent.Should().Be(message);
    }

    [Fact]
    public void Render_OuterContainer_HasErrorPanelClass()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test"));

        var outer = cut.Find(".error-panel");
        outer.Should().NotBeNull();
    }

    [Fact]
    public void Render_InnerContent_HasCenteredTextAlignment()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test"));

        var inner = cut.Find(".error-panel > div");
        inner.GetAttribute("style").Should().Contain("text-align: center");
    }

    [Fact]
    public void Render_ErrorIcon_ContainsQuestionMark()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test"));

        var icon = cut.Find(".error-icon");
        icon.TextContent.Should().Contain("?");
    }

    [Fact]
    public void Render_TitleIsStatic_DoesNotChangeWithMessage()
    {
        var cut1 = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "error A"));
        var cut2 = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "error B"));

        cut1.Find(".error-title").TextContent
            .Should().Be(cut2.Find(".error-title").TextContent);
    }

    [Fact]
    public void Render_HelpTextIsStatic_AlwaysPresent()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-help").TextContent
            .Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Render_ElementOrder_IconThenTitleThenDetailsThenHelp()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "some error"));

        var inner = cut.Find(".error-panel > div");
        var children = inner.Children;

        children.Length.Should().Be(4);
        children[0].ClassName.Should().Be("error-icon");
        children[1].ClassName.Should().Be("error-title");
        children[2].ClassName.Should().Be("error-details");
        children[3].ClassName.Should().Be("error-help");
    }

    [Fact]
    public void Render_WithoutDetails_ElementOrder_IconThenTitleThenHelp()
    {
        var cut = RenderComponent<ErrorPanel>();

        var inner = cut.Find(".error-panel > div");
        var children = inner.Children;

        children.Length.Should().Be(3);
        children[0].ClassName.Should().Be("error-icon");
        children[1].ClassName.Should().Be("error-title");
        children[2].ClassName.Should().Be("error-help");
    }

    [Fact]
    public void Render_ParameterUpdate_ShowsNewMessage()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "first error"));

        cut.Find(".error-details").TextContent.Should().Be("first error");

        cut.SetParametersAndRender(p =>
            p.Add(x => x.ErrorMessage, "second error"));

        cut.Find(".error-details").TextContent.Should().Be("second error");
    }

    [Fact]
    public void Render_ParameterUpdate_FromMessageToNull_HidesDetails()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "some error"));

        cut.FindAll(".error-details").Should().HaveCount(1);

        cut.SetParametersAndRender(p =>
            p.Add(x => x.ErrorMessage, (string)null!));

        cut.FindAll(".error-details").Should().BeEmpty();
    }

    [Fact]
    public void Render_ParameterUpdate_FromNullToMessage_ShowsDetails()
    {
        var cut = RenderComponent<ErrorPanel>();
        cut.FindAll(".error-details").Should().BeEmpty();

        cut.SetParametersAndRender(p =>
            p.Add(x => x.ErrorMessage, "new error"));

        cut.FindAll(".error-details").Should().HaveCount(1);
        cut.Find(".error-details").TextContent.Should().Be("new error");
    }

    [Fact]
    public void Render_WithMultilineErrorMessage_PreservesContent()
    {
        const string multiline = "Line 1\nLine 2\nLine 3";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, multiline));

        cut.Find(".error-details").TextContent.Should().Contain("Line 1");
        cut.Find(".error-details").TextContent.Should().Contain("Line 3");
    }
}