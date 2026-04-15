using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class HeaderComponentTests : TestContext
{
    private DashboardData CreateTestData(string? backlogUrl = "https://dev.azure.com/test") => new()
    {
        Title = "Test Project Roadmap",
        Subtitle = "Test Team \u2022 Test Workstream \u2022 April 2026",
        BacklogUrl = backlogUrl
    };

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersTitle_InH1Element()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain("Test Project Roadmap");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersBacklogLink_WhenBacklogUrlIsPresent()
    {
        var data = CreateTestData("https://dev.azure.com/myorg/backlog");

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("h1 a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/myorg/backlog");
        link.GetAttribute("target").Should().Be("_blank");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", "Unit")]
    public void Header_HidesBacklogLink_WhenBacklogUrlIsNullOrEmpty(string? backlogUrl)
    {
        var data = CreateTestData(backlogUrl);

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        var links = cut.FindAll("h1 a");
        links.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersSubtitle_WithSubClass()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        var sub = cut.Find(".sub");
        sub.TextContent.Should().Contain("Test Team");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersFourLegendItems_InCorrectOrder()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Header>(
            p => p.Add(x => x.Data, data));

        var legendItems = cut.FindAll(".hdr-legend .legend-item");
        legendItems.Should().HaveCount(4);

        var labels = cut.FindAll(".legend-label");
        labels[0].TextContent.Should().Be("PoC Milestone");
        labels[1].TextContent.Should().Be("Production Release");
        labels[2].TextContent.Should().Be("Checkpoint");
        labels[3].TextContent.Should().Be("Now");

        // Verify icon CSS classes
        cut.FindAll(".legend-poc").Should().HaveCount(1);
        cut.FindAll(".legend-prod").Should().HaveCount(1);
        cut.FindAll(".legend-checkpoint").Should().HaveCount(1);
        cut.FindAll(".legend-now").Should().HaveCount(1);
    }
}