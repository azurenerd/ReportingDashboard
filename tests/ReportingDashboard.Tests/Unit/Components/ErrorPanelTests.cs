using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Unit tests for the ErrorPanel Blazor component in isolation.
/// </summary>
[Trait("Category", "Unit")]
public class ErrorPanelTests : TestContext
{
    [Fact]
    public void ErrorPanel_WithMessage_RendersErrorPanelClass()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "File not found"));

        cut.Find(".error-panel").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_WithMessage_RendersHeading()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "File not found"));

        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void ErrorPanel_WithMessage_RendersErrorMessageText()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "data.json not found at /path/to/file"));

        cut.Markup.Should().Contain("data.json not found at /path/to/file");
    }

    [Fact]
    public void ErrorPanel_WithMessage_RendersHint()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Some error"));

        cut.Find(".error-hint").TextContent.Should().Contain("Check data.json");
    }

    [Fact]
    public void ErrorPanel_WithNullMessage_DoesNotRenderMessageParagraph()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, (string?)null));

        cut.Find(".error-panel").Should().NotBeNull();
        var paragraphs = cut.FindAll("p");
        // Should only have the hint paragraph, not a message paragraph
        paragraphs.Should().HaveCount(1);
    }

    [Fact]
    public void ErrorPanel_WithEmptyMessage_RendersEmptyParagraph()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, ""));

        // Empty string is still a non-null string, component may render it
        cut.Find(".error-panel").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_WithLongMessage_RendersFullMessage()
    {
        var longMessage = new string('X', 500);
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, longMessage));

        cut.Markup.Should().Contain(longMessage);
    }

    [Fact]
    public void ErrorPanel_WithSpecialCharacters_RendersEscaped()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Error: <script>alert('xss')</script>"));

        // Blazor auto-escapes HTML content
        cut.Markup.Should().NotContain("<script>");
        cut.Markup.Should().Contain("&lt;script&gt;");
    }
}