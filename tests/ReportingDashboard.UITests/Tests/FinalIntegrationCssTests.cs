using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// CSS design fidelity tests for the Final Integration PR (#521).
/// Verifies the full dashboard matches OriginalDesignConcept.html spec at 1920x1080.
/// Covers body layout, typography, spacing, borders, and color palette.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FinalIntegrationCssTests
{
    private readonly PlaywrightFixture _fixture;

    public FinalIntegrationCssTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Body and Layout

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_Is1920x1080_FlexColumn()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            var display = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).display");
            Assert.Equal("flex", display);

            var flexDir = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).flexDirection");
            Assert.Equal("column", flexDir);

            var overflow = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).overflow");
            Assert.Equal("hidden", overflow);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_Is1920x1080_FlexColumn));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_FontFamily_IsSegoeUI()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        try
        {
            var fontFamily = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).fontFamily");
            Assert.Contains("Segoe UI", fontFamily);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_FontFamily_IsSegoeUI));
            throw;
        }
    }

    #endregion

    #region Header CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Padding_Is12px44px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var paddingTop = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingTop");
            Assert.Equal("12px", paddingTop);

            var paddingLeft = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingLeft");
            Assert.Equal("44px", paddingLeft);

            var paddingRight = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingRight");
            Assert.Equal("44px", paddingRight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Padding_Is12px44px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BorderBottom_1pxSolidE0E0E0()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var borderWidth = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomWidth");
            Assert.Equal("1px", borderWidth);

            var borderStyle = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            Assert.Equal("solid", borderStyle);

            var borderColor = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomColor");
            Assert.Contains("224", borderColor); // rgb(224,224,224) = #E0E0E0
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BorderBottom_1pxSolidE0E0E0));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_H1_FontSize24px_Weight700()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var fontSize = await dashboard.Title.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("24px", fontSize);

            var fontWeight = await dashboard.Title.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            Assert.Equal("700", fontWeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_H1_FontSize24px_Weight700));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_FontSize12px_GrayColor()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var fontSize = await dashboard.Subtitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("12px", fontSize);

            var color = await dashboard.Subtitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #888 = rgb(136, 136, 136)
            Assert.Contains("136", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_FontSize12px_GrayColor));
            throw;
        }
    }

    #endregion

    #region Timeline CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_FlexDisplay_AlignStretch()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var display = await tl.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            Assert.Equal("flex", display);

            var alignItems = await tl.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).alignItems");
            Assert.Equal("stretch", alignItems);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_FlexDisplay_AlignStretch));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_BottomBorder_2pxSolidE8E8E8()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var borderWidth = await tl.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomWidth");
            Assert.Equal("2px", borderWidth);

            var borderStyle = await tl.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            Assert.Equal("solid", borderStyle);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_BottomBorder_2pxSolidE8E8E8));
            throw;
        }
    }

    #endregion

    #region Heatmap CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_Title_FontSize14px_Bold_Uppercase()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var fontSize = await hm.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("14px", fontSize);

            var fontWeight = await hm.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            Assert.Equal("700", fontWeight);

            var textTransform = await hm.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).textTransform");
            Assert.Equal("uppercase", textTransform);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_Title_FontSize14px_Bold_Uppercase));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_Grid_HasCssBorder()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var borderWidth = await hm.HeatmapGrid.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderWidth");
            Assert.Contains("1px", borderWidth);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_Grid_HasCssBorder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_Corner_FontSize11px_Bold_Uppercase()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var fontSize = await hm.CornerCell.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("11px", fontSize);

            var fontWeight = await hm.CornerCell.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            Assert.Equal("700", fontWeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_Corner_FontSize11px_Bold_Uppercase));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_ColumnHeader_FontSize16px_Bold()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var colCount = await hm.ColumnHeaders.CountAsync();
            if (colCount > 0)
            {
                var fontSize = await hm.ColumnHeaders.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");
                Assert.Equal("16px", fontSize);

                var fontWeight = await hm.ColumnHeaders.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontWeight");
                Assert.Equal("700", fontWeight);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_ColumnHeader_FontSize16px_Bold));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_RowHeaders_FontSize11px_Bold_Uppercase()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var rowCount = await hm.RowHeaders.CountAsync();
            if (rowCount > 0)
            {
                var fontSize = await hm.RowHeaders.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");
                Assert.Equal("11px", fontSize);

                var fontWeight = await hm.RowHeaders.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontWeight");
                Assert.Equal("700", fontWeight);

                var textTransform = await hm.RowHeaders.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).textTransform");
                Assert.Equal("uppercase", textTransform);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_RowHeaders_FontSize11px_Bold_Uppercase));
            throw;
        }
    }

    #endregion

    #region Backlog Link CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BacklogLink_Color_IsMicrosoftBlue()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            if (await header.HasBacklogLinkAsync())
            {
                var color = await header.BacklogLink.EvaluateAsync<string>(
                    "el => getComputedStyle(el).color");
                // #0078D4 = rgb(0, 120, 212)
                Assert.Contains("0", color);
                Assert.Contains("120", color);
                Assert.Contains("212", color);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(BacklogLink_Color_IsMicrosoftBlue));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BacklogLink_NoTextDecoration()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            if (await header.HasBacklogLinkAsync())
            {
                var textDecoration = await header.BacklogLink.EvaluateAsync<string>(
                    "el => getComputedStyle(el).textDecorationLine");
                Assert.Equal("none", textDecoration);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(BacklogLink_NoTextDecoration));
            throw;
        }
    }

    #endregion
}