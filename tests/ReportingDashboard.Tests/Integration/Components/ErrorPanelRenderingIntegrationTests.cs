using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class ErrorPanelRenderingIntegrationTests : TestContext
{
    [Fact]
    public void ErrorPanel_WithMessage_RendersAllSections()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "data.json not found"));

        cut.FindAll(".error-panel").Should().HaveCount(1);
        cut.FindAll(".error-icon").Should().HaveCount(1);
        cut.FindAll(".error-title").Should().HaveCount(1);
        cut.FindAll(".error-details").Should().HaveCount(1);
        cut.FindAll(".error-help").Should().HaveCount(1);
    }

    [Fact]
    public void ErrorPanel_TitleText_MatchesSpecification()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "some error"));

        cut.Find(".error-title").TextContent
            .Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void ErrorPanel_HelpText_MatchesSpecification()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "some error"));

        cut.Find(".error-help").TextContent
            .Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void ErrorPanel_DetailsText_MatchesParameter()
    {
        const string msg = "Failed to parse data.json: unexpected token";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, msg));

        cut.Find(".error-details").TextContent.Should().Be(msg);
    }

    [Fact]
    public void ErrorPanel_NullMessage_HidesDetails()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, (string)null!));

        cut.FindAll(".error-details").Should().BeEmpty();
        cut.Find(".error-title").Should().NotBeNull();
        cut.Find(".error-help").Should().NotBeNull();
    }

    [Fact]
    public void ErrorPanel_EmptyMessage_HidesDetails()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, ""));

        cut.FindAll(".error-details").Should().BeEmpty();
    }

    [Fact]
    public void ErrorPanel_IconIsRendered()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "error"));

        cut.Find(".error-icon").TextContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ErrorPanel_CenteredLayout_HasInlineStyle()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "error"));

        var inner = cut.Find(".error-panel > div");
        inner.GetAttribute("style").Should().Contain("text-align: center");
    }

    [Fact]
    public void ErrorPanel_SpecialCharacters_AreEncoded()
    {
        const string msg = "<script>alert('xss')</script>";
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, msg));

        cut.Find(".error-details").TextContent.Should().Be(msg);
        cut.Markup.Should().NotContain("<script>");
    }
}