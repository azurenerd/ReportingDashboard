using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class LegendTests
{
    private readonly PlaywrightFixture _fixture;

    public LegendTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<DashboardPage> LoadDashboardAsync()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();
        return dashboard;
    }

    [Fact]
    public async Task Legend_DisplaysFourItems()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.LegendItems.CountAsync();
            count.Should().Be(4);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_DisplaysFourItems));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ContainsPocMilestoneLabel()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var legendText = await dashboard.HeaderRight.TextContentAsync();
            legendText.Should().Contain("PoC Milestone");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_ContainsPocMilestoneLabel));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ContainsProductionReleaseLabel()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var legendText = await dashboard.HeaderRight.TextContentAsync();
            legendText.Should().Contain("Production Release");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_ContainsProductionReleaseLabel));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ContainsCheckpointLabel()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var legendText = await dashboard.HeaderRight.TextContentAsync();
            legendText.Should().Contain("Checkpoint");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_ContainsCheckpointLabel));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ContainsNowLabel()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var legendText = await dashboard.HeaderRight.TextContentAsync();
            legendText.Should().Contain("Now");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_ContainsNowLabel));
            throw;
        }
    }

    [Fact]
    public async Task Legend_PocDiamond_HasGoldBackground()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var diamonds = dashboard.LegendDiamonds;
            var count = await diamonds.CountAsync();
            count.Should().BeGreaterThanOrEqualTo(2);

            // First diamond is PoC (gold #F4B400)
            var bgColor = await diamonds.Nth(0).EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bgColor.Should().NotBeNullOrWhiteSpace();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_PocDiamond_HasGoldBackground));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ProductionDiamond_HasGreenBackground()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var diamonds = dashboard.LegendDiamonds;
            var count = await diamonds.CountAsync();
            count.Should().BeGreaterThanOrEqualTo(2);

            // Second diamond is Production (green #34A853)
            var bgColor = await diamonds.Nth(1).EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bgColor.Should().NotBeNullOrWhiteSpace();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_ProductionDiamond_HasGreenBackground));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ItemsHave12pxFont()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.LegendItems.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var fontSize = await dashboard.LegendItems.Nth(i).EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");
                fontSize.Should().Be("12px");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Legend_ItemsHave12pxFont));
            throw;
        }
    }
}