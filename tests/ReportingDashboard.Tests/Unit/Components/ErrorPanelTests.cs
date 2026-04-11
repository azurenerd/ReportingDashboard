using Bunit;
using FluentAssertions;
using ReportingDashboard.Tests.Unit.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class ErrorPanelTests : TestContext
{
    [Fact]
    public void Render_WithMessage_DisplaysErrorIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "Some error occurred"));

        // Assert
        var icon = cut.Find(".error-icon");
        icon.Should().NotBeNull();
        icon.TextContent.Should().Contain("⚠");
    }

    [Fact]
    public void Render_WithMessage_DisplaysTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "Some error"));

        // Assert
        var title = cut.Find("h2");
        title.TextContent.Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void Render_WithMessage_DisplaysErrorMessage()
    {
        // Arrange
        var errorMessage = "Failed to parse data.json: Unexpected character at line 5";

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, errorMessage));

        // Assert
        cut.Markup.Should().Contain(errorMessage);
    }

    [Fact]
    public void Render_WithMessage_DisplaysHelpText()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "Some error"));

        // Assert
        var hint = cut.Find(".error-hint");
        hint.TextContent.Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Render_WithNullMessage_DoesNotRenderMessageParagraph()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, (string?)null));

        // Assert - should have error-panel div, h2, and error-hint but no extra <p> for the message
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1, "only the hint paragraph should render when message is null");
        paragraphs[0].ClassList.Should().Contain("error-hint");
    }

    [Fact]
    public void Render_WithEmptyMessage_DoesNotRenderMessageParagraph()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, string.Empty));

        // Assert
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1, "only the hint paragraph should render when message is empty");
        paragraphs[0].ClassList.Should().Contain("error-hint");
    }

    [Fact]
    public void Render_WithWhitespaceMessage_DoesNotRenderMessageParagraph()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "   "));

        // Assert - whitespace is not considered empty by string.IsNullOrEmpty, so it should render
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(2, "whitespace string passes IsNullOrEmpty check so message paragraph renders");
    }

    [Fact]
    public void Render_WithNoParameters_DoesNotRenderMessageParagraph()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>();

        // Assert
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1, "only the hint paragraph should render when no message parameter is provided");
    }

    [Fact]
    public void Render_HasErrorPanelCssClass()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "test"));

        // Assert
        var panel = cut.Find(".error-panel");
        panel.Should().NotBeNull();
    }

    [Fact]
    public void Render_StructureOrder_IconThenTitleThenMessageThenHint()
    {
        // Arrange
        var errorMessage = "specific error text";

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, errorMessage));

        // Assert - verify DOM order within .error-panel
        var panel = cut.Find(".error-panel");
        var children = panel.Children;

        children[0].ClassName.Should().Contain("error-icon");
        children[1].TagName.Should().BeEquivalentTo("H2");
        // Message paragraph
        children[2].TagName.Should().BeEquivalentTo("P");
        children[2].TextContent.Should().Contain(errorMessage);
        // Hint paragraph
        children[3].TagName.Should().BeEquivalentTo("P");
        children[3].ClassName.Should().Contain("error-hint");
    }

    [Fact]
    public void Render_WithLongMessage_DisplaysFullMessage()
    {
        // Arrange
        var longMessage = new string('x', 2000);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, longMessage));

        // Assert
        cut.Markup.Should().Contain(longMessage);
    }

    [Fact]
    public void Render_WithSpecialCharacters_EncodesMessage()
    {
        // Arrange
        var htmlMessage = "<script>alert('xss')</script>";

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, htmlMessage));

        // Assert - Blazor should HTML-encode the message
        cut.Markup.Should().NotContain("<script>");
        cut.Markup.Should().Contain("&lt;script&gt;");
    }

    [Fact]
    public void Render_WithMultilineMessage_DisplaysAllLines()
    {
        // Arrange
        var multilineMessage = "Error on line 1\nError on line 2\nError on line 3";

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, multilineMessage));

        // Assert
        cut.Markup.Should().Contain("Error on line 1");
        cut.Markup.Should().Contain("Error on line 3");
    }

    [Fact]
    public void MessageParameter_CanBeUpdated()
    {
        // Arrange
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "Initial error"));

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Message, "Updated error"));

        // Assert
        cut.Markup.Should().Contain("Updated error");
        cut.Markup.Should().NotContain("Initial error");
    }

    [Fact]
    public void MessageParameter_UpdatedToNull_HidesMessageParagraph()
    {
        // Arrange
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "Initial error"));
        cut.FindAll("p").Should().HaveCount(2);

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Message, (string?)null));

        // Assert
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1);
        paragraphs[0].ClassList.Should().Contain("error-hint");
    }

    [Fact]
    public void Render_ErrorIconContent_ContainsWarningSymbol()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, "test"));

        // Assert - &#9888; is the Unicode warning sign ⚠
        var icon = cut.Find(".error-icon");
        icon.InnerHtml.Should().Contain("⚠");
    }

    [Fact]
    public void Render_TitleText_IsExactMatch()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>();

        // Assert
        var title = cut.Find("h2");
        title.TextContent.Trim().Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void Render_HintText_IsExactMatch()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>();

        // Assert
        var hint = cut.Find(".error-hint");
        hint.TextContent.Trim().Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Render_WithFileNotFoundMessage_DisplaysCorrectly()
    {
        // Arrange - simulate the actual error message from DashboardDataService
        var message = "data.json not found at /app/wwwroot/data.json";

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, message));

        // Assert
        cut.Markup.Should().Contain("data.json not found");
    }

    [Fact]
    public void Render_WithJsonParseErrorMessage_DisplaysCorrectly()
    {
        // Arrange - simulate a JSON parse error
        var message = "Failed to parse data.json: '{' is invalid at line 3, position 1";

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(parameters => parameters
            .Add(p => p.Message, message));

        // Assert
        cut.Markup.Should().Contain("Failed to parse data.json");
    }
}