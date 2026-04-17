using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class CssVisualQATests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public CssVisualQATests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page.SetDefaultTimeout(60000);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    public async Task Body_FlexColumnLayout_MatchesDesignConcept()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var styles = await _page.EvaluateAsync<Dictionary<string, string>>(@"() => {
            const s = getComputedStyle(document.body);
            return {
                display: s.display,
                flexDirection: s.flexDirection,
                width: s.width,
                height: s.height,
                overflow: s.overflow
            };
        }");

        styles["display"].Should().Be("flex");
        styles["flexDirection"].Should().Be("column");
        styles["width"].Should().Be("1920px");
        styles["height"].Should().Be("1080px");
        styles["overflow"].Should().Be("hidden");
    }

    [Fact]
    public async Task CssCustomProperties_ColorPalette_DefinesAllCategories()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var vars = await _page.EvaluateAsync<Dictionary<string, string>>(@"() => {
            const root = getComputedStyle(document.documentElement);
            const get = (v) => root.getPropertyValue(v).trim();
            return {
                bgWhite: get('--bg-white'),
                bgSubtle: get('--bg-subtle'),
                textPrimary: get('--text-primary'),
                linkColor: get('--link-color'),
                nowColor: get('--now-color'),
                pocColor: get('--poc-color'),
                prodColor: get('--prod-color'),
                shipHdrText: get('--ship-hdr-text'),
                progHdrText: get('--prog-hdr-text'),
                carryHdrText: get('--carry-hdr-text'),
                blockHdrText: get('--block-hdr-text'),
                hlColBg: get('--hl-col-bg'),
                hlColText: get('--hl-col-text'),
                shipDot: get('--ship-dot'),
                progDot: get('--prog-dot'),
                carryDot: get('--carry-dot'),
                blockDot: get('--block-dot')
            };
        }");

        vars["bgWhite"].Should().NotBeEmpty("--bg-white must be defined");
        vars["bgSubtle"].Should().NotBeEmpty("--bg-subtle must be defined");
        vars["textPrimary"].Should().NotBeEmpty("--text-primary must be defined");
        vars["linkColor"].Should().NotBeEmpty("--link-color must be defined");
        vars["nowColor"].Should().NotBeEmpty("--now-color must be defined");
        vars["pocColor"].Should().NotBeEmpty("--poc-color must be defined");
        vars["prodColor"].Should().NotBeEmpty("--prod-color must be defined");
        vars["shipHdrText"].Should().NotBeEmpty("--ship-hdr-text must be defined");
        vars["progHdrText"].Should().NotBeEmpty("--prog-hdr-text must be defined");
        vars["carryHdrText"].Should().NotBeEmpty("--carry-hdr-text must be defined");
        vars["blockHdrText"].Should().NotBeEmpty("--block-hdr-text must be defined");
        vars["hlColBg"].Should().NotBeEmpty("--hl-col-bg must be defined");
        vars["hlColText"].Should().NotBeEmpty("--hl-col-text must be defined");
        vars["shipDot"].Should().NotBeEmpty("--ship-dot must be defined");
        vars["progDot"].Should().NotBeEmpty("--prog-dot must be defined");
        vars["carryDot"].Should().NotBeEmpty("--carry-dot must be defined");
        vars["blockDot"].Should().NotBeEmpty("--block-dot must be defined");
    }

    [Fact]
    public async Task ThreeSectionLayout_HeaderTimelineHeatmap_HasCorrectStructuralStyles()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the CSS rules exist in the stylesheet even if elements aren't rendered yet.
        // We inject elements with the expected classes and check computed styles.
        var results = await _page.EvaluateAsync<Dictionary<string, string>>(@"() => {
            // Create temporary elements to probe CSS rules
            const hdr = document.createElement('div');
            hdr.className = 'hdr';
            document.body.appendChild(hdr);

            const tlArea = document.createElement('div');
            tlArea.className = 'tl-area';
            document.body.appendChild(tlArea);

            const hmWrap = document.createElement('div');
            hmWrap.className = 'hm-wrap';
            document.body.appendChild(hmWrap);

            const hdrS = getComputedStyle(hdr);
            const tlS = getComputedStyle(tlArea);
            const hmS = getComputedStyle(hmWrap);

            const result = {
                hdrDisplay: hdrS.display,
                hdrJustify: hdrS.justifyContent,
                hdrAlignItems: hdrS.alignItems,
                hdrFlexShrink: hdrS.flexShrink,
                tlAreaHeight: tlS.height,
                tlAreaFlexShrink: tlS.flexShrink,
                tlAreaDisplay: tlS.display,
                hmWrapFlex: hmS.flexGrow,
                hmWrapDisplay: hmS.display,
                hmWrapFlexDir: hmS.flexDirection
            };

            document.body.removeChild(hdr);
            document.body.removeChild(tlArea);
            document.body.removeChild(hmWrap);

            return result;
        }");

        results["hdrDisplay"].Should().Be("flex");
        results["hdrJustify"].Should().Be("space-between");
        results["hdrAlignItems"].Should().Be("center");
        results["hdrFlexShrink"].Should().Be("0");
        results["tlAreaHeight"].Should().Be("196px");
        results["tlAreaFlexShrink"].Should().Be("0");
        results["tlAreaDisplay"].Should().Be("flex");
        results["hmWrapFlex"].Should().Be("1");
        results["hmWrapDisplay"].Should().Be("flex");
        results["hmWrapFlexDir"].Should().Be("column");
    }

    [Fact]
    public async Task HeatmapCellClasses_ApplyCorrectBackgroundColors()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var colors = await _page.EvaluateAsync<Dictionary<string, string>>(@"() => {
            const probeColor = (cls) => {
                const el = document.createElement('div');
                el.className = cls;
                document.body.appendChild(el);
                const bg = getComputedStyle(el).backgroundColor;
                document.body.removeChild(el);
                return bg;
            };
            return {
                shipCell: probeColor('hm-cell ship-cell'),
                shipCellApr: probeColor('hm-cell ship-cell apr'),
                progCell: probeColor('hm-cell prog-cell'),
                progCellApr: probeColor('hm-cell prog-cell apr'),
                carryCell: probeColor('hm-cell carry-cell'),
                carryCellApr: probeColor('hm-cell carry-cell apr'),
                blockCell: probeColor('hm-cell block-cell'),
                blockCellApr: probeColor('hm-cell block-cell apr')
            };
        }");

        // Verify base and highlighted colors differ for each category
        colors["shipCell"].Should().NotBe(colors["shipCellApr"], "apr highlight should differ from base ship-cell");
        colors["progCell"].Should().NotBe(colors["progCellApr"], "apr highlight should differ from base prog-cell");
        colors["carryCell"].Should().NotBe(colors["carryCellApr"], "apr highlight should differ from base carry-cell");
        colors["blockCell"].Should().NotBe(colors["blockCellApr"], "apr highlight should differ from base block-cell");

        // Verify each category has a distinct base color
        var baseBgs = new[] { colors["shipCell"], colors["progCell"], colors["carryCell"], colors["blockCell"] };
        baseBgs.Distinct().Should().HaveCount(4, "all four category cell backgrounds should be distinct");
    }

    [Fact]
    public async Task ErrorContainer_CentersContent_WithFlexbox()
    {
        // Navigate to trigger error state with nonexistent file
        await _page.GotoAsync($"{_fixture.BaseUrl}/?data=nonexistent.json");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = _page.Locator(".error-container");
        await errorContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var styles = await _page.EvaluateAsync<Dictionary<string, string>>(@"() => {
            const container = document.querySelector('.error-container');
            const msg = document.querySelector('.error-message');
            const result = {};
            if (container) {
                const cs = getComputedStyle(container);
                result.containerDisplay = cs.display;
                result.containerJustify = cs.justifyContent;
                result.containerAlignItems = cs.alignItems;
            }
            if (msg) {
                const ms = getComputedStyle(msg);
                result.msgFontSize = ms.fontSize;
                result.msgMaxWidth = ms.maxWidth;
            }
            return result;
        }");

        styles["containerDisplay"].Should().Be("flex");
        styles["containerJustify"].Should().Be("center");
        styles["containerAlignItems"].Should().Be("center");
        styles["msgFontSize"].Should().Be("16px");
        styles["msgMaxWidth"].Should().Be("600px");
    }
}