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
    public async Task ErrorState_ErrorSectionHasCenteredLayout()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateToAsync("/");

        if (await errorPage.IsErrorVisibleAsync())
        {
            var display = await errorPage.ErrorContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            var justifyContent = await errorPage.ErrorContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");
            var alignItems = await errorPage.ErrorContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).alignItems");

            display.Should().Be("flex");
            justifyContent.Should().Be("center");
            alignItems.Should().Be("center");
        }
    }

    [Fact]
    public async Task ErrorState_ErrorContainsHeading()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateToAsync("/");

        if (await errorPage.IsErrorVisibleAsync())
        {
            await Assertions.Expect(errorPage.ErrorHeading).ToBeVisibleAsync();
            var heading = await errorPage.ErrorHeading.TextContentAsync();
            heading.Should().Contain("Dashboard Error");
        }
    }

    [Fact]
    public async Task ErrorState_ErrorContainsDescriptiveMessage()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateToAsync("/");

        if (await errorPage.IsErrorVisibleAsync())
        {
            var message = await errorPage.GetErrorTextAsync();
            message.Should().NotBeNullOrWhiteSpace();
            // Should contain either file-not-found, parse error, or validation error
            var hasDescriptiveMessage = message.Contains("data.json") ||
                                        message.Contains("Invalid JSON") ||
                                        message.Contains("Missing required field") ||
                                        message.Contains("deserialized to null");
            hasDescriptiveMessage.Should().BeTrue("Error message should be descriptive, not a raw exception");
        }
    }

    [Fact]
    public async Task ErrorState_NoStackTraceVisible()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateToAsync("/");

        if (await errorPage.IsErrorVisibleAsync())
        {
            var html = await page.ContentAsync();
            html.Should().NotContain("System.Exception");
            html.Should().NotContain("StackTrace");
            html.Should().NotContain("at ReportingDashboard.");
            html.Should().NotContain("NullReferenceException");
        }
    }

    [Fact]
    public async Task ErrorState_NoDataSectionsRendered()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateToAsync("/");

        if (await errorPage.IsErrorVisibleAsync())
        {
            var hdrCount = await page.Locator(".hdr").CountAsync();
            var tlCount = await page.Locator(".tl-area").CountAsync();
            var hmCount = await page.Locator(".hm-wrap").CountAsync();

            hdrCount.Should().Be(0);
            tlCount.Should().Be(0);
            hmCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task ErrorState_NoBlazorErrorBoundary()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPage(page, _fixture.BaseUrl);

        await errorPage.NavigateToAsync("/");

        var html = await page.ContentAsync();
        html.Should().NotContain("blazor-error-boundary");
        html.Should().NotContain("blazor-error-ui");
    }

    [Fact]
    public async Task ErrorState_PageStillReturns200()
    {
        var page = await _fixture.NewPageAsync();
        IResponse? mainResponse = null;

        page.Response += (_, r) =>
        {
            if (r.Url.TrimEnd('/') == _fixture.BaseUrl.TrimEnd('/') || r.Url == _fixture.BaseUrl + "/")
                mainResponse = r;
        };

        var errorPage = new ErrorPage(page, _fixture.BaseUrl);
        await errorPage.NavigateToAsync("/");

        if (mainResponse is not null)
        {
            mainResponse.Status.Should().Be(200);
        }
    }
}