using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class LayoutUITests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Page_LoadsWithCorrectTitle()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await page.TitleAsync();
        title.Should().Be("Executive Reporting Dashboard");
    }

    [Fact]
    public async Task Body_HasFixedDimensions_1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var width = await page.EvaluateAsync<int>("() => document.body.scrollWidth");
        var height = await page.EvaluateAsync<int>("() => document.body.scrollHeight");

        width.Should().Be(1920);
        height.Should().Be(1080);
    }

    [Fact]
    public async Task MainElement_IsRendered_WithNoExtraChrome()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var mainExists = await page.Locator("main").CountAsync();
        mainExists.Should().Be(1);

        var navExists = await page.Locator("nav").CountAsync();
        navExists.Should().Be(0);

        var headerExists = await page.Locator("header").CountAsync();
        headerExists.Should().Be(0);
    }

    [Fact]
    public async Task BlazorErrorUI_IsNotVisible()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var display = await page.EvaluateAsync<string>(
            "() => { const el = document.getElementById('blazor-error-ui'); return el ? window.getComputedStyle(el).display : 'none'; }"
        );
        display.Should().Be("none");
    }

    [Fact]
    public async Task Page_HasNoHorizontalScrollbar()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasHorizontalScroll = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"
        );
        hasHorizontalScroll.Should().BeFalse();
    }
}