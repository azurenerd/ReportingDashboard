using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class HeaderComponentTests : TestContext
{
    [Fact]
    public void RendersTitle_And_Subtitle()
    {
        var project = new ProjectInfo
        {
            Title = "Test Dashboard",
            Subtitle = "Team X - Q2 2026"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        cut.Find("h1").TextContent.Should().Contain("Test Dashboard");
        cut.Find(".sub").TextContent.Should().Contain("Team X - Q2 2026");
    }

    [Fact]
    public void RendersBacklogLink_WhenUrlProvided()
    {
        var project = new ProjectInfo
        {
            Title = "Project",
            BacklogUrl = "https://dev.azure.com/backlog"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        var link = cut.Find("a.backlog-link");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/backlog");
        link.GetAttribute("target").Should().Be("_blank");
    }

    [Fact]
    public void OmitsBacklogLink_WhenUrlNull()
    {
        var project = new ProjectInfo { Title = "No Link Project" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        cut.FindAll("a.backlog-link").Should().BeEmpty();
    }

    [Fact]
    public void RendersFourLegendItems()
    {
        var project = new ProjectInfo { Title = "Legend Test" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        var legendItems = cut.FindAll(".legend-item");
        legendItems.Should().HaveCount(4);
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now");
    }
}