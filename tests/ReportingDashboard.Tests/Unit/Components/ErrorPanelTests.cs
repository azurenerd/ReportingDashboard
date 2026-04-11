using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class ErrorPanelTests : TestContext
{
    [Fact]
    public void ErrorPanel_WithMessage_ShouldDisplayMessage()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, "File not found"));

        cut.Markup.Should().Contain("File not found");
    }

    [Fact]
    public void ErrorPanel_WithMessage_ShouldDisplayHeading()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, "Some error"));

        cut.Find("h2").TextContent.Should().Contain("Dashboard data could not be loaded");
    }

    [Fact]
    public void ErrorPanel_WithMessage_ShouldDisplayHint()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, "Error"));

        cut.Find(".error-hint").TextContent.Should().Contain("Check data.json");
    }

    [Fact]
    public void ErrorPanel_WithMessage_ShouldDisplayWarningIcon()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, "Error"));

        cut.Find(".error-icon").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_WithNullMessage_ShouldNotRenderMessageParagraph()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, (string?)null));

        // Should have h2 and hint paragraph but no extra message paragraph
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1); // only the hint
    }

    [Fact]
    public void ErrorPanel_WithEmptyMessage_ShouldNotRenderMessageParagraph()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, ""));

        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1); // only the hint
    }

    [Fact]
    public void ErrorPanel_WithLongMessage_ShouldDisplayFullMessage()
    {
        var longMessage = new string('x', 500);
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, longMessage));

        cut.Markup.Should().Contain(longMessage);
    }

    [Fact]
    public void ErrorPanel_ShouldHaveErrorPanelClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, "test"));

        cut.Find(".error-panel").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_WithSpecialCharacters_ShouldEncodeAndDisplay()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters =>
            parameters.Add(p => p.Message, "Error: <script>alert('xss')</script>"));

        // bUnit HTML-encodes by default
        cut.Markup.Should().NotContain("<script>");
        cut.Markup.Should().Contain("alert");
    }
}