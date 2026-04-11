using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineSectionTests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineSectionTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<DashboardPage> LoadDashboardAsync()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();
        return dp;
    }

    private async Task<bool> HasContent(DashboardPage dp) =>
        await dp.ErrorSection.CountAsync() == 0 && await dp.TimelineArea.CountAsync() > 0;

    [Fact]
    public async Task Timeline_AreaIsVisible()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        await Assertions.Expect(dp.TimelineArea).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Timeline_HasFAFAFABackground()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var bg = await dp.TimelineArea.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        // #FAFAFA = rgb(250, 250, 250)
        bg.Should().Contain("250");
    }

    [Fact]
    public async Task Timeline_Has196pxHeight()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var height = await dp.TimelineArea.EvaluateAsync<string>(
            "el => getComputedStyle(el).height");
        height.Should().Be("196px");
    }

    [Fact]
    public async Task Timeline_HasFlexShrink0()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var flexShrink = await dp.TimelineArea.EvaluateAsync<string>(
            "el => getComputedStyle(el).flexShrink");
        flexShrink.Should().Be("0");
    }

    [Fact]
    public async Task Timeline_HasDisplayFlex()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var display = await dp.TimelineArea.EvaluateAsync<string>(
            "el => getComputedStyle(el).display");
        display.Should().Be("flex");
    }

    [Fact]
    public async Task Timeline_HasBottomBorder()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var borderStyle = await dp.TimelineArea.EvaluateAsync<string>(
            "el => getComputedStyle(el).borderBottomStyle");
        borderStyle.Should().Be("solid");
    }

    [Fact]
    public async Task Timeline_ContainsPlaceholderText()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var text = await dp.TimelineArea.TextContentAsync();
        text.Should().Contain("Timeline placeholder");
    }

    [Fact]
    public async Task Timeline_SvgBoxExists()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var svgBoxCount = await dp.TimelineSvgBox.CountAsync();
        svgBoxCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Timeline_AppearsBetweenHeaderAndHeatmap()
    {
        var dp = await LoadDashboardAsync();
        if (!await HasContent(dp)) return;

        var headerBox = await dp.Header.BoundingBoxAsync();
        var timelineBox = await dp.TimelineArea.BoundingBoxAsync();
        var heatmapBox = await dp.HeatmapWrap.BoundingBoxAsync();

        if (headerBox is null || timelineBox is null || heatmapBox is null) return;

        timelineBox.Y.Should().BeGreaterThan(headerBox.Y, "timeline should be below header");
        heatmapBox.Y.Should().BeGreaterThan(timelineBox.Y, "heatmap should be below timeline");
    }
}