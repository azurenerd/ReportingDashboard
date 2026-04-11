using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class LayoutStructureTests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutStructureTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Layout_MainContainer_HasCorrectDimensions()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        await Assertions.Expect(container).ToBeVisibleAsync();

        var box = await container.BoundingBoxAsync();
        box.Should().NotBeNull();
        box!.Width.Should().BeApproximately(1920, 2);
        box.Height.Should().BeApproximately(1080, 2);
    }

    [Fact]
    public async Task Layout_MainContainer_IsFlexColumn()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        var display = await container.EvaluateAsync<string>("el => getComputedStyle(el).display");
        var direction = await container.EvaluateAsync<string>("el => getComputedStyle(el).flexDirection");

        display.Should().Be("flex");
        direction.Should().Be("column");
    }

    [Fact]
    public async Task Layout_MainContainer_HasOverflowHidden()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        var overflow = await container.EvaluateAsync<string>("el => getComputedStyle(el).overflow");
        overflow.Should().Be("hidden");
    }

    [Fact]
    public async Task Layout_MainContainer_HasSegoeUIFont()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        var fontFamily = await container.EvaluateAsync<string>("el => getComputedStyle(el).fontFamily");
        fontFamily.Should().Contain("Segoe UI");
    }

    [Fact]
    public async Task Layout_MainContainer_HasWhiteBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        var bg = await container.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        // #fff = rgb(255, 255, 255)
        bg.Should().Contain("255");
    }

    [Fact]
    public async Task Layout_MainContainer_HasDarkTextColor()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        var color = await container.EvaluateAsync<string>("el => getComputedStyle(el).color");
        // #111 = rgb(17, 17, 17)
        color.Should().Contain("17");
    }

    [Fact]
    public async Task Layout_WithValidData_AllThreeSectionsWithinContainer()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var containerBox = await dp.MainContainer.BoundingBoxAsync();
        var headerBox = await dp.Header.BoundingBoxAsync();
        var timelineBox = await dp.TimelineArea.BoundingBoxAsync();
        var heatmapBox = await dp.HeatmapWrap.BoundingBoxAsync();

        if (containerBox is null || headerBox is null || timelineBox is null || heatmapBox is null)
            return;

        // All sections should be within the 1920x1080 container
        headerBox.X.Should().BeGreaterOrEqualTo(containerBox.X);
        timelineBox.X.Should().BeGreaterOrEqualTo(containerBox.X);
        heatmapBox.X.Should().BeGreaterOrEqualTo(containerBox.X);

        (headerBox.Y + headerBox.Height).Should().BeLessOrEqualTo(containerBox.Y + containerBox.Height + 1);
        (timelineBox.Y + timelineBox.Height).Should().BeLessOrEqualTo(containerBox.Y + containerBox.Height + 1);
    }

    [Fact]
    public async Task Layout_HeaderIsTopSection()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var containerBox = await dp.MainContainer.BoundingBoxAsync();
        var headerBox = await dp.Header.BoundingBoxAsync();

        if (containerBox is null || headerBox is null) return;

        headerBox.Y.Should().BeApproximately(containerBox.Y, 5,
            "header should be at the top of the container");
    }

    [Fact]
    public async Task Layout_SectionsDoNotOverlap()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var headerBox = await dp.Header.BoundingBoxAsync();
        var timelineBox = await dp.TimelineArea.BoundingBoxAsync();
        var heatmapBox = await dp.HeatmapWrap.BoundingBoxAsync();

        if (headerBox is null || timelineBox is null || heatmapBox is null) return;

        var headerBottom = headerBox.Y + headerBox.Height;
        var timelineBottom = timelineBox.Y + timelineBox.Height;

        timelineBox.Y.Should().BeGreaterOrEqualTo(headerBottom - 2,
            "timeline should not overlap with header");
        heatmapBox.Y.Should().BeGreaterOrEqualTo(timelineBottom - 2,
            "heatmap should not overlap with timeline");
    }

    [Fact]
    public async Task Layout_HeatmapFillsRemainingVerticalSpace()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        if (await dp.ErrorSection.CountAsync() > 0) return;

        var containerBox = await dp.MainContainer.BoundingBoxAsync();
        var heatmapBox = await dp.HeatmapWrap.BoundingBoxAsync();

        if (containerBox is null || heatmapBox is null) return;

        var containerBottom = containerBox.Y + containerBox.Height;
        var heatmapBottom = heatmapBox.Y + heatmapBox.Height;

        // Heatmap should extend close to the bottom of the container
        heatmapBottom.Should().BeGreaterThan(containerBottom - 50,
            "heatmap should fill remaining space to near the bottom");
    }
}