using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class IndexPageTests
{
    private readonly PlaywrightFixture _fx;

    public IndexPageTests(PlaywrightFixture fx) => _fx = fx;

    [Fact]
    public async Task Index_ReturnsHttp200()
    {
        var page = await _fx.NewPageAsync();
        var response = await page.GotoAsync(_fx.BaseUrl + "/");
        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task Index_Renders_WithoutScrollbarsAt1920x1080()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var overflow = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.body).overflow");
        overflow.Should().Contain("hidden");
    }

    [Fact]
    public async Task Index_Body_HasFixedDimensions()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var size = await page.EvaluateAsync<int[]>(
            "() => [document.body.clientWidth, document.body.clientHeight]");
        size[0].Should().Be(1920);
        size[1].Should().Be(1080);
    }

    [Fact]
    public async Task SiteCss_IsServedAsTextCss()
    {
        var page = await _fx.NewPageAsync();
        var response = await page.GotoAsync(_fx.BaseUrl + "/css/site.css");
        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        var contentType = response.Headers.TryGetValue("content-type", out var ct) ? ct : "";
        contentType.Should().Contain("text/css");
    }

    [Fact]
    public async Task Index_ReconnectModal_IsHidden()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var modal = page.Locator("#components-reconnect-modal");
        if (await modal.CountAsync() > 0)
        {
            var display = await modal.First.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            display.Should().Be("none");
        }
    }
}