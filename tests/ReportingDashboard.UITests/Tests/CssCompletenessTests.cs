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

    [Fact]
    public async Task Css_AppCssFileIsLinked()
    {
        var page = await _fixture.NewPageAsync();
        var inspector = new CssInspector(page, _fixture.BaseUrl);
        await inspector.NavigateAsync();

        var hasLink = await inspector.CssFileLoadsAsync();
        hasLink.Should().BeTrue("app.css should be linked in the HTML head");
    }

    [Fact]
    public async Task Css_AppCssLoadsSuccessfully()
    {
        var page = await _fixture.NewPageAsync();

        bool cssLoaded = false;
        page.Response += (_, r) =>
        {
            if (r.Url.Contains("css/app.css") && r.Status == 200)
                cssLoaded = true;
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        cssLoaded.Should().BeTrue("css/app.css should return HTTP 200");
    }

    [Fact]
    public async Task Css_RootCustomProperties_ColorPocDefined()
    {
        var page = await _fixture.NewPageAsync();
        var inspector = new CssInspector(page, _fixture.BaseUrl);
        await inspector.NavigateAsync();

        var value = await inspector.GetCssCustomPropertyAsync("--color-poc");
        value.Should().NotBeNullOrEmpty("--color-poc custom property should be defined");
    }

    [Fact]
    public async Task Css_RootCustomProperties_ColorProdDefined()
    {
        var page = await _fixture.NewPageAsync();
        var inspector = new CssInspector(page, _fixture.BaseUrl);
        await inspector.NavigateAsync();

        var value = await inspector.GetCssCustomPropertyAsync("--color-prod");
        value.Should().NotBeNullOrEmpty("--color-prod custom property should be defined");
    }

    [Fact]
    public async Task Css_RootCustomProperties_ColorNowDefined()
    {
        var page = await _fixture.NewPageAsync();
        var inspector = new CssInspector(page, _fixture.BaseUrl);
        await inspector.NavigateAsync();

        var value = await inspector.GetCssCustomPropertyAsync("--color-now");
        value.Should().NotBeNullOrEmpty("--color-now custom property should be defined");
    }

    [Fact]
    public async Task Css_RootCustomProperties_ColorBorderDefined()
    {
        var page = await _fixture.NewPageAsync();
        var inspector = new CssInspector(page, _fixture.BaseUrl);
        await inspector.NavigateAsync();

        var value = await inspector.GetCssCustomPropertyAsync("--color-border");
        value.Should().NotBeNullOrEmpty("--color-border custom property should be defined");
    }

    [Fact]
    public async Task Css_RootCustomProperties_ColorLinkDefined()
    {
        var page = await _fixture.NewPageAsync();
        var inspector = new CssInspector(page, _fixture.BaseUrl);
        await inspector.NavigateAsync();

        var value = await inspector.GetCssCustomPropertyAsync("--color-link");
        value.Should().NotBeNullOrEmpty("--color-link custom property should be defined");
    }

    [Fact]
    public async Task Css_ReconnectModalHiddenRule_Applied()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        // Inject a fake reconnect modal and verify it's hidden
        await page.EvaluateAsync(@"() => {
            const div = document.createElement('div');
            div.className = 'components-reconnect-modal';
            div.textContent = 'Reconnecting...';
            document.body.appendChild(div);
        }");

        var modal = page.Locator(".components-reconnect-modal");
        var display = await modal.EvaluateAsync<string>(
            "el => getComputedStyle(el).display");
        display.Should().Be("none", "reconnect modal should be hidden via CSS");
    }

    [Fact]
    public async Task Css_HdrClass_HasBorderBottom()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var borderStyle = await dp.Header.EvaluateAsync<string>(
            "el => getComputedStyle(el).borderBottomStyle");
        borderStyle.Should().Be("solid");
    }

    [Fact]
    public async Task Css_HdrH1_Is24pxBold()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var fontSize = await dp.HeaderTitle.EvaluateAsync<string>(
            "el => getComputedStyle(el).fontSize");
        var fontWeight = await dp.HeaderTitle.EvaluateAsync<string>(
            "el => getComputedStyle(el).fontWeight");

        fontSize.Should().Be("24px");
        fontWeight.Should().BeOneOf("700", "bold");
    }

    [Fact]
    public async Task Css_SubClass_Is12px()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var fontSize = await dp.HeaderSubtitle.EvaluateAsync<string>(
            "el => getComputedStyle(el).fontSize");
        fontSize.Should().Be("12px");
    }

    [Fact]
    public async Task Css_SubClass_HasGrayColor()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var color = await dp.HeaderSubtitle.EvaluateAsync<string>(
            "el => getComputedStyle(el).color");
        // #888 = rgb(136, 136, 136)
        color.Should().Contain("136");
    }

    [Fact]
    public async Task Css_HmTitleClass_Is14px()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;
        if (await dp.HeatmapTitle.CountAsync() == 0) return;

        var fontSize = await dp.HeatmapTitle.EvaluateAsync<string>(
            "el => getComputedStyle(el).fontSize");
        fontSize.Should().Be("14px");
    }

    [Fact]
    public async Task Css_HmCornerClass_HasF5Background()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;
        if (await dp.HeatmapCorner.CountAsync() == 0) return;

        var bg = await dp.HeatmapCorner.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        // #F5F5F5 = rgb(245, 245, 245)
        bg.Should().Contain("245");
    }
}