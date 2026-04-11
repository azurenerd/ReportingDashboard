using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    // ── Header Visibility ──

    [Fact]
    public async Task Header_IsVisible_OnDashboardLoad()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();
            var isVisible = await headerPage.IsHeaderVisibleAsync();
            isVisible.Should().BeTrue("the header should be visible when the dashboard loads");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_IsVisible_OnDashboardLoad));
            throw;
        }
    }

    // ── Title ──

    [Fact]
    public async Task Header_Title_IsDisplayedInH1()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();
            await Assertions.Expect(headerPage.Title).ToBeVisibleAsync();

            var titleText = await headerPage.GetTitleTextAsync();
            titleText.Should().NotBeNullOrWhiteSpace("the title should contain text from data.json");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_IsDisplayedInH1));
            throw;
        }
    }

    [Fact]
    public async Task Header_Title_HasCorrectFontStyling()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var fontSize = await headerPage.Title.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            fontSize.Should().Be("24px", "title font-size should be 24px");

            var fontWeight = await headerPage.Title.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            fontWeight.Should().Be("700", "title font-weight should be 700 (bold)");

            var color = await headerPage.Title.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            color.Should().Be("rgb(17, 17, 17)", "title color should be #111");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_HasCorrectFontStyling));
            throw;
        }
    }

    // ── Backlog Link ──

    [Fact]
    public async Task Header_BacklogLink_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var linkCount = await headerPage.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                await Assertions.Expect(headerPage.BacklogLink).ToBeVisibleAsync();
                var text = await headerPage.BacklogLink.TextContentAsync();
                text.Should().Contain("ADO Backlog");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_IsVisible));
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLink_OpensInNewTab()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var linkCount = await headerPage.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var target = await headerPage.GetBacklogLinkTargetAsync();
                target.Should().Be("_blank", "backlog link should open in new tab");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_OpensInNewTab));
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLink_HasMicrosoftBlueColor()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var linkCount = await headerPage.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var color = await headerPage.BacklogLink.EvaluateAsync<string>(
                    "el => getComputedStyle(el).color");
                color.Should().Be("rgb(0, 120, 212)", "link color should be #0078D4 (Microsoft blue)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasMicrosoftBlueColor));
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLink_HasNoUnderline()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var linkCount = await headerPage.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var textDecoration = await headerPage.BacklogLink.EvaluateAsync<string>(
                    "el => getComputedStyle(el).textDecorationLine");
                textDecoration.Should().Be("none", "link should have no underline");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasNoUnderline));
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLink_HasSecurityRelAttribute()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var linkCount = await headerPage.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var rel = await headerPage.BacklogLink.GetAttributeAsync("rel");
                rel.Should().Contain("noopener", "link should have noopener for security");
                rel.Should().Contain("noreferrer", "link should have noreferrer for security");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasSecurityRelAttribute));
            throw;
        }
    }

    // ── Subtitle ──

    [Fact]
    public async Task Header_Subtitle_IsDisplayed()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();
            await Assertions.Expect(headerPage.Subtitle).ToBeVisibleAsync();

            var subtitleText = await headerPage.GetSubtitleTextAsync();
            subtitleText.Should().NotBeNullOrWhiteSpace("subtitle should contain data-driven text");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_IsDisplayed));
            throw;
        }
    }

    [Fact]
    public async Task Header_Subtitle_HasCorrectStyling()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var fontSize = await headerPage.Subtitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            fontSize.Should().Be("12px", "subtitle font-size should be 12px");

            var color = await headerPage.Subtitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            color.Should().Be("rgb(136, 136, 136)", "subtitle color should be #888");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_HasCorrectStyling));
            throw;
        }
    }

    [Fact]
    public async Task Header_Subtitle_HasSubCssClass()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var classAttr = await headerPage.Subtitle.GetAttributeAsync("class");
            classAttr.Should().Contain("sub");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_HasSubCssClass));
            throw;
        }
    }

    // ── Legend ──

    [Fact]
    public async Task Header_Legend_HasFourItems()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var count = await headerPage.GetLegendItemCountAsync();
            count.Should().Be(4, "legend should display exactly four items");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_HasFourItems));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_DisplaysPoCMilestone()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var pocItem = headerPage.GetLegendItemByText("PoC Milestone");
            await Assertions.Expect(pocItem.First).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_DisplaysPoCMilestone));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_DisplaysProductionRelease()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var prodItem = headerPage.GetLegendItemByText("Production Release");
            await Assertions.Expect(prodItem.First).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_DisplaysProductionRelease));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_DisplaysCheckpoint()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var checkpointItem = headerPage.GetLegendItemByText("Checkpoint");
            await Assertions.Expect(checkpointItem.First).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_DisplaysCheckpoint));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_DisplaysNowLine()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var nowItem = headerPage.GetLegendItemByText("Now");
            await Assertions.Expect(nowItem.First).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_DisplaysNowLine));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_PoCDiamondHasGoldColor()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            // First diamond shape in the legend (PoC)
            var pocDiamond = page.Locator("div.hdr > div:last-child > span:nth-child(1) > span:first-child");
            var bgColor = await pocDiamond.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bgColor.Should().Be("rgb(244, 180, 0)", "PoC diamond should be gold #F4B400");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_PoCDiamondHasGoldColor));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_ProductionDiamondHasGreenColor()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            // Second diamond shape (Production Release)
            var prodDiamond = page.Locator("div.hdr > div:last-child > span:nth-child(2) > span:first-child");
            var bgColor = await prodDiamond.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bgColor.Should().Be("rgb(52, 168, 83)", "Production diamond should be green #34A853");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ProductionDiamondHasGreenColor));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_CheckpointHasGrayCircle()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            // Third legend item symbol (Checkpoint)
            var checkpoint = page.Locator("div.hdr > div:last-child > span:nth-child(3) > span:first-child");
            var bgColor = await checkpoint.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bgColor.Should().Be("rgb(153, 153, 153)", "Checkpoint circle should be gray #999");

            var borderRadius = await checkpoint.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderRadius");
            borderRadius.Should().Be("50%", "Checkpoint should be circular");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_CheckpointHasGrayCircle));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_NowBarHasRedColor()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            // Fourth legend item symbol (Now bar)
            var nowBar = page.Locator("div.hdr > div:last-child > span:nth-child(4) > span:first-child");
            var bgColor = await nowBar.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bgColor.Should().Be("rgb(234, 67, 53)", "Now bar should be red #EA4335");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_NowBarHasRedColor));
            throw;
        }
    }

    [Fact]
    public async Task Header_Legend_ContainerHas22pxGap()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var gap = await headerPage.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).gap");
            gap.Should().Be("22px", "legend items should have 22px gap");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainerHas22pxGap));
            throw;
        }
    }

    // ── Header Layout ──

    [Fact]
    public async Task Header_HasFlexboxLayout()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var display = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            display.Should().Be("flex", "header should use flexbox layout");

            var justifyContent = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");
            justifyContent.Should().Be("space-between", "header should space elements apart");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasFlexboxLayout));
            throw;
        }
    }

    [Fact]
    public async Task Header_HasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var borderBottom = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            borderBottom.Should().Be("solid", "header should have a solid bottom border");

            var borderColor = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomColor");
            borderColor.Should().Be("rgb(224, 224, 224)", "header border should be #E0E0E0");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasBottomBorder));
            throw;
        }
    }

    [Fact]
    public async Task Header_HasCorrectPadding()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var paddingTop = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingTop");
            paddingTop.Should().Be("12px");

            var paddingRight = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingRight");
            paddingRight.Should().Be("44px");

            var paddingBottom = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingBottom");
            paddingBottom.Should().Be("10px");

            var paddingLeft = await headerPage.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingLeft");
            paddingLeft.Should().Be("44px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasCorrectPadding));
            throw;
        }
    }
}