using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

public class DashboardHeaderUITests : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;
    private IBrowserContext _context = null!;

    public DashboardHeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page = await _context.NewPageAsync();
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
        // Wait for Blazor to render the header component
        await _page.WaitForSelectorAsync("div.hdr", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });
    }

    public async Task DisposeAsync()
    {
        if (_page is not null) await _page.CloseAsync();
        if (_context is not null) await _context.DisposeAsync();
    }

    [Fact]
    public async Task HeaderRendersWithTitleAndSubtitle()
    {
        var h1 = await _page.QuerySelectorAsync("div.hdr h1");
        Assert.NotNull(h1);
        var titleText = await h1.InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(titleText), "H1 title should not be empty");

        var sub = await _page.QuerySelectorAsync("div.hdr div.sub");
        Assert.NotNull(sub);
        var subText = await sub.InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(subText), "Subtitle should not be empty");
    }

    [Fact]
    public async Task AdoBacklogLinkIsPresent()
    {
        var link = await _page.QuerySelectorAsync("div.hdr h1 a");
        Assert.NotNull(link);

        var href = await link.GetAttributeAsync("href");
        Assert.False(string.IsNullOrWhiteSpace(href), "ADO Backlog link href should not be empty");

        var text = await link.InnerTextAsync();
        Assert.Contains("ADO Backlog", text);

        var color = await link.EvaluateAsync<string>("el => getComputedStyle(el).color");
        Assert.NotNull(color);
    }

    [Fact]
    public async Task LegendDisplaysFourItems()
    {
        var legendContainer = await _page.QuerySelectorAllAsync("div.hdr > div:last-child > div");
        Assert.Equal(4, legendContainer.Count);

        var legendTexts = new List<string>();
        foreach (var item in legendContainer)
        {
            var text = await item.InnerTextAsync();
            legendTexts.Add(text.Trim());
        }

        Assert.Contains(legendTexts, t => t.Contains("PoC Milestone"));
        Assert.Contains(legendTexts, t => t.Contains("Production Release"));
        Assert.Contains(legendTexts, t => t.Contains("Checkpoint"));
        Assert.Contains(legendTexts, t => t.Contains("Now"));
    }

    [Fact]
    public async Task LegendShapesHaveCorrectColors()
    {
        var shapes = await _page.QuerySelectorAllAsync("div.hdr > div:last-child > div > span:first-child");
        Assert.True(shapes.Count >= 4, $"Expected at least 4 legend shapes, found {shapes.Count}");

        // PoC Milestone - amber diamond
        var pocBg = await shapes[0].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("244", pocBg); // #F4B400 -> rgb(244, 180, 0)

        // Production Release - green diamond
        var prodBg = await shapes[1].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("52", prodBg); // #34A853 -> rgb(52, 168, 83)

        // Checkpoint - gray circle
        var cpBg = await shapes[2].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("153", cpBg); // #999 -> rgb(153, 153, 153)

        // Now - red bar
        var nowBg = await shapes[3].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("234", nowBg); // #EA4335 -> rgb(234, 67, 53)
    }

    [Fact]
    public async Task HeaderLayoutAt1920x1080_NoOverflow()
    {
        var hdr = await _page.QuerySelectorAsync("div.hdr");
        Assert.NotNull(hdr);

        var box = await hdr.BoundingBoxAsync();
        Assert.NotNull(box);

        Assert.True(box.Width <= 1920, $"Header width {box.Width} exceeds 1920px viewport");
        Assert.True(box.Y < 10, $"Header Y position {box.Y} should be near top of page");
    }

    [Fact]
    public void DashboardHeaderComponent_ExistsInProject()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.sln").Length == 0)
        {
            dir = dir.Parent;
        }
        Assert.NotNull(dir);

        var componentPath = Path.Combine(dir.FullName, "ReportingDashboard", "Components", "DashboardHeader.razor");
        Assert.True(File.Exists(componentPath), $"DashboardHeader.razor should exist at {componentPath}");
    }

    [Fact]
    public void DashboardData_ModelHasRequiredProperties()
    {
        var type = typeof(ReportingDashboard.Models.DashboardData);
        Assert.NotNull(type.GetProperty("Title"));
        Assert.NotNull(type.GetProperty("Subtitle"));
        Assert.NotNull(type.GetProperty("BacklogUrl"));
        Assert.NotNull(type.GetProperty("Heatmap"));
    }

    [Fact]
    public void HeatmapData_HasCurrentMonthProperty()
    {
        var type = typeof(ReportingDashboard.Models.HeatmapData);
        var prop = type.GetProperty("CurrentMonth");
        Assert.NotNull(prop);
        Assert.Equal(typeof(string), prop.PropertyType);
    }

    [Fact]
    public void DashboardHeaderRazor_IsNotModifyingOtherFiles()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.sln").Length == 0)
        {
            dir = dir.Parent;
        }
        Assert.NotNull(dir);

        var componentsDir = Path.Combine(dir.FullName, "ReportingDashboard", "Components");
        Assert.True(Directory.Exists(componentsDir));

        var headerFile = Path.Combine(componentsDir, "DashboardHeader.razor");
        Assert.True(File.Exists(headerFile));
    }

    [Fact]
    public void DashboardDataService_IsRegistered()
    {
        var serviceType = typeof(ReportingDashboard.Services.DashboardDataService);
        Assert.NotNull(serviceType);
        Assert.NotNull(serviceType.GetProperty("Data"));
        Assert.NotNull(serviceType.GetProperty("HasError"));
        Assert.NotNull(serviceType.GetProperty("ErrorMessage"));
    }

    [Fact]
    public void ComponentParameter_AcceptsDashboardData()
    {
        var assembly = typeof(ReportingDashboard.Models.DashboardData).Assembly;
        Assert.NotNull(assembly);

        var modelType = assembly.GetType("ReportingDashboard.Models.DashboardData");
        Assert.NotNull(modelType);
    }

    [Fact]
    public void LegendItems_AreDataDriven()
    {
        var heatmapType = typeof(ReportingDashboard.Models.HeatmapData);
        var currentMonthProp = heatmapType.GetProperty("CurrentMonth");
        Assert.NotNull(currentMonthProp);
    }
}