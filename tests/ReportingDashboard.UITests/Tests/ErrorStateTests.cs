using FluentAssertions;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection(PlaywrightCollection.Name)]
public class ErrorStateTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorStateTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires running server with missing data.json")]
    public async Task MissingDataJson_ShowsErrorBanner()
    {
        var page = await _fixture.CreatePageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateAsync();

        (await errorPage.IsErrorVisibleAsync()).Should().BeTrue();
        var text = await errorPage.GetErrorTextAsync();
        text.Should().Contain("Data Error");
    }

    [Fact(Skip = "Requires running server with malformed data.json")]
    public async Task MalformedJson_ShowsErrorBanner()
    {
        var page = await _fixture.CreatePageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateAsync();

        (await errorPage.IsErrorVisibleAsync()).Should().BeTrue();
        var text = await errorPage.GetErrorTextAsync();
        text.Should().Contain("Invalid JSON");
    }
}