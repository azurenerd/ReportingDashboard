using Bunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Components;

[Trait("Category", "Unit")]
public class ErrorBannerTests : IDisposable
{
    private readonly Bunit.TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void RendersNothing_WhenErrorIsNull()
    {
        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.ErrorBanner>(p => p
            .Add(x => x.Error, null));

        cut.FindAll("div.error-banner").Should().BeEmpty();
        cut.Markup.Trim().Should().BeEmpty();
    }

    [Theory]
    [InlineData(DashboardLoadErrorKind.NotFound, "data.json not found", "not found")]
    [InlineData(DashboardLoadErrorKind.ParseError, "Failed to parse data.json", "parse error")]
    [InlineData(DashboardLoadErrorKind.ValidationError, "data.json failed validation", "validation error")]
    public void RendersHeadingAndKindLabel_ForEachKind(string kind, string expectedHeading, string expectedKindLabel)
    {
        var err = new DashboardLoadError(
            FilePath: "C:/x/data.json",
            Message: "some message",
            Line: null,
            Column: null,
            Kind: kind);

        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.ErrorBanner>(p => p
            .Add(x => x.Error, err));

        var root = cut.Find("div.error-banner");
        root.GetAttribute("role").Should().Be("alert");
        cut.Find("div.error-banner strong").TextContent.Should().Be(expectedHeading);
        cut.Find(".error-kind").TextContent.Should().Be(expectedKindLabel);
        cut.Find(".error-path").TextContent.Should().Be("C:/x/data.json");
        cut.Find(".error-message").TextContent.Should().Be("some message");
        cut.FindAll(".error-location").Should().BeEmpty();
    }

    [Fact]
    public void RendersLocation_OnlyWhenLineIsProvided()
    {
        var err = new DashboardLoadError("data.json", "parse failed", Line: 12, Column: 5, Kind: DashboardLoadErrorKind.ParseError);

        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.ErrorBanner>(p => p
            .Add(x => x.Error, err));

        var loc = cut.Find(".error-location").TextContent;
        loc.Should().Contain("line 12");
        loc.Should().Contain("col 5");
    }

    [Fact]
    public void HtmlEncodes_HostileMessageAndPath()
    {
        var err = new DashboardLoadError(
            FilePath: "<path>",
            Message: "<script>alert(1)</script>",
            Line: null,
            Column: null,
            Kind: DashboardLoadErrorKind.ValidationError);

        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.ErrorBanner>(p => p
            .Add(x => x.Error, err));

        cut.Markup.Should().NotContain("<script>alert(1)</script>");
        cut.Markup.Should().Contain("&lt;script&gt;");
        cut.Markup.Should().Contain("&lt;path&gt;");
    }
}