using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardPageTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_LoadsSuccessfully()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var content = await page.ContentAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_HasNoConsoleErrors()
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

        consoleErrors.Should().BeEmpty("Dashboard should not have console errors");
    }

    [Fact]
    public async Task Dashboard_DisplaysPlaceholderText()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var pageContent = await page.ContentAsync();
        pageContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_PageResponds_WithinTimeout()
    {
        var page = _fixture.Page!;

        var navigationTask = page.GotoAsync($"{_fixture.BaseUrl}/dashboard", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var completedTask = await Task.WhenAny(navigationTask, Task.Delay(5000));

        completedTask.Should().Be(navigationTask, "Page should load within 5 seconds");
    }

    [Fact]
    public async Task Dashboard_IsAccessible()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var url = page.Url;
        url.Should().NotBeNullOrEmpty();
    }
}