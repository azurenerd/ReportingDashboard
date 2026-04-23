using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class HeaderComponentTests : TestContext
{
    [Fact]
    public void RendersProjectTitle_InH1Element()
    {
        var project = new ProjectInfo { Title = "Test Dashboard", Subtitle = "Test Sub", CurrentDate = "2026-04-15" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain("Test Dashboard");
    }

    [Fact]
    public void RendersSubtitle_InSubDiv()
    {
        var project = new ProjectInfo { Title = "T", Subtitle = "My Subtitle Text", CurrentDate = "2026-04-15" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        var sub = cut.Find(".sub");
        sub.TextContent.Should().Contain("My Subtitle Text");
    }

    [Fact]
    public void RendersBacklogLink_WhenUrlProvided()
    {
        var project = new ProjectInfo { Title = "T", Subtitle = "S", BacklogUrl = "https://ado.example.com", CurrentDate = "2026-04-15" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        var link = cut.Find("a[target='_blank']");
        link.GetAttribute("href").Should().Be("https://ado.example.com");
    }

    [Fact]
    public void OmitsBacklogLink_WhenUrlIsNull()
    {
        var project = new ProjectInfo { Title = "T", Subtitle = "S", BacklogUrl = null, CurrentDate = "2026-04-15" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        cut.FindAll("a[target='_blank']").Should().BeEmpty();
    }

    [Fact]
    public void RendersFourLegendItems()
    {
        var project = new ProjectInfo { Title = "T", Subtitle = "S", CurrentDate = "2026-04-15" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        var legendItems = cut.FindAll(".legend-item");
        legendItems.Should().HaveCount(4);
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (");
    }
}