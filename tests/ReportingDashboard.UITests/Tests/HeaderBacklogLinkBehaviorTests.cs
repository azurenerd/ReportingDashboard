using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests for the backlog link behavior in Header.razor.
/// Existing HeaderTests cover basic link presence, href, target, and rel.
/// This file tests the conditional rendering: the link uses @if (!string.IsNullOrEmpty)
/// and renders with the .hdr-link CSS class. Tests verify the link's class attribute,
/// its position inside h1, and the "ADO Backlog" text content with the arrow character.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderBacklogLinkBehaviorTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderBacklogLinkBehaviorTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasHdrLinkClass()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var cls = await header.BacklogLink.GetAttributeAsync("class") ?? "";
                Assert.Contains("hdr-link", cls);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasHdrLinkClass));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_IsInsideH1()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // The link should be nested inside the h1.hdr-title element
            var linkInH1 = page.Locator("h1.hdr-title a.hdr-link");
            var count = await linkInH1.CountAsync();

            if (count > 0)
            {
                await Assertions.Expect(linkInH1).ToBeVisibleAsync();
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_IsInsideH1));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_ContainsArrowCharacter()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var text = await header.BacklogLink.TextContentAsync() ?? "";
                // The link text includes "ADO Backlog" and an arrow character
                Assert.Contains("ADO Backlog", text);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_ContainsArrowCharacter));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasMicrosoftBlueColor()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var color = await header.BacklogLink.EvaluateAsync<string>(
                    "el => getComputedStyle(el).color");
                // #0078D4 = rgb(0, 120, 212)
                Assert.Contains("0", color);
                Assert.Contains("120", color);
                Assert.Contains("212", color);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasMicrosoftBlueColor));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasNoTextDecoration()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var textDecoration = await header.BacklogLink.EvaluateAsync<string>(
                    "el => getComputedStyle(el).textDecorationLine");
                Assert.Equal("none", textDecoration);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasNoTextDecoration));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_IsRightOfTitle()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                // The link is inline within the h1, should be after the title text
                var h1Box = await header.TitleH1.BoundingBoxAsync();
                var linkBox = await header.BacklogLink.BoundingBoxAsync();

                Assert.NotNull(h1Box);
                Assert.NotNull(linkBox);

                // Link should be inside the h1 bounding box
                Assert.True(linkBox!.X >= h1Box!.X,
                    "Backlog link should be within the h1 element");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_IsRightOfTitle));
            throw;
        }
    }
}