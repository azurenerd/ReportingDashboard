using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeaderTests : TestContext
{
    [Fact]
    public void Header_WithAllParameters_ShouldRenderTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "My Project")
            .Add(x => x.Subtitle, "Team Alpha - Q1")
            .Add(x => x.BacklogLink, "https://ado.example.com")
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.NowDate, "2026-04-10"));

        cut.Find("h1").TextContent.Should().Contain("My Project");
    }

    [Fact]
    public void Header_WithSubtitle_ShouldRenderSubtitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.Subtitle, "Engineering - April 2026")
            .Add(x => x.NowDate, ""));

        cut.Find(".sub").TextContent.Should().Be("Engineering - April 2026");
    }

    [Fact]
    public void Header_WithBacklogLink_ShouldRenderClickableLink()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.BacklogLink, "https://dev.azure.com/backlog")
            .Add(x => x.NowDate, ""));

        var link = cut.Find("a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/backlog");
        link.GetAttribute("target").Should().Be("_blank");
        link.GetAttribute("rel").Should().Contain("noopener");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    [Fact]
    public void Header_WithEmptyBacklogLink_ShouldNotRenderLink()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.BacklogLink, "")
            .Add(x => x.NowDate, ""));

        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void Header_WithNullBacklogLink_ShouldNotRenderLink()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.NowDate, ""));

        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void Header_ShouldRenderLegendItems()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.NowDate, "2026-04-10")
            .Add(x => x.CurrentMonth, "Apr"));

        var legendItems = cut.FindAll(".legend-item");
        legendItems.Should().HaveCount(4);
    }

    [Fact]
    public void Header_LegendShouldContainPocMilestone()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.NowDate, ""));

        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("#F4B400");
    }

    [Fact]
    public void Header_LegendShouldContainProductionRelease()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.NowDate, ""));

        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("#34A853");
    }

    [Fact]
    public void Header_LegendShouldContainCheckpoint()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.NowDate, ""));

        cut.Markup.Should().Contain("Checkpoint");
    }

    [Fact]
    public void Header_WithValidNowDate_ShouldDisplayYear()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.NowDate, "2026-04-10"));

        cut.Markup.Should().Contain("Apr 2026");
    }

    [Fact]
    public void Header_WithInvalidNowDate_ShouldShowEmptyYear()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.NowDate, "not-a-date"));

        // Should show "Now (Apr )" since year is empty
        cut.Markup.Should().Contain("Apr");
        cut.Markup.Should().NotContain("2026");
    }

    [Fact]
    public void Header_WithEmptyNowDate_ShouldShowEmptyYear()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.NowDate, ""));

        cut.Markup.Should().Contain("Now (Apr ");
    }

    [Fact]
    public void Header_ShouldRenderHdrLeftAndHdrRight()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.NowDate, ""));

        cut.Find(".hdr-left").Should().NotBeNull();
        cut.Find(".hdr-right").Should().NotBeNull();
    }

    [Fact]
    public void Header_WithEmptyTitle_ShouldStillRender()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p => p
            .Add(x => x.Title, "")
            .Add(x => x.NowDate, ""));

        cut.Find("h1").Should().NotBeNull();
    }
}