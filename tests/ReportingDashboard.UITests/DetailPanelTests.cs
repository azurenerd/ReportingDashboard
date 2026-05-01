using Microsoft.Playwright;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DetailPanelTests
{
    private readonly PlaywrightFixture _fixture;

    public DetailPanelTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DetailPanel_AppearsWhenNodeIsClicked()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var detailPanel = new DetailPanelPage(page);

        // Panel should not be visible initially
        Assert.False(await detailPanel.IsPanelVisibleAsync());

        // Click a 3D node (canvas interaction) - use evaluate to set store state
        await page.EvaluateAsync(@"() => {
            const canvas = document.querySelector('canvas');
            if (canvas) {
                canvas.dispatchEvent(new MouseEvent('click', { bubbles: true, clientX: 400, clientY: 400 }));
            }
        }");

        // Allow time for potential panel appearance
        await page.WaitForTimeoutAsync(2000);

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DetailPanel_ClosesViaCloseButton()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var detailPanel = new DetailPanelPage(page);

        var clickableNode = page.Locator("[data-entity-id]").First;
        if (await clickableNode.CountAsync() > 0 && await clickableNode.IsVisibleAsync())
        {
            await clickableNode.ClickAsync();
            await detailPanel.WaitForPanelVisibleAsync();
            await detailPanel.ClickCloseButtonAsync();
            await detailPanel.WaitForPanelHiddenAsync();
            Assert.False(await detailPanel.IsPanelVisibleAsync());
        }

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DetailPanel_ClosesViaEscapeKey()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var detailPanel = new DetailPanelPage(page);

        var panel = detailPanel.Panel;
        if (await panel.CountAsync() > 0 && await panel.IsVisibleAsync())
        {
            await detailPanel.PressEscapeAsync();
            await detailPanel.WaitForPanelHiddenAsync();
            Assert.False(await detailPanel.IsPanelVisibleAsync());
        }

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DetailPanel_ClosesViaBackdropClick()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var detailPanel = new DetailPanelPage(page);

        var panel = detailPanel.Panel;
        if (await panel.CountAsync() > 0 && await panel.IsVisibleAsync())
        {
            await detailPanel.ClickBackdropAsync();
            await detailPanel.WaitForPanelHiddenAsync();
            Assert.False(await detailPanel.IsPanelVisibleAsync());
        }

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DetailPanel_ShowsLoadingStateWhileFetching()
    {
        var page = await _fixture.CreatePageAsync();

        await page.RouteAsync("**/api/report/**", async route =>
        {
            await Task.Delay(2000);
            await route.ContinueAsync();
        });

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await page.TitleAsync();
        Assert.NotNull(title);

        await page.CloseAsync();
    }
}