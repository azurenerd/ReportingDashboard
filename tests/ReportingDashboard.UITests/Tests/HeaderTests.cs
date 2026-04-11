using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_DisplaysProjectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.Title).ToBeVisibleAsync();
            var titleText = await dashboard.Title.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(titleText), "Title should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_DisplaysProjectTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_DisplaysSubtitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.Subtitle).ToBeVisibleAsync();
            var subText = await dashboard.Subtitle.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(subText), "Subtitle should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_DisplaysSubtitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_IsClickable()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var linkCount = await dashboard.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                await Assertions.Expect(dashboard.BacklogLink).ToBeVisibleAsync();
                var href = await dashboard.BacklogLink.GetAttributeAsync("href");
                Assert.False(string.IsNullOrWhiteSpace(href), "Backlog link should have an href");
                var target = await dashboard.BacklogLink.GetAttributeAsync("target");
                Assert.Equal("_blank", target);
                var rel = await dashboard.BacklogLink.GetAttributeAsync("rel");
                Assert.Contains("noopener", rel ?? "");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_IsClickable));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_ContainsADOBacklogText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var linkCount = await dashboard.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var text = await dashboard.BacklogLink.TextContentAsync();
                Assert.Contains("ADO Backlog", text ?? "");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_ContainsADOBacklogText));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_HasFourItems()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.Legend).ToBeVisibleAsync();
            var count = await dashboard.LegendItems.CountAsync();
            Assert.Equal(4, count);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_HasFourItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ContainsExpectedLabels()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var legendText = await dashboard.Legend.TextContentAsync() ?? "";
            Assert.Contains("PoC Milestone", legendText);
            Assert.Contains("Production Release", legendText);
            Assert.Contains("Checkpoint", legendText);
            Assert.Contains("Now", legendText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainsExpectedLabels));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_HasCorrectSymbols()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.Equal(1, await dashboard.LegendPocDiamond.CountAsync());
            Assert.Equal(1, await dashboard.LegendProdDiamond.CountAsync());
            Assert.Equal(1, await dashboard.LegendCircle.CountAsync());
            Assert.Equal(1, await dashboard.LegendBar.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_HasCorrectSymbols));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var borderBottom = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            Assert.Equal("solid", borderBottom);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasBottomBorder));
            throw;
        }
    }
}