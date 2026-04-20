using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Web.Tests.Integration;

[Trait("Category", "Integration")]
public class LayoutPayloadIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LayoutPayloadIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_RendersHeatmapRowHeadersForAllFourCategories()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().Contain("ship-hdr");
        body.Should().Contain("prog-hdr");
        body.Should().Contain("carry-hdr");
        body.Should().Contain("block-hdr");
    }

    [Fact]
    public async Task Root_RendersPageTitleElement()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().Contain("Reporting Dashboard");
    }

    [Fact]
    public async Task Root_DoesNotReferenceLegacyAprClassNames()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().NotContain("apr-hdr");
        body.Should().NotContain("\" apr\"");
    }

    [Fact]
    public async Task TotalPayload_Index_AppCss_ScopedBundle_IsUnder150Kb()
    {
        var client = _factory.CreateClient();

        var indexBytes = (await client.GetByteArrayAsync("/")).Length;
        var appCssBytes = (await client.GetByteArrayAsync("/app.css")).Length;
        var scopedResp = await client.GetAsync("/ReportingDashboard.Web.styles.css");
        var scopedBytes = scopedResp.IsSuccessStatusCode
            ? (await scopedResp.Content.ReadAsByteArrayAsync()).Length
            : 0;

        (indexBytes + appCssBytes + scopedBytes).Should().BeLessThan(150 * 1024);
    }

    [Fact]
    public async Task AppCss_ContainsGlobalResetAndBodyFontFamily()
    {
        var client = _factory.CreateClient();
        var css = await client.GetStringAsync("/app.css");

        css.Should().Contain("box-sizing");
        css.Should().Contain("Segoe UI");
    }
}