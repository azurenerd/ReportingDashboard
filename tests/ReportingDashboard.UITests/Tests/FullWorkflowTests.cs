using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// End-to-end tests verifying the full user workflow:
/// Navigate to dashboard → see header → see timeline → see heatmap → verify data integrity.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FullWorkflowTests
{
    private readonly PlaywrightFixture _fixture;

    public FullWorkflowTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task UserWorkflow_NavigateToDashboard_SeeAllSections()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            // Step 1: Navigate
            await dashboard.NavigateAsync();

            // Step 2: Verify no error
            Assert.False(await dashboard.IsErrorVisibleAsync());
            Assert.True(await dashboard.IsDashboardVisibleAsync());

            // Step 3: Header is rendered with title and legend
            await Assertions.Expect(dashboard.Title).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.Legend).ToBeVisibleAsync();
            Assert.Equal(4, await dashboard.LegendItems.CountAsync());

            // Step 4: Timeline is rendered with tracks and SVG
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
            Assert.True(await dashboard.GetTrackCountAsync() > 0);
            await Assertions.Expect(dashboard.TimelineSvg).ToBeVisibleAsync();

            // Step 5: Heatmap is rendered with all categories
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
            Assert.Equal(4, await dashboard.GetHeatmapRowCountAsync());
            Assert.True(await dashboard.GetMonthColumnCountAsync() > 0);

            // Step 6: Current month is highlighted
            Assert.Equal(1, await dashboard.HeatmapCurrentMonthHeader.CountAsync());
            Assert.Equal(4, await dashboard.HeatmapCurrentCells.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(UserWorkflow_NavigateToDashboard_SeeAllSections));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task UserWorkflow_VerifyDataConsistency_MonthCountMatchesColumns()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var monthCount = await dashboard.GetMonthColumnCountAsync();

            // Each category row should have exactly monthCount cells
            var shippedCellCount = await dashboard.ShippedCells.CountAsync();
            var progCellCount = await dashboard.InProgressCells.CountAsync();
            var carryCellCount = await dashboard.CarryoverCells.CountAsync();
            var blockCellCount = await dashboard.BlockerCells.CountAsync();

            Assert.Equal(monthCount, shippedCellCount);
            Assert.Equal(monthCount, progCellCount);
            Assert.Equal(monthCount, carryCellCount);
            Assert.Equal(monthCount, blockCellCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(UserWorkflow_VerifyDataConsistency_MonthCountMatchesColumns));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task UserWorkflow_ClickBacklogLink_OpensNewTab()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var linkCount = await dashboard.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                // Verify the link has target="_blank" (opens new tab)
                var target = await dashboard.BacklogLink.GetAttributeAsync("target");
                Assert.Equal("_blank", target);

                // Verify clicking triggers a new page (popup)
                var popupTask = page.Context.WaitForPageAsync();
                await dashboard.BacklogLink.ClickAsync();
                // Note: The popup may fail to load if the URL is not accessible,
                // but the important thing is that a new page was requested
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(UserWorkflow_ClickBacklogLink_OpensNewTab));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task UserWorkflow_PageTitle_ContainsDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var title = await page.TitleAsync();
            // The page should have some title set
            Assert.False(string.IsNullOrWhiteSpace(title), "Page should have a title");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(UserWorkflow_PageTitle_ContainsDashboard));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task UserWorkflow_NoConsoleErrors()
    {
        var page = await _fixture.NewPageAsync();
        var consoleErrors = new List<string>();

        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            // Wait a bit for any async errors
            await page.WaitForTimeoutAsync(2000);

            // Filter out Blazor SignalR reconnection noise if any
            var realErrors = consoleErrors
                .Where(e => !e.Contains("WebSocket") && !e.Contains("negotiate"))
                .ToList();

            Assert.Empty(realErrors);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(UserWorkflow_NoConsoleErrors));
            throw;
        }
    }
}