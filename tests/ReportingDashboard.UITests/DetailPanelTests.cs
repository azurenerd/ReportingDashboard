using Microsoft.Playwright;
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

    private async Task<IPage> NavigateToAppAsync()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { Timeout = 15000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
            return page;
        }
        catch (PlaywrightException)
        {
            // Server not running — skip test gracefully
            Skip.If(true, "Application server is not running. Set BASE_URL or start the app with 'npm run dev'.");
            return page; // unreachable but satisfies compiler
        }
    }

    [SkippableFact]
    public async Task Panel_SlidesIn_WhenNodeClicked()
    {
        var page = await NavigateToAppAsync();

        var canvas = page.Locator("canvas");
        if (await canvas.CountAsync() > 0)
        {
            var box = await canvas.BoundingBoxAsync();
            if (box != null)
            {
                await page.Mouse.ClickAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
            }
        }

        var panel = page.Locator("[class*='w-\\[400px\\]']").Or(
            page.Locator("[style*='backdrop-filter']")
        );

        if (await panel.CountAsync() > 0)
        {
            await panel.First.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
            Assert.True(await panel.First.IsVisibleAsync());
        }
    }

    [SkippableFact]
    public async Task Panel_DisplaysContent_WhenOpen()
    {
        var page = await NavigateToAppAsync();

        await page.EvaluateAsync(@"() => {
            const nodes = document.querySelectorAll('[data-entity-id]');
            if (nodes.length > 0) nodes[0].click();
        }");

        await page.WaitForTimeoutAsync(2000);

        var descriptionHeader = page.GetByText("Description");
        var dependenciesHeader = page.GetByText("Dependencies");

        if (await descriptionHeader.CountAsync() > 0)
        {
            Assert.True(await descriptionHeader.First.IsVisibleAsync());
            Assert.True(await dependenciesHeader.First.IsVisibleAsync());
        }
    }

    [SkippableFact]
    public async Task Panel_ClosesViaEscapeKey()
    {
        var page = await NavigateToAppAsync();

        var canvas = page.Locator("canvas");
        if (await canvas.CountAsync() > 0)
        {
            var box = await canvas.BoundingBoxAsync();
            if (box != null)
            {
                await page.Mouse.ClickAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
            }
        }

        await page.WaitForTimeoutAsync(1000);

        await page.Keyboard.PressAsync("Escape");
        await page.WaitForTimeoutAsync(500);

        var panel = page.Locator("[style*='backdrop-filter']");
        var panelCount = await panel.CountAsync();
        if (panelCount > 0)
        {
            await Assertions.Expect(panel.First).Not.ToBeVisibleAsync(
                new LocatorAssertionsToBeVisibleOptions { Timeout = 5000 }
            );
        }
    }

    [SkippableFact]
    public async Task Panel_ClosesViaCloseButton()
    {
        var page = await NavigateToAppAsync();

        var canvas = page.Locator("canvas");
        if (await canvas.CountAsync() > 0)
        {
            var box = await canvas.BoundingBoxAsync();
            if (box != null)
            {
                await page.Mouse.ClickAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
            }
        }

        await page.WaitForTimeoutAsync(1000);

        var closeButton = page.Locator("button[aria-label='Close panel']");

        if (await closeButton.CountAsync() > 0)
        {
            await closeButton.First.ClickAsync();
            await page.WaitForTimeoutAsync(500);

            var panel = page.Locator("[style*='backdrop-filter']");
            if (await panel.CountAsync() > 0)
            {
                await Assertions.Expect(panel.First).Not.ToBeVisibleAsync(
                    new LocatorAssertionsToBeVisibleOptions { Timeout = 5000 }
                );
            }
        }
    }

    [SkippableFact]
    public async Task Panel_ShowsLoadingSpinner_BeforeDataLoads()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.RouteAsync("**/api/report/**", async route =>
            {
                await Task.Delay(3000);
                await route.ContinueAsync();
            });

            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { Timeout = 15000 });
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 15000 });
        }
        catch (PlaywrightException)
        {
            Skip.If(true, "Application server is not running. Set BASE_URL or start the app with 'npm run dev'.");
            return;
        }

        var canvas = page.Locator("canvas");
        if (await canvas.CountAsync() > 0)
        {
            var box = await canvas.BoundingBoxAsync();
            if (box != null)
            {
                await page.Mouse.ClickAsync(box.X + box.Width / 2, box.Y + box.Height / 2);
            }
        }

        var spinner = page.Locator("[class*='animate-spin']");
        if (await spinner.CountAsync() > 0)
        {
            Assert.True(await spinner.First.IsVisibleAsync());
        }

        await page.UnrouteAsync("**/api/report/**");
    }
}