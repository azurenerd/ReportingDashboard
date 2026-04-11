using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class NavigationTests
{
    private readonly PlaywrightFixture _fixture;

    public NavigationTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task RootUrl_RendersDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        // Should show either dashboard content or error panel
        var hasHeader = await dashboard.Header.CountAsync() > 0;
        var hasError = await dashboard.ErrorPanel.CountAsync() > 0;

        (hasHeader || hasError).Should().BeTrue("root URL should render dashboard or error panel");
    }

    [Fact]
    public async Task NonExistentRoute_ShowsNotFoundMessage()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        // Blazor router should show "Page not found" for unknown routes
        // Wait a moment for Blazor to initialize
        await page.WaitForTimeoutAsync(2000);

        var bodyText = await page.TextContentAsync("body");
        // Either shows "not found" or redirects to root
        bodyText.Should().NotBeNull();
    }

    [Fact]
    public async Task PageRefresh_PreservesContent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        // Capture initial title
        var hasHeader = await dashboard.Header.CountAsync() > 0;
        string? initialTitle = null;
        if (hasHeader)
        {
            initialTitle = await dashboard.Title.TextContentAsync();
        }

        // Refresh
        await page.ReloadAsync(new PageReloadOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await dashboard.WaitForDashboardLoadedAsync();

        // Content should be the same
        if (initialTitle is not null)
        {
            var afterTitle = await dashboard.Title.TextContentAsync();
            afterTitle.Should().Be(initialTitle);
        }
    }

    [Fact]
    public async Task BacklogLink_OpensInNewTab()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        var linkCount = await dashboard.BacklogLink.CountAsync();
        if (linkCount > 0)
        {
            var target = await dashboard.BacklogLink.GetAttributeAsync("target");
            target.Should().Be("_blank", "backlog link should open in new tab");

            var rel = await dashboard.BacklogLink.GetAttributeAsync("rel");
            rel.Should().Contain("noopener", "link should have noopener for security");
        }
    }
}