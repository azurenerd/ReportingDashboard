using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Shared;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class HeaderComponentTests : TestContext
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersTitle_WhenTitleParameterProvided()
    {
        var cut = RenderComponent<Header>(p => p
            .Add(x => x.Title, "Privacy Automation Release Roadmap")
            .Add(x => x.Subtitle, "")
            .Add(x => x.BacklogUrl, ""));

        var titleElement = cut.Find("span.title");
        titleElement.TextContent.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersSubtitle_WhenSubtitleParameterProvided()
    {
        var cut = RenderComponent<Header>(p => p
            .Add(x => x.Title, "Test Title")
            .Add(x => x.Subtitle, "Trusted Platform · Privacy Automation Workstream · April 2026")
            .Add(x => x.BacklogUrl, ""));

        var subElement = cut.Find("div.sub");
        subElement.TextContent.Should().Be("Trusted Platform · Privacy Automation Workstream · April 2026");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersBacklogLink_WhenBacklogUrlProvided()
    {
        var cut = RenderComponent<Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.Subtitle, "Sub")
            .Add(x => x.BacklogUrl, "https://dev.azure.com/org/project/_backlogs"));

        var link = cut.Find("a.ado-link");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/org/project/_backlogs");
        link.GetAttribute("target").Should().Be("_blank");
        link.GetAttribute("rel").Should().Be("noopener noreferrer");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_HidesBacklogLink_WhenBacklogUrlIsEmpty()
    {
        var cut = RenderComponent<Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.Subtitle, "Sub")
            .Add(x => x.BacklogUrl, ""));

        cut.Markup.Should().NotContain("<a");
        cut.Markup.Should().NotContain("ado-link");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersFourLegendItems_WithCorrectLabels()
    {
        var cut = RenderComponent<Header>(p => p
            .Add(x => x.Title, "Test")
            .Add(x => x.Subtitle, "Sub")
            .Add(x => x.BacklogUrl, ""));

        var legendItems = cut.FindAll("div.legend-item");
        legendItems.Should().HaveCount(4);

        var labels = cut.FindAll("span.legend-label");
        labels[0].TextContent.Should().Be("PoC Milestone");
        labels[1].TextContent.Should().Be("Production Release");
        labels[2].TextContent.Should().Be("Checkpoint");
        labels[3].TextContent.Should().Be("Now");

        // Verify icon CSS classes
        cut.FindAll("span.icon.diamond.poc").Should().HaveCount(1);
        cut.FindAll("span.icon.diamond.prod").Should().HaveCount(1);
        cut.FindAll("span.icon.checkpoint").Should().HaveCount(1);
        cut.FindAll("span.icon.now-line").Should().HaveCount(1);
    }
}