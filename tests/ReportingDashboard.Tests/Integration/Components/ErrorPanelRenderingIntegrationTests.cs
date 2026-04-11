using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class ErrorPanelRenderingIntegrationTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersCompleteStructure()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Test error message"));

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-icon").Should().NotBeNull();
        cut.Find("h2").Should().NotBeNull();
        cut.Find(".error-hint").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_IconContainsWarningSymbol()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-icon").TextContent.Should().Contain("⚠");
    }

    [Fact]
    public void ErrorPanel_HeadingTextIsCorrect()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void ErrorPanel_HintTextIsCorrect()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-hint").TextContent.Should()
            .Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void ErrorPanel_WithMessage_ContainsMessageText()
    {
        var message = "Failed to parse data.json: unexpected token at position 42";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, message));

        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(2); // message + hint
        paragraphs[0].TextContent.Should().Be(message);
    }

    [Fact]
    public void ErrorPanel_WithNullMessage_OnlyRendersHintParagraph()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, (string?)null));

        cut.FindAll("p").Should().HaveCount(1);
        cut.Find(".error-hint").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_WithEmptyMessage_OnlyRendersHintParagraph()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, ""));

        cut.FindAll("p").Should().HaveCount(1);
        cut.Find(".error-hint").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_FileNotFoundMessage_RendersCorrectly()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Dashboard data file not found: /app/wwwroot/data.json. Please create wwwroot/data.json."));

        cut.Markup.Should().Contain("not found");
        cut.Markup.Should().Contain("wwwroot/data.json");
    }

    [Fact]
    public void ErrorPanel_WithHtmlInMessage_EscapesHtml()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "<script>alert('xss')</script>"));

        cut.Markup.Should().NotContain("<script>");
        cut.Markup.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void ErrorPanel_WithLongMessage_RendersFullText()
    {
        var longMessage = string.Join(" ", Enumerable.Repeat("error detail", 50));

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, longMessage));

        cut.Markup.Should().Contain(longMessage);
    }
}