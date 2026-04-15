using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

public class DashboardComponentTests : TestContext
{
    private static DashboardData CreateTestData() => new()
    {
        SchemaVersion = 1,
        Title = "Test Dashboard",
        Subtitle = "Test Team · April 2026",
        BacklogUrl = "https://example.com/backlog",
        NowDateOverride = "2026-04-15",
        Timeline = new TimelineConfig
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            Workstreams = new[]
            {
                new Workstream
                {
                    Id = "M1",
                    Name = "Test Workstream",
                    Color = "#0078D4",
                    Milestones = new[]
                    {
                        new Milestone { Label = "Jan Start", Date = "2026-01-15", Type = "start" },
                        new Milestone { Label = "Mar PoC", Date = "2026-03-20", Type = "poc" }
                    }
                }
            }
        },
        Heatmap = new HeatmapConfig
        {
            MonthColumns = new[] { "Jan", "Feb", "Mar", "Apr" },
            Categories = new[]
            {
                new StatusCategory
                {
                    Name = "Shipped", Emoji = "✅", CssClass = "ship",
                    Months = new[]
                    {
                        new MonthItems { Month = "Jan", Items = new[] { "Item A", "Item B" } },
                        new MonthItems { Month = "Feb", Items = new[] { "Item C" } },
                        new MonthItems { Month = "Mar", Items = Array.Empty<string>() },
                        new MonthItems { Month = "Apr", Items = new[] { "Item D" } }
                    }
                },
                new StatusCategory
                {
                    Name = "In Progress", Emoji = "🔵", CssClass = "prog",
                    Months = new[]
                    {
                        new MonthItems { Month = "Jan", Items = new[] { "WIP 1" } },
                        new MonthItems { Month = "Feb", Items = Array.Empty<string>() },
                        new MonthItems { Month = "Mar", Items = new[] { "WIP 2" } },
                        new MonthItems { Month = "Apr", Items = new[] { "WIP 3", "WIP 4" } }
                    }
                },
                new StatusCategory
                {
                    Name = "Carryover", Emoji = "🟡", CssClass = "carry",
                    Months = new[]
                    {
                        new MonthItems { Month = "Jan", Items = Array.Empty<string>() },
                        new MonthItems { Month = "Feb", Items = new[] { "Carry 1" } },
                        new MonthItems { Month = "Mar", Items = Array.Empty<string>() },
                        new MonthItems { Month = "Apr", Items = Array.Empty<string>() }
                    }
                },
                new StatusCategory
                {
                    Name = "Blockers", Emoji = "🔴", CssClass = "block",
                    Months = new[]
                    {
                        new MonthItems { Month = "Jan", Items = Array.Empty<string>() },
                        new MonthItems { Month = "Feb", Items = Array.Empty<string>() },
                        new MonthItems { Month = "Mar", Items = new[] { "Blocker X" } },
                        new MonthItems { Month = "Apr", Items = new[] { "Blocker Y" } }
                    }
                }
            }
        }
    };

    private DataService SetupService(DashboardData? data = null, string? errorOverride = null)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "RD_Test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        if (data != null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            File.WriteAllText(Path.Combine(tempDir, "data.json"), json);
        }

        var env = new TestHostEnv(tempDir);
        var svc = new DataService(env);

        if (errorOverride != null)
        {
            var field = typeof(DataService).GetField("_currentError",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(svc, errorOverride);
        }

        Services.AddSingleton(svc);
        return svc;
    }

    [Fact]
    public void Renders_Title()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.Contains("Test Dashboard", cut.Find("h1").TextContent);
    }

    [Fact]
    public void Renders_BacklogLink()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        var link = cut.Find("h1 a");
        Assert.Equal("https://example.com/backlog", link.GetAttribute("href"));
    }

    [Fact]
    public void Renders_Subtitle()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.Contains("Test Team", cut.Find(".sub").TextContent);
    }

    [Fact]
    public void Renders_Legend_FourItems()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.Equal(4, cut.FindAll(".legend-item").Count);
    }

    [Fact]
    public void Renders_WorkstreamLabels()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        var ids = cut.FindAll(".tl-ws-id");
        Assert.Single(ids);
        Assert.Equal("M1", ids[0].TextContent);
    }

    [Fact]
    public void Renders_Svg()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.NotNull(cut.Find("svg"));
    }

    [Fact]
    public void Renders_HeatmapTitle()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.Contains("Monthly Execution Heatmap", cut.Find(".hm-title").TextContent);
    }

    [Fact]
    public void Renders_MonthHeaders()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        var hdrs = cut.FindAll(".hm-col-hdr");
        Assert.Equal(4, hdrs.Count);
        Assert.Contains("Jan", hdrs[0].TextContent);
        Assert.Contains("Apr", hdrs[3].TextContent);
    }

    [Fact]
    public void CurrentMonth_HasNowClass()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        var nowHdrs = cut.FindAll(".hm-col-hdr.now-hdr");
        Assert.Single(nowHdrs);
        Assert.Contains("Apr", nowHdrs[0].TextContent);
        Assert.Contains("Now", nowHdrs[0].TextContent);
    }

    [Fact]
    public void Renders_StatusRowHeaders()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.Single(cut.FindAll(".ship-hdr"));
        Assert.Single(cut.FindAll(".prog-hdr"));
        Assert.Single(cut.FindAll(".carry-hdr"));
        Assert.Single(cut.FindAll(".block-hdr"));
    }

    [Fact]
    public void Renders_16_DataCells()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.Equal(16, cut.FindAll(".hm-cell").Count);
    }

    [Fact]
    public void EmptyCells_ShowDash()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        var empties = cut.FindAll(".it.empty");
        Assert.True(empties.Count > 0);
        foreach (var e in empties)
            Assert.Equal("-", e.TextContent);
    }

    [Fact]
    public void CurrentMonthCells_HaveNowClass()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        Assert.Equal(4, cut.FindAll(".hm-cell.now").Count);
    }

    [Fact]
    public void GridColumns_MatchMonthCount()
    {
        SetupService(CreateTestData());
        var cut = RenderComponent<Dashboard>();
        var style = cut.Find(".hm-grid").GetAttribute("style");
        Assert.Contains("repeat(4, 1fr)", style);
    }

    [Fact]
    public void NoData_ShowsError()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "RD_Empty_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var env = new TestHostEnv(tempDir);
        var svc = new DataService(env);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        Assert.NotNull(cut.Find(".error-page"));
    }

    [Fact]
    public void DataWithError_ShowsBanner()
    {
        SetupService(CreateTestData(), "Parse error in data.json");
        var cut = RenderComponent<Dashboard>();
        var banner = cut.Find(".error-banner");
        Assert.Contains("Parse error in data.json", banner.TextContent);
        Assert.NotNull(cut.Find(".hdr"));
    }
}

public class TestHostEnv : IWebHostEnvironment
{
    public TestHostEnv(string root)
    {
        WebRootPath = root;
        ContentRootPath = root;
        EnvironmentName = "Testing";
        ApplicationName = "ReportingDashboard.Tests";
        ContentRootFileProvider = new NullFileProvider();
        WebRootFileProvider = new NullFileProvider();
    }

    public string WebRootPath { get; set; }
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }
}