using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineTests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_HasTrackLabels()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var trackCount = await dashboard.GetTrackCountAsync();
            Assert.True(trackCount > 0, "Expected at least one timeline track");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_HasTrackLabels));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_TrackLabelsHaveNameAndDescription()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var trackCount = await dashboard.TimelineTrackLabels.CountAsync();
            for (int i = 0; i < trackCount; i++)
            {
                var label = dashboard.TimelineTrackLabels.Nth(i);
                var idText = await label.Locator(".tl-track-id").TextContentAsync();
                var nameText = await label.Locator(".tl-track-name").TextContentAsync();
                Assert.False(string.IsNullOrWhiteSpace(idText), $"Track {i} ID should not be empty");
                Assert.False(string.IsNullOrWhiteSpace(nameText), $"Track {i} name should not be empty");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_TrackLabelsHaveNameAndDescription));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_TrackIdsHaveDistinctColors()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var count = await dashboard.TimelineTrackIds.CountAsync();
            if (count > 1)
            {
                var colors = new HashSet<string>();
                for (int i = 0; i < count; i++)
                {
                    var color = await dashboard.TimelineTrackIds.Nth(i)
                        .EvaluateAsync<string>("el => getComputedStyle(el).color");
                    colors.Add(color);
                }
                Assert.True(colors.Count > 1, "Expected tracks to have distinct colors");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_TrackIdsHaveDistinctColors));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_ContainsSvgElement()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.TimelineSvg).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_ContainsSvgElement));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SvgHasWidth1560()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var width = await dashboard.TimelineSvg.GetAttributeAsync("width");
            Assert.Equal("1560", width);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SvgHasWidth1560));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SvgContainsNowMarker()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var nowText = dashboard.TimelineSvgBox.Locator("text:has-text('NOW')");
            Assert.True(await nowText.CountAsync() > 0, "Expected NOW marker in SVG");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SvgContainsNowMarker));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SvgContainsPolygons_ForMilestones()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var polygons = dashboard.TimelineSvgBox.Locator("polygon");
            var count = await polygons.CountAsync();
            Assert.True(count > 0, "Expected at least one diamond polygon in SVG");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SvgContainsPolygons_ForMilestones));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SvgHasDropShadowFilter()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var filter = dashboard.TimelineSvgBox.Locator("filter#sh");
            Assert.True(await filter.CountAsync() > 0, "Expected drop shadow filter in SVG defs");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SvgHasDropShadowFilter));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_HasFafafaBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var bg = await dashboard.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #FAFAFA = rgb(250, 250, 250)
            Assert.Contains("250", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_HasFafafaBackground));
            throw;
        }
    }
}