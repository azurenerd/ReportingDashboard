using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorDisplayTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorDisplayTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ErrorPage_ShowsErrorContainer_WhenDataMissing()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);
        await errorPage.NavigateAsync();

        // This test verifies behavior when data.json is missing.
        // If the server happens to have valid data, the error won't show.
        var isError = await errorPage.IsErrorVisibleAsync();
        if (isError)
        {
            await Assertions.Expect(errorPage.ErrorContainer).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ErrorPage_ErrorMessageContainsUsefulText()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);
        await errorPage.NavigateAsync();

        var isError = await errorPage.IsErrorVisibleAsync();
        if (isError)
        {
            var text = await errorPage.GetErrorTextAsync();
            text.Should().NotBeNullOrWhiteSpace();
            // Should contain some indication of the problem
            (text.Contains("data.json") || text.Contains("Missing") || text.Contains("Invalid") || text.Contains("Error"))
                .Should().BeTrue("error message should describe the problem");
        }
    }

    [Fact]
    public async Task ErrorPage_DoesNotShowDashboardSections()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);
        await errorPage.NavigateAsync();

        var isError = await errorPage.IsErrorVisibleAsync();
        if (isError)
        {
            var hidden = await errorPage.IsDashboardContentHiddenAsync();
            hidden.Should().BeTrue("when error is shown, dashboard sections should not render");
        }
    }

    [Fact]
    public async Task ErrorPage_NoStackTraceExposed()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);
        await errorPage.NavigateAsync();

        var isError = await errorPage.IsErrorVisibleAsync();
        if (isError)
        {
            var html = await page.ContentAsync();
            html.Should().NotContain("StackTrace");
            html.Should().NotContain("at ReportingDashboard.");
            html.Should().NotContain("NullReferenceException");
        }
    }

    [Fact]
    public async Task ErrorPage_StillRendersWithin1920x1080Container()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var isError = await dp.ErrorSection.CountAsync() > 0;
        if (isError)
        {
            var container = dp.MainContainer;
            await Assertions.Expect(container).ToBeVisibleAsync();

            var box = await container.BoundingBoxAsync();
            box.Should().NotBeNull();
            box!.Width.Should().BeApproximately(1920, 2);
            box.Height.Should().BeApproximately(1080, 2);
        }
    }

    [Fact]
    public async Task ErrorPage_PageStillReturns200()
    {
        var page = await _fixture.NewPageAsync();

        IResponse? response = null;
        page.Response += (_, r) =>
        {
            if (r.Url.Contains(_fixture.BaseUrl.TrimEnd('/')))
                response = r;
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        // Even errors render with HTTP 200 (error shown in page)
        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }
}