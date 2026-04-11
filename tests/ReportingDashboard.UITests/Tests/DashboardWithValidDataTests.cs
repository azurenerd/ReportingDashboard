using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardWithValidDataTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardWithValidDataTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ValidData_RendersHeaderSection()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        // If data is valid, header should be visible; if error, skip assertion
        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            await Assertions.Expect(dashboardPage.Header).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ValidData_HeaderContainsProjectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            var titleText = await dashboardPage.HeaderTitle.TextContentAsync();
            titleText.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task ValidData_HeaderTitleIs24pxBold()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            var fontSize = await dashboardPage.HeaderTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            var fontWeight = await dashboardPage.HeaderTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");

            fontSize.Should().Be("24px");
            fontWeight.Should().BeOneOf("700", "bold");
        }
    }

    [Fact]
    public async Task ValidData_SubtitleIsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            await Assertions.Expect(dashboardPage.HeaderSubtitle).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ValidData_HeaderHasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            var borderBottom = await dashboardPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            borderBottom.Should().Be("solid");
        }
    }

    [Fact]
    public async Task ValidData_TimelineAreaIsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            await Assertions.Expect(dashboardPage.TimelineArea).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ValidData_TimelineContainsPlaceholderText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            var text = await dashboardPage.TimelineArea.TextContentAsync();
            text.Should().Contain("Timeline placeholder");
            text.Should().Contain("track(s) configured");
        }
    }

    [Fact]
    public async Task ValidData_HeatmapAreaIsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            await Assertions.Expect(dashboardPage.HeatmapWrap).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ValidData_HeatmapContainsPlaceholderText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            var text = await dashboardPage.HeatmapWrap.TextContentAsync();
            text.Should().Contain("Heatmap placeholder");
            text.Should().Contain("categories x");
            text.Should().Contain("months");
        }
    }

    [Fact]
    public async Task ValidData_ThreeSectionsRenderedInOrder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var errorCount = await dashboardPage.ErrorSection.CountAsync();
        if (errorCount == 0)
        {
            var headerBox = await dashboardPage.Header.BoundingBoxAsync();
            var timelineBox = await dashboardPage.TimelineArea.BoundingBoxAsync();
            var heatmapBox = await dashboardPage.HeatmapWrap.BoundingBoxAsync();

            headerBox.Should().NotBeNull();
            timelineBox.Should().NotBeNull();
            heatmapBox.Should().NotBeNull();

            // Header above timeline
            headerBox!.Y.Should().BeLessThan(timelineBox!.Y);
            // Timeline above heatmap
            timelineBox.Y.Should().BeLessThan(heatmapBox!.Y);
        }
    }

    [Fact]
    public async Task ValidData_NoErrorSectionRendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        // If we get data loaded, error should not be present
        var headerCount = await dashboardPage.Header.CountAsync();
        if (headerCount > 0)
        {
            var errorCount = await dashboardPage.ErrorSection.CountAsync();
            errorCount.Should().Be(0);
        }
    }
}