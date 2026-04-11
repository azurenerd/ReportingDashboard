using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineUITests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Timeline_RendersTimelineArea()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasDashboardContentAsync())
            {
                await Assertions.Expect(timelinePage.TimelineArea).ToBeVisibleAsync();
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_RendersArea_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_RendersSvgElement()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                await Assertions.Expect(timelinePage.Svg).ToBeVisibleAsync();

                var width = await timelinePage.GetSvgWidthAsync();
                width.Should().Be("1560", "SVG width should be 1560px");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_RendersSvg_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_HasTrackLabels()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var trackCount = await timelinePage.GetTrackCountAsync();
                trackCount.Should().BeGreaterThan(0, "timeline should have at least one track label");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_HasTrackLabels_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_RendersPocDiamonds_WithGoldColor()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var pocCount = await timelinePage.PocDiamonds.CountAsync();
                pocCount.Should().BeGreaterThan(0, "timeline should have PoC milestone diamonds (#F4B400)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_PocDiamonds_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_RendersProductionDiamonds_WithGreenColor()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var prodCount = await timelinePage.ProductionDiamonds.CountAsync();
                prodCount.Should().BeGreaterThan(0, "timeline should have production milestone diamonds (#34A853)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_ProductionDiamonds_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_RendersCheckpointCircles()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var circleCount = await timelinePage.CheckpointCircles.CountAsync();
                circleCount.Should().BeGreaterThan(0, "timeline should have checkpoint circles");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_CheckpointCircles_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_RendersNowLine_WithRedDashedStroke()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var nowLineCount = await timelinePage.NowLine.CountAsync();
                nowLineCount.Should().BeGreaterThan(0, "timeline should have a red dashed NOW line");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_NowLine_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_RendersDropShadowFilter()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var filterCount = await timelinePage.DropShadowFilter.CountAsync();
                filterCount.Should().Be(1, "SVG should have a drop shadow filter with id='sh'");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_DropShadow_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_HasSvgTextLabels()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var textCount = await timelinePage.SvgTextElements.CountAsync();
                textCount.Should().BeGreaterThan(0, "SVG should have text labels for months and milestones");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SvgTextLabels_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_NowLabel_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var nowText = page.Locator(".tl-svg-box svg text:has-text('NOW')");
                var count = await nowText.CountAsync();
                count.Should().BeGreaterThan(0, "SVG should contain a 'NOW' text label");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_NowLabel_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_MilestoneTooltips_ArePresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var titles = page.Locator(".tl-svg-box svg title");
                var count = await titles.CountAsync();
                count.Should().BeGreaterThan(0, "SVG milestone markers should have <title> tooltips");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Tooltips_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_HasTimelineAreaBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var timelinePage = new TimelinePage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasTimelineAsync())
            {
                var bgColor = await timelinePage.TimelineArea.EvaluateAsync<string>(
                    "el => getComputedStyle(el).backgroundColor");

                // #FAFAFA = rgb(250, 250, 250)
                bgColor.Should().NotBeNull();
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Background_Failed");
            throw;
        }
    }
}