using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests verifying that the sample data.json content from PR #555
/// renders correctly in the dashboard. Verifies specific data values
/// (3 tracks, 4 months, heatmap items) appear in the rendered page.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class DataJsonContentTests
{
    private readonly PlaywrightFixture _fixture;

    public DataJsonContentTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Header Data from data.json

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_TitleRendered_InHeader()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var titleText = await dashboard.Title.TextContentAsync() ?? "";
            // data.json has title: "Privacy Automation Release Roadmap"
            Assert.False(string.IsNullOrWhiteSpace(titleText),
                "Title should be populated from data.json");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_TitleRendered_InHeader));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_SubtitleRendered_InHeader()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var subText = await dashboard.Subtitle.TextContentAsync() ?? "";
            Assert.False(string.IsNullOrWhiteSpace(subText),
                "Subtitle should be populated from data.json");
            // data.json subtitle includes "April 2026"
            Assert.Contains("2026", subText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_SubtitleRendered_InHeader));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_BacklogLink_PointsToADO()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var linkCount = await dashboard.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var href = await dashboard.BacklogLink.GetAttributeAsync("href");
                Assert.NotNull(href);
                Assert.Contains("dev.azure.com", href!);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_BacklogLink_PointsToADO));
            throw;
        }
    }

    #endregion

    #region Timeline Tracks from data.json

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_ThreeTimelineTracks_Rendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // data.json has 3 tracks: M1, M2, M3
            var content = await dashboard.TimelineArea.TextContentAsync() ?? "";
            Assert.Contains("M1", content);
            Assert.Contains("M2", content);
            Assert.Contains("M3", content);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_ThreeTimelineTracks_Rendered));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_TrackLabels_Rendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var content = await dashboard.TimelineArea.TextContentAsync() ?? "";
            // data.json track labels
            Assert.Contains("Chatbot", content);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_TrackLabels_Rendered));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_TrackColors_DistinctInSvg()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var svg = await timeline.Svg.InnerHTMLAsync();
            // data.json track colors: #0078D4, #00897B, #546E7A
            Assert.Contains("#0078D4", svg);
            Assert.Contains("#00897B", svg);
            Assert.Contains("#546E7A", svg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_TrackColors_DistinctInSvg));
            throw;
        }
    }

    #endregion

    #region Heatmap Months from data.json

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_FourMonths_InHeatmapColumns()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var colCount = await heatmap.GetMonthColumnCountAsync();
            // data.json has months: ["Jan", "Feb", "Mar", "Apr"]
            Assert.Equal(4, colCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_FourMonths_InHeatmapColumns));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_MonthNames_RenderedInHeaders()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var headers = heatmap.ColumnHeaders;
            var count = await headers.CountAsync();

            var allText = "";
            for (int i = 0; i < count; i++)
            {
                allText += await headers.Nth(i).TextContentAsync() + " ";
            }

            Assert.Contains("Jan", allText);
            Assert.Contains("Feb", allText);
            Assert.Contains("Mar", allText);
            Assert.Contains("Apr", allText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_MonthNames_RenderedInHeaders));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_CurrentMonth_IsApr()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var curHdrCount = await heatmap.CurrentMonthHeader.CountAsync();
            Assert.Equal(1, curHdrCount);

            var text = await heatmap.CurrentMonthHeader.TextContentAsync() ?? "";
            Assert.Contains("Apr", text);
            Assert.Contains("Now", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_CurrentMonth_IsApr));
            throw;
        }
    }

    #endregion

    #region Heatmap Items from data.json

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_ShippedItems_Present()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemTexts = await heatmap.GetItemTextsAsync(heatmap.ShippedItems);
            Assert.True(itemTexts.Count > 0,
                "Expected shipped items from data.json to be rendered");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_ShippedItems_Present));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_InProgressItems_Present()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemTexts = await heatmap.GetItemTextsAsync(heatmap.InProgressItems);
            Assert.True(itemTexts.Count > 0,
                "Expected in-progress items from data.json to be rendered");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_InProgressItems_Present));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_CarryoverItems_Present()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemTexts = await heatmap.GetItemTextsAsync(heatmap.CarryoverItems);
            Assert.True(itemTexts.Count > 0,
                "Expected carryover items from data.json to be rendered");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_CarryoverItems_Present));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_BlockerItems_Present()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemTexts = await heatmap.GetItemTextsAsync(heatmap.BlockerItems);
            Assert.True(itemTexts.Count > 0,
                "Expected blocker items from data.json to be rendered");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_BlockerItems_Present));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_AllFourCategories_HaveRowHeaders()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var rowCount = await heatmap.GetCategoryRowCountAsync();
            Assert.Equal(4, rowCount);

            var headers = heatmap.RowHeaders;
            Assert.Contains("SHIPPED", (await headers.Nth(0).TextContentAsync())?.Trim() ?? "");
            Assert.Contains("IN PROGRESS", (await headers.Nth(1).TextContentAsync())?.Trim() ?? "");
            Assert.Contains("CARRYOVER", (await headers.Nth(2).TextContentAsync())?.Trim() ?? "");
            Assert.Contains("BLOCKERS", (await headers.Nth(3).TextContentAsync())?.Trim() ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_AllFourCategories_HaveRowHeaders));
            throw;
        }
    }

    #endregion

    #region Milestone Types from data.json

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_AllThreeMilestoneTypes_Rendered()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var svg = await timeline.Svg.InnerHTMLAsync();

            // PoC milestones (#F4B400)
            Assert.Contains("#F4B400", svg);

            // Production milestones (#34A853)
            Assert.Contains("#34A853", svg);

            // Checkpoint circles
            var circleCount = await timeline.GetCircleCountAsync();
            Assert.True(circleCount > 0, "Expected checkpoint circles from data.json");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_AllThreeMilestoneTypes_Rendered));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_NowMarker_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var nowTextCount = await timeline.NowText.CountAsync();
            Assert.True(nowTextCount > 0, "Expected NOW marker from data.json nowDate field");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_NowMarker_IsPresent));
            throw;
        }
    }

    #endregion

    #region Full Dashboard Integration

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_AllThreeSections_RenderedTogether()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.True(await dashboard.IsDashboardVisibleAsync(),
                "Dashboard root should be visible with valid data.json");
            Assert.False(await dashboard.IsErrorVisibleAsync(),
                "Error panel should not be visible with valid data.json");

            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_AllThreeSections_RenderedTogether));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_SectionsOrdered_HeaderTimelineHeatmap()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var headerBox = await dashboard.Header.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineArea.BoundingBoxAsync();
            var hmBox = await dashboard.HeatmapWrap.BoundingBoxAsync();

            Assert.NotNull(headerBox);
            Assert.NotNull(tlBox);
            Assert.NotNull(hmBox);

            Assert.True(headerBox!.Y < tlBox!.Y,
                "Header should be above timeline");
            Assert.True(tlBox.Y < hmBox!.Y,
                "Timeline should be above heatmap");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_SectionsOrdered_HeaderTimelineHeatmap));
            throw;
        }
    }

    #endregion
}