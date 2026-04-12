using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

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
    public async Task Header_DisplaysProjectTitle()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = await _fixture.Page.QuerySelectorAsync("h1");
        h1.Should().NotBeNull();

        var titleText = await h1!.TextContentAsync();
        titleText.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Header_DisplaysProjectDescription()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var subElement = await _fixture.Page.QuerySelectorAsync(".sub");
        subElement.Should().NotBeNull();

        var description = await subElement!.TextContentAsync();
        description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Header_ContainsAdoBacklogLink()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var adoLink = await _fixture.Page.QuerySelectorAsync(".ado-link");
        adoLink.Should().NotBeNull();

        var href = await adoLink!.GetAttributeAsync("href");
        href.Should().NotBeNullOrEmpty();
        href.Should().Contain("dev.azure.com");
    }

    [Fact]
    public async Task Header_DisplaysLastUpdatedTime()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var lastUpdated = await _fixture.Page.QuerySelectorAsync(".last-updated");
        lastUpdated.Should().NotBeNull();

        var text = await lastUpdated!.TextContentAsync();
        text.Should().Contain("Last Updated");
    }

    [Fact]
    public async Task Header_DisplaysLegendItems()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendItems = await _fixture.Page.QuerySelectorAllAsync(".legend-item");
        
        legendItems.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task Header_LegendContainsPocMilestone()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendText = await _fixture.Page.ContentAsync();
        legendText.Should().Contain("PoC Milestone");
    }

    [Fact]
    public async Task Header_LegendContainsProductionRelease()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendText = await _fixture.Page.ContentAsync();
        legendText.Should().Contain("Production Release");
    }

    [Fact]
    public async Task Header_LegendContainsCheckpoint()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendText = await _fixture.Page.ContentAsync();
        legendText.Should().Contain("Checkpoint");
    }
}