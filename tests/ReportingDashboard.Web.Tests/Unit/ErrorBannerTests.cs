using Bunit;
using FluentAssertions;
using ReportingDashboard.Web.Components.Partials;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class ErrorBannerTests : TestContext
{
    [Fact]
    public void NotFound_RendersExpectedText_WithFilePath()
    {
        var err = new DashboardLoadError(@"C:\app\wwwroot\data.json", "missing", null, null, "NotFound");

        var cut = RenderComponent<ErrorBanner>(p => p.Add(x => x.Error, err));

        var banner = cut.Find(".error-banner");
        banner.GetAttribute("role").Should().Be("alert");
        cut.Find(".error-text").TextContent
            .Should().Be(@"data.json not found at C:\app\wwwroot\data.json. See README for schema.");
    }

    [Fact]
    public void ValidationError_RendersValidationText()
    {
        var err = new DashboardLoadError("data.json", "timeline.end must be after timeline.start", null, null, "ValidationError");

        var cut = RenderComponent<ErrorBanner>(p => p.Add(x => x.Error, err));

        cut.Find(".error-text").TextContent
            .Should().Be("data.json validation failed: timeline.end must be after timeline.start");
    }

    [Fact]
    public void ParseError_WithLineAndColumn_AppendsLocation()
    {
        var err = new DashboardLoadError("data.json", "Unexpected end of JSON input", 42, 3, "ParseError");

        var cut = RenderComponent<ErrorBanner>(p => p.Add(x => x.Error, err));

        cut.Find(".error-text").TextContent
            .Should().Be("Failed to load data.json: Unexpected end of JSON input at line 42, column 3.");
    }

    [Fact]
    public void ParseError_WithoutLineOrColumn_OmitsLocation()
    {
        var err = new DashboardLoadError("data.json", "bad json", null, null, "ParseError");

        var cut = RenderComponent<ErrorBanner>(p => p.Add(x => x.Error, err));

        cut.Find(".error-text").TextContent.Should().Be("Failed to load data.json: bad json");
    }

    [Fact]
    public void ParseError_WithOnlyLine_DoesNotAppendLocation()
    {
        // Source requires BOTH line and column to append; with only line, no suffix.
        var err = new DashboardLoadError("data.json", "oops", 10, null, "ParseError");

        var cut = RenderComponent<ErrorBanner>(p => p.Add(x => x.Error, err));

        cut.Find(".error-text").TextContent.Should().Be("Failed to load data.json: oops");
    }
}