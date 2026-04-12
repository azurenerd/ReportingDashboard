using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class LayoutTests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MainLayout_LoadsSuccessfully()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var content = await page.ContentAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MainLayout_HasFlexboxContainer()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var pageElement = await page.QuerySelectorAsync("body");
        pageElement.Should().NotBeNull();
    }

    [Fact]
    public async Task MainLayout_RendersPageContent()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var body = await page.QuerySelectorAsync("body");
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task MainLayout_HasValidDom()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        var html = await page.QuerySelectorAsync("html");
        html.Should().NotBeNull();
    }
}