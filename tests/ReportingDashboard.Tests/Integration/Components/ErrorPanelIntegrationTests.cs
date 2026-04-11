using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class ErrorPanelIntegrationTests : TestContext
{
    [Fact]
    public void ErrorPanel_RendersAllStructuralElements()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, "data.json not found at /path/to/file"));

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-icon").Should().NotBeNull();
        cut.Find(".error-title").Should().NotBeNull();
        cut.Find(".error-details").Should().NotBeNull();
        cut.Find(".error-help").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_DisplaysFixedTitle()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, "test error"));

        cut.Find(".error-title").TextContent.Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void ErrorPanel_DisplaysFixedHelpText()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, "test error"));

        cut.Find(".error-help").TextContent
            .Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void ErrorPanel_DisplaysErrorMessageInDetails()
    {
        var errorMsg = "Failed to parse data.json: unexpected token at line 5";

        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, errorMsg));

        cut.Find(".error-details").TextContent.Should().Contain(errorMsg);
    }

    [Fact]
    public void ErrorPanel_WithNullMessage_RendersEmptyDetails()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, (string?)null));

        cut.Find(".error-details").TextContent.Trim().Should().BeEmpty();
    }

    [Fact]
    public void ErrorPanel_WithFileNotFoundMessage_DisplaysPath()
    {
        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, "data.json not found at C:\\app\\wwwroot\\data.json"));

        cut.Find(".error-details").TextContent.Should().Contain("C:\\app\\wwwroot\\data.json");
    }

    [Fact]
    public void ErrorPanel_WithValidationMessage_DisplaysFieldErrors()
    {
        var msg = "data.json validation: title is required; subtitle is required; months is required and must be non-empty";

        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, msg));

        var details = cut.Find(".error-details").TextContent;
        details.Should().Contain("title is required");
        details.Should().Contain("subtitle is required");
        details.Should().Contain("months is required");
    }

    [Fact]
    public void ErrorPanel_WithLongErrorMessage_RendersCompletely()
    {
        var longMsg = string.Join("; ", Enumerable.Range(1, 20).Select(i => $"field_{i} is required"));

        var cut = RenderComponent<ErrorPanel>(parameters =>
            parameters.Add(p => p.ErrorMessage, longMsg));

        cut.Find(".error-details").TextContent.Should().Contain("field_1 is required");
        cut.Find(".error-details").TextContent.Should().Contain("field_20 is required");
    }
}