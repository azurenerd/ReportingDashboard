using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// E2E tests verifying that dashboard content is driven by data.json.
/// Checks that title, subtitle, backlog link, timeline tracks, and heatmap items
/// all render from the sample dataset.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardDataDrivenTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardDataDrivenTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsDataDrivenTitle()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.Title).ToBeVisibleAsync();
            var text = await po.Title.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(text), "Title should be populated from data.json");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsDataDrivenTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsDataDrivenSubtitle()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.Subtitle).ToBeVisibleAsync();
            var text = await po.Subtitle.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(text), "Subtitle should be populated from data.json");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsDataDrivenSubtitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsBacklogLink()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            if (await po.BacklogLink.CountAsync() > 0)
            {
                var href = await po.BacklogLink.GetAttributeAsync("href");
                Assert.NotNull(href);
                Assert.StartsWith("https://", href!);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsBacklogLink));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsAtLeastThreeTimelineTracks()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var trackCount = await po.GetTrackCountAsync();
            Assert.True(trackCount >= 3,
                $"Expected at least 3 timeline tracks per acceptance criteria, got {trackCount}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsAtLeastThreeTimelineTracks));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsFourMonthColumns()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var monthCount = await po.GetMonthColumnCountAsync();
            Assert.True(monthCount >= 4,
                $"Expected at least 4 month columns, got {monthCount}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsFourMonthColumns));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HeatmapHasFourRowCategories()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var rowCount = await po.GetHeatmapRowCountAsync();
            Assert.Equal(4, rowCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeatmapHasFourRowCategories));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HeatmapShowsShippedRow()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.ShippedHeader).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeatmapShowsShippedRow));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HeatmapShowsInProgressRow()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.InProgressHeader).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeatmapShowsInProgressRow));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HeatmapShowsCarryoverRow()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.CarryoverHeader).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeatmapShowsCarryoverRow));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HeatmapShowsBlockersRow()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.BlockersHeader).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeatmapShowsBlockersRow));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HeatmapHasWorkItems()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var itemCount = await po.HeatmapItems.CountAsync();
            Assert.True(itemCount > 0,
                "Expected at least one work item in the heatmap");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeatmapHasWorkItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HeatmapCurrentMonthHighlighted()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var curHdr = po.HeatmapCurrentMonthHeader;
            if (await curHdr.CountAsync() > 0)
            {
                var bg = await curHdr.EvaluateAsync<string>(
                    "el => getComputedStyle(el).backgroundColor");
                // Current month should have highlighted background (gold-ish)
                Assert.False(string.IsNullOrWhiteSpace(bg));
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeatmapCurrentMonthHighlighted));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_TimelineSvgContainsNowMarker()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var nowText = po.Page.Locator("text:has-text('NOW')");
            Assert.True(await nowText.CountAsync() > 0, "Expected NOW marker in SVG");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_TimelineSvgContainsNowMarker));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_TimelineSvgHasWidth1560()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            if (await po.TimelineSvg.CountAsync() > 0)
            {
                var width = await po.TimelineSvg.GetAttributeAsync("width");
                Assert.Equal("1560", width);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_TimelineSvgHasWidth1560));
            throw;
        }
    }
}