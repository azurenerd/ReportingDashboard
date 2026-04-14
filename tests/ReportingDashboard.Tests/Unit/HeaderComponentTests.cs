using Bunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;
using FluentAssertions;

namespace ReportingDashboard.Tests.Unit;

public class HeaderComponentTests : TestContext
{
    private static DashboardData CreateTestData(
        string title = "Test Project Roadmap",
        string subtitle = "Engineering · Test Workstream · April 2026",
        string backlogUrl = "https://dev.azure.com/org/project/_backlogs") =>
        new()
        {
            SchemaVersion = 1,
            Title = title,
            Subtitle = subtitle,
            BacklogUrl = backlogUrl,
            NowDateOverride = "2026-04-14",
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
                            new Milestone { Label = "Jan 12", Date = "2026-01-12", Type = "start" }
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
                        Name = "Shipped",
                        Emoji = "✅",
                        CssClass = "ship",
                        Months = new[]
                        {
                            new MonthItems { Month = "Jan", Items = new[] { "Item A" } },
                            new MonthItems { Month = "Feb", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Mar", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Apr", Items = Array.Empty<string>() }
                        }
                    },
                    new StatusCategory
                    {
                        Name = "In Progress",
                        Emoji = "🔄",
                        CssClass = "prog",
                        Months = new[]
                        {
                            new MonthItems { Month = "Jan", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Feb", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Mar", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Apr", Items = Array.Empty<string>() }
                        }
                    },
                    new StatusCategory
                    {
                        Name = "Carryover",
                        Emoji = "🔁",
                        CssClass = "carry",
                        Months = new[]
                        {
                            new MonthItems { Month = "Jan", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Feb", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Mar", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Apr", Items = Array.Empty<string>() }
                        }
                    },
                    new StatusCategory
                    {
                        Name = "Blockers",
                        Emoji = "🚫",
                        CssClass = "block",
                        Months = new[]
                        {
                            new MonthItems { Month = "Jan", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Feb", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Mar", Items = Array.Empty<string>() },
                            new MonthItems { Month = "Apr", Items = Array.Empty<string>() }
                        }
                    }
                }
            }
        };

    private DataService CreateDataServiceWithData(string tempDir, DashboardData testData)
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

        var json = System.Text.Json.JsonSerializer.Serialize(testData, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        File.WriteAllText(Path.Combine(tempDir, "data.json"), json);

        return new DataService(mockEnv.Object);
    }

    [Fact]
    public void Header_DisplaysProjectTitle()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"hdr_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var dataService = CreateDataServiceWithData(tempDir, CreateTestData());
        Services.AddSingleton(dataService);

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find(".hdr h1");
        h1.TextContent.Should().Contain("Test Project Roadmap");

        dataService.Dispose();
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Header_DisplaysBacklogLink()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"hdr_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var dataService = CreateDataServiceWithData(tempDir, CreateTestData(backlogUrl: "https://dev.azure.com/myorg/myproject/_backlogs"));
        Services.AddSingleton(dataService);

        var cut = RenderComponent<Dashboard>();

        var link = cut.Find(".hdr h1 a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/myorg/myproject/_backlogs");
        link.TextContent.Should().Contain("ADO Backlog");

        dataService.Dispose();
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Header_DisplaysSubtitle()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"hdr_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var dataService = CreateDataServiceWithData(tempDir, CreateTestData(subtitle: "My Team · My Workstream · Q1 2026"));
        Services.AddSingleton(dataService);

        var cut = RenderComponent<Dashboard>();

        var sub = cut.Find(".hdr .sub");
        sub.TextContent.Should().Contain("My Team · My Workstream · Q1 2026");

        dataService.Dispose();
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Header_ContainsLegendWithFourItems()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"hdr_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var dataService = CreateDataServiceWithData(tempDir, CreateTestData());
        Services.AddSingleton(dataService);

        var cut = RenderComponent<Dashboard>();

        var legendItems = cut.FindAll(".legend .legend-item");
        legendItems.Count.Should().Be(4);

        cut.FindAll(".icon-diamond.gold").Count.Should().Be(1);
        cut.FindAll(".icon-diamond.green").Count.Should().Be(1);
        cut.FindAll(".icon-circle").Count.Should().Be(1);
        cut.FindAll(".icon-now-line").Count.Should().Be(1);

        dataService.Dispose();
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Header_LegendContainsCorrectLabels()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"hdr_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var dataService = CreateDataServiceWithData(tempDir, CreateTestData());
        Services.AddSingleton(dataService);

        var cut = RenderComponent<Dashboard>();

        var legendMarkup = cut.Find(".legend").TextContent;
        legendMarkup.Should().Contain("PoC Milestone");
        legendMarkup.Should().Contain("Production Release");
        legendMarkup.Should().Contain("Checkpoint");
        legendMarkup.Should().Contain("Now");

        dataService.Dispose();
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void ErrorState_ShowsErrorPageWhenNoData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"hdr_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);
        // No data.json file — triggers error state

        var dataService = new DataService(mockEnv.Object);
        Services.AddSingleton(dataService);

        var cut = RenderComponent<Dashboard>();

        var errorPage = cut.Find(".error-page");
        errorPage.Should().NotBeNull();
        cut.Find(".error-message").TextContent.Should().Contain("data.json");

        dataService.Dispose();
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Header_TitleIsDrivenFromDataJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"hdr_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var dataService = CreateDataServiceWithData(tempDir, CreateTestData(title: "Custom Dashboard Title XYZ"));
        Services.AddSingleton(dataService);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".hdr h1").TextContent.Should().Contain("Custom Dashboard Title XYZ");

        dataService.Dispose();
        Directory.Delete(tempDir, true);
    }
}