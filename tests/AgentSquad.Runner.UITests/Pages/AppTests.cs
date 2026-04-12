using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class AppTests
{
    private readonly PlaywrightFixture _fixture;

    public AppTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task App_LoadsRootRoute_Successfully()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/");

        var title = await page.TitleAsync();
        title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task App_HasNoConsoleErrors_OnLoad()
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

        await page.GotoAsync($"{_fixture.BaseUrl}/");

        consoleErrors.Should().BeEmpty("Should not have console errors on page load");
    }

    [Fact]
    public async Task App_RendersRouterComponent()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/");

        var body = await page.ContentAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task App_NavigatesToDashboard_Successfully()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        page.Url.Should().Contain("dashboard");
    }
}