#nullable enable

using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.UITests.Components;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class TimelineComponentTests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineComponentTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TimelineChart_RendersSvgWithMilestoneShapesAndGridlines()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");
        await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        var timelineSvg = await page.QuerySelectorAsync(".timeline-svg");
        timelineSvg.Should().NotBeNull("SVG timeline should be rendered");

        var svgContent = await page.InnerHTMLAsync(".timeline-svg");
        svgContent.Should().Contain("line", "Should have gridlines");
        svgContent.Should().Contain("polygon", "Should have milestone diamonds");
    }
}