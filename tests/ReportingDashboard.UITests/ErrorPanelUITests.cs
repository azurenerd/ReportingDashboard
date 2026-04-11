using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelUITests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_HasAllRequiredElements()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var errorPanel = new ErrorPanelPage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasErrorPanelAsync())
            {
                await Assertions.Expect(errorPanel.Icon).ToBeVisibleAsync();
                await Assertions.Expect(errorPanel.Title).ToBeVisibleAsync();
                await Assertions.Expect(errorPanel.Details).ToBeVisibleAsync();
                await Assertions.Expect(errorPanel.Help).ToBeVisibleAsync();
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "ErrorPanel_AllElements_Failed");
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_ShowsCorrectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var errorPanel = new ErrorPanelPage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasErrorPanelAsync())
            {
                var titleText = await errorPanel.GetTitleTextAsync();
                titleText.Should().Contain("Dashboard data could not be loaded");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "ErrorPanel_Title_Failed");
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_ShowsHelpText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var errorPanel = new ErrorPanelPage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasErrorPanelAsync())
            {
                var helpText = await errorPanel.GetHelpTextAsync();
                helpText.Should().Contain("Check data.json for errors and restart the application.");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "ErrorPanel_HelpText_Failed");
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_ShowsErrorDetails()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);
        var errorPanel = new ErrorPanelPage(page);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasErrorPanelAsync())
            {
                var detailsText = await errorPanel.GetDetailsTextAsync();
                // Error details should contain some meaningful error message
                detailsText.Should().NotBeNullOrEmpty("error details should contain the specific error");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "ErrorPanel_Details_Failed");
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_NoDashboardContentShown()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasErrorPanelAsync())
            {
                var hasDashboard = await dashboardPage.HasDashboardContentAsync();
                hasDashboard.Should().BeFalse("dashboard content should not render when error panel is shown");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "ErrorPanel_NoDashboard_Failed");
            throw;
        }
    }
}