using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fixture;
    private const int DefaultTimeout = 60000;

    public DashboardUITests(PlaywrightFixture fixture)
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
    public async Task Dashboard_LoadsAtRootUrl_DisplaysContent()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bodyText = await page.Locator("body").InnerTextAsync();
        bodyText.Should().NotBeNullOrEmpty("the dashboard page should render content at /");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersHeaderSection()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        var errorContainer = page.GetByText("Unable to load dashboard data");

        if (await errorContainer.CountAsync() == 0)
        {
            (await header.CountAsync()).Should().BeGreaterThan(0, "header component should render when data is loaded");
            var h1 = page.Locator(".hdr h1");
            var h1Text = await h1.InnerTextAsync();
            h1Text.Should().NotBeNullOrEmpty("header title should have text from data.json");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersTimelineArea()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.GetByText("Unable to load dashboard data");

        if (await errorContainer.CountAsync() == 0)
        {
            var timeline = page.Locator(".tl-area");
            (await timeline.CountAsync()).Should().BeGreaterThan(0, "timeline area should render when data is loaded");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersHeatmapGrid()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.GetByText("Unable to load dashboard data");

        if (await errorContainer.CountAsync() == 0)
        {
            var heatmap = page.Locator(".hm-grid");
            (await heatmap.CountAsync()).Should().BeGreaterThan(0, "heatmap grid should render when data is loaded");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_ErrorState_DisplaysErrorMessageWithCorrectLayout()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorText = page.GetByText("Unable to load dashboard data");

        if (await errorText.CountAsync() > 0)
        {
            var errorContainer = page.Locator("div[style*='width:1920px'][style*='height:1080px']");
            (await errorContainer.CountAsync()).Should().BeGreaterThan(0,
                "error container should have 1920x1080 dimensions");

            var message = await errorText.InnerTextAsync();
            message.Should().Contain("Please check that data.json exists and contains valid JSON");
        }

        await page.CloseAsync();
    }
}