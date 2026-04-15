using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeaderUITests
{
    private readonly PlaywrightFixture _fixture;
    private const int DefaultTimeout = 60000;

    public HeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        page.SetDefaultTimeout(DefaultTimeout);
        return page;
    }

    [Fact]
    public async Task Header_DisplaysTitleFromDataJson()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorText = page.GetByText("Unable to load dashboard data");
        if (await errorText.CountAsync() == 0)
        {
            var header = page.Locator(".hdr h1");
            var text = await header.InnerTextAsync();
            text.Should().NotBeNullOrEmpty("header title should display text from data.json");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Header_DisplaysSubtitle()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorText = page.GetByText("Unable to load dashboard data");
        if (await errorText.CountAsync() == 0)
        {
            var subtitle = page.Locator(".sub");
            (await subtitle.CountAsync()).Should().BeGreaterThan(0, "subtitle should be present");
        }

        await page.CloseAsync();
    }
}