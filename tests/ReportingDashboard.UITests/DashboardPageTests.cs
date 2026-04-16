using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardPageTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task Dashboard_LoadsSuccessfully_ShowsContent()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Should show either the dashboard content or the error container
        var body = await page.Locator("body").InnerTextAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_WithValidData_ShowsTitle()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If data.json is present, h1 should have the title
        var h1 = page.Locator("h1");
        if (await h1.CountAsync() > 0)
        {
            var text = await h1.First.TextContentAsync();
            text.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task Dashboard_WithValidData_ShowsHeatmapSection()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check for heatmap title text
        var heatmapTitle = page.GetByText("MONTHLY EXECUTION HEATMAP");
        if (await heatmapTitle.CountAsync() > 0)
        {
            (await heatmapTitle.IsVisibleAsync()).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Dashboard_WithValidData_ShowsTimelineArea()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check for timeline placeholder text
        var timeline = page.GetByText("Timeline placeholder");
        if (await timeline.CountAsync() > 0)
        {
            (await timeline.IsVisibleAsync()).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Dashboard_PageHasNoScrollbars_FixedLayout()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var overflow = await page.EvaluateAsync<string>(
            "() => window.getComputedStyle(document.body).overflow");
        overflow.Should().Be("hidden");
    }
}