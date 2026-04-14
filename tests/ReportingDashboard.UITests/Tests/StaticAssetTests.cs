using FluentAssertions;
using ReportingDashboard.UITests.Infrastructure;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection(PlaywrightCollection.Name)]
public class StaticAssetTests
{
    private readonly PlaywrightFixture _fixture;

    public StaticAssetTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires running server")]
    public async Task CssDashboard_IsAccessible()
    {
        var page = await _fixture.CreatePageAsync();
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact(Skip = "Requires running server")]
    public async Task CssDashboard_ContainsRequiredClasses()
    {
        var page = await _fixture.CreatePageAsync();
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
        var content = await response!.TextAsync();

        content.Should().Contain(".dashboard");
        content.Should().Contain(".error-panel");
        content.Should().Contain(".hdr");
        content.Should().Contain(".tl-area");
        content.Should().Contain(".hm-wrap");
        content.Should().Contain(".hm-grid");
    }

    [Fact(Skip = "Requires running server")]
    public async Task BlazorServerJs_IsAccessible()
    {
        var page = await _fixture.CreatePageAsync();
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/_framework/blazor.server.js");

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }
}