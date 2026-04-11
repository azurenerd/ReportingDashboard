using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class ErrorPanelTests : TestContext
{
    [Fact]
    public void Renders_ErrorPanelStructure()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-icon").Should().NotBeNull();
        cut.Find(".error-title").Should().NotBeNull();
        cut.Find(".error-details").Should().NotBeNull();
        cut.Find(".error-help").Should().NotBeNull();
    }

    [Fact]
    public void Renders_FixedTitle()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-title").TextContent.Should().Contain("Dashboard data could not be loaded");
    }

    [Fact]
    public void Renders_HelpText()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-help").TextContent.Should().Contain("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Renders_ErrorIcon()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-icon").TextContent.Should().NotBeEmpty();
    }

    [Fact]
    public void Renders_ErrorMessage_WhenProvided()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, "File not found at /path/data.json"));

        cut.Find(".error-details").TextContent.Should().Contain("File not found at /path/data.json");
    }

    [Fact]
    public void Renders_EmptyDetails_WhenNoErrorMessage()
    {
        var cut = RenderComponent<ErrorPanel>();

        cut.Find(".error-details").TextContent.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Renders_NullErrorMessage_Gracefully()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, (string?)null));

        cut.Find(".error-details").Should().NotBeNull();
    }

    [Fact]
    public void Renders_LongErrorMessage()
    {
        var longMessage = new string('x', 500);

        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, longMessage));

        cut.Find(".error-details").TextContent.Should().Contain(longMessage);
    }

    [Fact]
    public void Renders_SpecialCharactersInErrorMessage()
    {
        var message = "Failed to parse: unexpected token '<' at line 5, col 10";

        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, message));

        cut.Find(".error-details").TextContent.Should().Contain("unexpected token");
    }

    [Fact]
    public void Markup_ContainsAllRequiredSections()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, "Test error"));

        cut.Markup.Should().Contain("error-panel");
        cut.Markup.Should().Contain("error-icon");
        cut.Markup.Should().Contain("error-title");
        cut.Markup.Should().Contain("error-details");
        cut.Markup.Should().Contain("error-help");
    }
}