using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardPageTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardPageTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task HomePage_LoadsSuccessfully_Returns200()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        IResponse? response = null;
        page.Response += (_, r) =>
        {
            if (r.Url.TrimEnd('/') == _fixture.BaseUrl.TrimEnd('/') || r.Url == _fixture.BaseUrl + "/")
                response = r;
        };

        await dashboardPage.NavigateAsync();

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task HomePage_HasCorrectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var title = await dashboardPage.GetTitleAsync();
        title.Should().Contain("Executive Reporting Dashboard");
    }

    [Fact]
    public async Task HomePage_Has1920x1080Container()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var container = dashboardPage.MainContainer;
        await Assertions.Expect(container).ToBeVisibleAsync();

        var box = await container.BoundingBoxAsync();
        box.Should().NotBeNull();
        box!.Width.Should().BeApproximately(1920, 2);
        box.Height.Should().BeApproximately(1080, 2);
    }

    [Fact]
    public async Task HomePage_ContainerHasFlexColumnLayout()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var container = dashboardPage.MainContainer;
        var display = await container.EvaluateAsync<string>("el => getComputedStyle(el).display");
        var direction = await container.EvaluateAsync<string>("el => getComputedStyle(el).flexDirection");

        display.Should().Be("flex");
        direction.Should().Be("column");
    }

    [Fact]
    public async Task HomePage_ContainerHasOverflowHidden()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var container = dashboardPage.MainContainer;
        var overflow = await container.EvaluateAsync<string>("el => getComputedStyle(el).overflow");

        overflow.Should().Be("hidden");
    }

    [Fact]
    public async Task HomePage_HasCorrectFontFamily()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var container = dashboardPage.MainContainer;
        var fontFamily = await container.EvaluateAsync<string>("el => getComputedStyle(el).fontFamily");

        fontFamily.Should().Contain("Segoe UI");
    }

    [Fact]
    public async Task HomePage_NoBlazorReconnectModalVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var hasModal = await dashboardPage.HasBlazorReconnectModalAsync();
        hasModal.Should().BeFalse();
    }

    [Fact]
    public async Task HomePage_NoBlazorServerScript()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var html = await dashboardPage.GetPageHtmlAsync();
        html.Should().NotContain("blazor.server.js");
    }

    [Fact]
    public async Task HomePage_HasViewportMetaWidth1920()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var viewport = await page.Locator("meta[name='viewport']").GetAttributeAsync("content");
        viewport.Should().Contain("width=1920");
    }

    [Fact]
    public async Task HomePage_HasCssLink()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var cssLink = page.Locator("link[rel='stylesheet'][href='css/app.css']");
        var count = await cssLink.CountAsync();
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HomePage_HasDoctype()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var doctype = await page.EvaluateAsync<string>("() => document.doctype ? new XMLSerializer().serializeToString(document.doctype) : ''");
        doctype.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HomePage_HtmlHasLangAttribute()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var lang = await page.Locator("html").GetAttributeAsync("lang");
        lang.Should().Be("en");
    }
}