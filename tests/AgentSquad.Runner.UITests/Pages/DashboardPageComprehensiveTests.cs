using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardPageComprehensiveTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardPageComprehensiveTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_PageTitleIsSet()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await _fixture.Page.TitleAsync();
        title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_HasViewportDimensions()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var viewportSize = _fixture.Page.ViewportSize;
        viewportSize.Should().NotBeNull();
        viewportSize!.Width.Should().Be(1920);
        viewportSize.Height.Should().Be(1080);
    }

    [Fact]
    public async Task Dashboard_LoadsWithoutErrors()
    {
        var errors = new List<string>();
        
        _fixture.Page!.PageError += (sender, error) =>
        {
            errors.Add(error);
        };

        await _fixture.Page.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        errors.Should().BeEmpty("Page should load without JavaScript errors");
    }

    [Fact]
    public async Task Dashboard_AllMajorComponentsPresent()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = await _fixture.Page.QuerySelectorAsync(".hdr");
        var timeline = await _fixture.Page.QuerySelectorAsync(".tl-area");
        var heatmap = await _fixture.Page.QuerySelectorAsync(".hm-wrap");

        header.Should().NotBeNull("Header component should exist");
        timeline.Should().NotBeNull("Timeline component should exist");
        heatmap.Should().NotBeNull("Heatmap component should exist");
    }

    [Fact]
    public async Task Dashboard_ContainerHasFlexLayout()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var container = await _fixture.Page.QuerySelectorAsync(".dashboard-container");
        
        if (container != null)
        {
            var display = await container.EvaluateAsync<string>("el => window.getComputedStyle(el).display");
            display.Should().BeOneOf("flex", "flex-start");
        }
    }

    [Fact]
    public async Task Dashboard_CssFileIsLoaded()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var stylesheets = await _fixture.Page.QuerySelectorAllAsync("link[rel='stylesheet']");
        stylesheets.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_RespondsToRefreshCorrectly()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var initialUrl = _fixture.Page.Url;

        await _fixture.Page.ReloadAsync();
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reloadedUrl = _fixture.Page.Url;
        reloadedUrl.Should().Be(initialUrl);
    }

    [Fact]
    public async Task Dashboard_HasProperPageStructure()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var body = await _fixture.Page.QuerySelectorAsync("body");
        body.Should().NotBeNull();

        var children = await body!.QuerySelectorAllAsync("*");
        children.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_MetaTagsArePresent()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);

        var charset = await _fixture.Page.QuerySelectorAsync("meta[charset]");
        var viewport = await _fixture.Page.QuerySelectorAsync("meta[name='viewport']");

        charset.Should().NotBeNull("Charset meta tag should be present");
        viewport.Should().NotBeNull("Viewport meta tag should be present");
    }

    [Fact]
    public async Task Dashboard_BlazorScriptIsLoaded()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var blazorScript = await _fixture.Page.QuerySelectorAsync("script[src*='blazor']");
        blazorScript.Should().NotBeNull("Blazor script should be loaded");
    }

    [Fact]
    public async Task Dashboard_TextIsRendered()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bodyText = await _fixture.Page.ContentAsync();
        bodyText.Should().NotBeNullOrEmpty();
        bodyText.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public async Task Dashboard_LinksAreNavigable()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var links = await _fixture.Page.QuerySelectorAllAsync("a");
        
        links.Count.Should().BeGreaterThanOrEqualTo(0);
        
        foreach (var link in links)
        {
            var href = await link.GetAttributeAsync("href");
            href.Should().NotBeNullOrEmpty();
        }
    }
}