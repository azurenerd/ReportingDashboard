using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardErrorPanelIntegrationTests : TestContext
{
    [Fact]
    public void ErrorPanel_WithFileNotFoundMessage_RendersCorrectly()
    {
        const string errorMsg = "data.json not found at /app/wwwroot/data.json";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, errorMsg));

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-title").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Find(".error-details").TextContent.Should().Be(errorMsg);
        cut.Find(".error-help").TextContent.Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void ErrorPanel_WithParseErrorMessage_RendersCorrectly()
    {
        const string errorMsg = "Failed to parse data.json: Unexpected character encountered while parsing";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, errorMsg));

        cut.Find(".error-details").TextContent.Should().Be(errorMsg);
    }

    [Fact]
    public void ErrorPanel_WithValidationErrorMessage_RendersCorrectly()
    {
        const string errorMsg = "data.json validation: 'title' is required";

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, errorMsg));

        cut.Find(".error-details").TextContent.Should().Be(errorMsg);
    }

    [Fact]
    public void ErrorPanel_StructuralIntegrity_AllElementsPresent()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        var inner = cut.Find(".error-panel > div");
        var children = inner.Children;

        children.Length.Should().Be(4);
        children[0].ClassName.Should().Be("error-icon");
        children[1].ClassName.Should().Be("error-title");
        children[2].ClassName.Should().Be("error-details");
        children[3].ClassName.Should().Be("error-help");
    }

    [Fact]
    public void ErrorPanel_NoErrorMessage_ShowsOnlyStaticContent()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, (string)null!));

        var inner = cut.Find(".error-panel > div");
        var children = inner.Children;

        children.Length.Should().Be(3);
        children[0].ClassName.Should().Be("error-icon");
        children[1].ClassName.Should().Be("error-title");
        children[2].ClassName.Should().Be("error-help");
    }

    [Fact]
    public void ErrorPanel_TransitionFromErrorToNull_HidesDetails()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "initial error"));

        cut.FindAll(".error-details").Should().HaveCount(1);

        cut.SetParametersAndRender(p =>
            p.Add(x => x.ErrorMessage, (string)null!));

        cut.FindAll(".error-details").Should().BeEmpty();
    }

    [Fact]
    public void ErrorPanel_TransitionFromNullToError_ShowsDetails()
    {
        var cut = RenderComponent<ErrorPanel>();
        cut.FindAll(".error-details").Should().BeEmpty();

        cut.SetParametersAndRender(p =>
            p.Add(x => x.ErrorMessage, "new error occurred"));

        cut.FindAll(".error-details").Should().HaveCount(1);
        cut.Find(".error-details").TextContent.Should().Be("new error occurred");
    }
}