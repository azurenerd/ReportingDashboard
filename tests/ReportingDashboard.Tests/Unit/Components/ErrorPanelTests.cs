using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class ErrorPanelTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersErrorIcon()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-icon").Should().NotBeNull();
        cut.Find(".error-icon").TextContent.Should().Contain("⚠");
    }

    [Fact]
    public void ErrorPanel_RendersDefaultHeading()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void ErrorPanel_WithMessage_RendersMessageText()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "File not found: data.json"));

        cut.Markup.Should().Contain("File not found: data.json");
    }

    [Fact]
    public void ErrorPanel_WithNullMessage_DoesNotRenderMessageParagraph()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, (string?)null));

        var paragraphs = cut.FindAll("p");
        // Should only have the hint paragraph, not a message paragraph
        paragraphs.Should().HaveCount(1);
        paragraphs[0].ClassList.Should().Contain("error-hint");
    }

    [Fact]
    public void ErrorPanel_WithEmptyMessage_DoesNotRenderMessageParagraph()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, ""));

        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1);
        paragraphs[0].ClassList.Should().Contain("error-hint");
    }

    [Fact]
    public void ErrorPanel_RendersHintText()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-hint").TextContent.Should()
            .Contain("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void ErrorPanel_HasErrorPanelCssClass()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-panel").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_WithLongMessage_RendersFullMessage()
    {
        var longMessage = new string('x', 500);

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, longMessage));

        cut.Markup.Should().Contain(longMessage);
    }

    [Fact]
    public void ErrorPanel_WithSpecialCharacters_RendersEncodedMessage()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "Error: <script>alert('xss')</script>"));

        cut.Markup.Should().NotContain("<script>");
        cut.Markup.Should().Contain("&lt;script&gt;");
    }
}