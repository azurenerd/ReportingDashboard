using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class CssCompletenessTests
{
    private readonly PlaywrightFixture _fixture;

    public CssCompletenessTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<string> FetchCssAsync()
    {
        var page = await _fixture.NewPageAsync();
        var inspector = new CssInspector(page, _fixture.BaseUrl);
        return await inspector.FetchAppCssAsync();
    }

    [Fact]
    public async Task AppCss_IsServed_Returns200()
    {
        var page = await _fixture.NewPageAsync();
        var response = await page.APIRequest.GetAsync($"{_fixture.BaseUrl}/css/app.css");
        response.Status.Should().Be(200);
    }

    [Fact]
    public async Task AppCss_ContainsRootCustomProperties()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(":root");
    }

    [Fact]
    public async Task AppCss_ContainsGlobalReset()
    {
        var css = await FetchCssAsync();
        css.Should().Contain("box-sizing");
    }

    // Header classes
    [Fact]
    public async Task AppCss_ContainsHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hdr");
    }

    [Fact]
    public async Task AppCss_ContainsSubClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".sub");
    }

    // Timeline classes
    [Fact]
    public async Task AppCss_ContainsTlAreaClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".tl-area");
    }

    [Fact]
    public async Task AppCss_ContainsTlSvgBoxClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".tl-svg-box");
    }

    // Heatmap classes
    [Fact]
    public async Task AppCss_ContainsHmWrapClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hm-wrap");
    }

    [Fact]
    public async Task AppCss_ContainsHmTitleClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hm-title");
    }

    [Fact]
    public async Task AppCss_ContainsHmGridClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hm-grid");
    }

    [Fact]
    public async Task AppCss_ContainsHmCornerClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hm-corner");
    }

    [Fact]
    public async Task AppCss_ContainsHmColHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hm-col-hdr");
    }

    [Fact]
    public async Task AppCss_ContainsAprHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".apr-hdr");
    }

    [Fact]
    public async Task AppCss_ContainsHmRowHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hm-row-hdr");
    }

    [Fact]
    public async Task AppCss_ContainsHmCellClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".hm-cell");
    }

    [Fact]
    public async Task AppCss_ContainsItClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".it");
    }

    // Row color variants - Shipped (green)
    [Fact]
    public async Task AppCss_ContainsShipHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".ship-hdr");
    }

    [Fact]
    public async Task AppCss_ContainsShipCellClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".ship-cell");
    }

    // Row color variants - In Progress (blue)
    [Fact]
    public async Task AppCss_ContainsProgHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".prog-hdr");
    }

    [Fact]
    public async Task AppCss_ContainsProgCellClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".prog-cell");
    }

    // Row color variants - Carryover (amber)
    [Fact]
    public async Task AppCss_ContainsCarryHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".carry-hdr");
    }

    [Fact]
    public async Task AppCss_ContainsCarryCellClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".carry-cell");
    }

    // Row color variants - Blockers (red)
    [Fact]
    public async Task AppCss_ContainsBlockHdrClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".block-hdr");
    }

    [Fact]
    public async Task AppCss_ContainsBlockCellClass()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".block-cell");
    }

    // Blazor reconnect modal safety net
    [Fact]
    public async Task AppCss_ContainsReconnectModalHide()
    {
        var css = await FetchCssAsync();
        css.Should().Contain(".components-reconnect-modal");
        css.Should().Contain("display: none !important");
    }

    // Design reference color verification
    [Fact]
    public async Task AppCss_ContainsShipGreenColors()
    {
        var css = await FetchCssAsync();
        // From design: .ship-hdr color:#1B7A28, .ship-cell background:#F0FBF0
        css.Should().Contain("#E8F5E9");
    }

    [Fact]
    public async Task AppCss_ContainsProgBlueColors()
    {
        var css = await FetchCssAsync();
        // From design: .prog-hdr background:#E3F2FD
        css.Should().Contain("#E3F2FD");
    }

    [Fact]
    public async Task AppCss_ContainsCarryAmberColors()
    {
        var css = await FetchCssAsync();
        // From design: .carry-hdr background:#FFF8E1
        css.Should().Contain("#FFF8E1");
    }

    [Fact]
    public async Task AppCss_ContainsBlockRedColors()
    {
        var css = await FetchCssAsync();
        // From design: .block-hdr background:#FEF2F2
        css.Should().Contain("#FEF2F2");
    }

    [Fact]
    public async Task AppCss_ContainsAprHighlightColor()
    {
        var css = await FetchCssAsync();
        // From design: .apr-hdr background:#FFF0D0 color:#C07700
        css.Should().Contain("#FFF0D0");
        css.Should().Contain("#C07700");
    }

    [Fact]
    public async Task AppCss_ContainsDotPseudoElement()
    {
        var css = await FetchCssAsync();
        // .it::before pseudo-element for colored dots
        css.Should().Contain("::before");
        css.Should().Contain("border-radius");
    }
}