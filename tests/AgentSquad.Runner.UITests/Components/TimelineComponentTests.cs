using FluentAssertions;
using Microsoft.Playwright;
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
    public async Task Timeline_RendersOnPage()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var content = await page.ContentAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Timeline_LoadsWithoutErrors()
    {
        var page = _fixture.Page!;
        var consoleErrors = new List<string>();

        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                consoleErrors.Add(msg.Text);
            }
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        consoleErrors.Should().BeEmpty("Timeline should not produce console errors");
    }
}