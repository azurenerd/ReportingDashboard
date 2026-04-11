using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the conditional backlog link rendering in Components/Header.razor.
/// The component uses @if (!string.IsNullOrEmpty(Data.BacklogLink)) to hide/show the link.
/// Existing HeaderInlineTests checks empty title renders h1, but doesn't fully verify
/// the conditional link visibility for null/empty/whitespace cases.
/// </summary>
[Trait("Category", "Unit")]
public class HeaderBacklogLinkConditionalTests : TestContext
{
    private static DashboardData CreateData(string? backlogLink = "https://link") => new()
    {
        Title = "Test",
        Subtitle = "Sub",
        BacklogLink = backlogLink ?? "",
        CurrentMonth = "April",
        Months = new List<string> { "April" },
        Timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        },
        Heatmap = new HeatmapData()
    };

    [Fact]
    public void Header_WithValidBacklogLink_RendersAnchorTag()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("https://dev.azure.com/backlog")));

        var links = cut.FindAll("a");
        Assert.Single(links);
    }

    [Fact]
    public void Header_WithEmptyBacklogLink_HidesAnchorTag()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("")));

        var links = cut.FindAll("a");
        Assert.Empty(links);
    }

    [Fact]
    public void Header_WithNullBacklogLink_HidesAnchorTag()
    {
        var data = CreateData();
        data.BacklogLink = null!;

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        var links = cut.FindAll("a");
        Assert.Empty(links);
    }

    [Fact]
    public void Header_WithWhitespaceOnlyBacklogLink_ShowsLink()
    {
        // string.IsNullOrEmpty(" ") is false, so whitespace link should render
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("   ")));

        var links = cut.FindAll("a");
        Assert.Single(links);
    }

    [Fact]
    public void Header_BacklogLink_HasHdrLinkClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("https://link")));

        var link = cut.Find("a");
        Assert.Contains("hdr-link", link.GetAttribute("class") ?? "");
    }

    [Fact]
    public void Header_BacklogLink_HasTargetBlank()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("https://link")));

        var link = cut.Find("a");
        Assert.Equal("_blank", link.GetAttribute("target"));
    }

    [Fact]
    public void Header_BacklogLink_TextContainsADOBacklog()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("https://link")));

        var link = cut.Find("a");
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void Header_BacklogLink_HrefMatchesParameter()
    {
        var url = "https://dev.azure.com/org/project/_backlogs?q=test&filter=sprint";
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(url)));

        var link = cut.Find("a");
        Assert.Equal(url, link.GetAttribute("href"));
    }

    [Fact]
    public void Header_BacklogLink_IsInsideH1()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("https://link")));

        // The <a> should be inside <h1>
        var h1 = cut.Find("h1");
        var linkInH1 = h1.QuerySelector("a");
        Assert.NotNull(linkInH1);
    }

    [Fact]
    public void Header_WithEmptyLink_H1StillRendersTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData("")));

        var h1 = cut.Find("h1");
        Assert.Contains("Test", h1.TextContent);
    }
}