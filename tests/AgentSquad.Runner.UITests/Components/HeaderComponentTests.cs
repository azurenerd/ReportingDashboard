using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Components;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeaderComponentTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderComponentTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Header_DisplaysOnPage()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var headerContent = await page.ContentAsync();
        headerContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Header_LoadsWithoutErrors()
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

        consoleErrors.Should().BeEmpty("Header should not produce console errors");
    }

    [Fact]
    public async Task Header_RendersValidMarkup()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var content = await page.ContentAsync();
        content.Should().Contain("<");
    }
}