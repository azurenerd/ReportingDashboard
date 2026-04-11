using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests verifying the overall DOM structure of Components/Header.razor.
/// Ensures the component produces the correct hierarchy: div.hdr > h1 + div.sub + div.legend
/// and that legend items contain both a symbol span and text label.
/// </summary>
[Trait("Category", "Unit")]
public class HeaderStructureTests : TestContext
{
    private static DashboardData CreateData(
        string title = "Test",
        string subtitle = "Sub",
        string currentMonth = "April") => new()
    {
        Title = title,
        Subtitle = subtitle,
        BacklogLink = "https://link",
        CurrentMonth = currentMonth,
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

    #region Outer Container

    [Fact]
    public void Header_OuterDiv_HasHdrClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var outer = cut.Find("div.hdr");
        Assert.NotNull(outer);
    }

    [Fact]
    public void Header_OuterDiv_ContainsH1()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var hdr = cut.Find(".hdr");
        var h1 = hdr.QuerySelector("h1");
        Assert.NotNull(h1);
    }

    [Fact]
    public void Header_OuterDiv_ContainsSubDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var hdr = cut.Find(".hdr");
        var sub = hdr.QuerySelector(".sub");
        Assert.NotNull(sub);
    }

    [Fact]
    public void Header_OuterDiv_ContainsLegendDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var hdr = cut.Find(".hdr");
        var legend = hdr.QuerySelector(".legend");
        Assert.NotNull(legend);
    }

    #endregion

    #region Legend Item Internal Structure

    [Fact]
    public void Header_LegendItems_AreSpanElements()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var items = cut.FindAll(".legend-item");
        foreach (var item in items)
        {
            Assert.Equal("SPAN", item.TagName);
        }
    }

    [Fact]
    public void Header_EachLegendItem_ContainsSymbolSpan()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var items = cut.FindAll(".legend-item");
        foreach (var item in items)
        {
            var innerSpan = item.QuerySelector("span");
            Assert.NotNull(innerSpan);
        }
    }

    #endregion

    #region Data Binding Verification

    [Fact]
    public void Header_Title_ReflectsDataParameter()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(title: "My Custom Title")));

        var h1 = cut.Find("h1");
        Assert.Contains("My Custom Title", h1.TextContent);
    }

    [Fact]
    public void Header_Subtitle_ReflectsDataParameter()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(subtitle: "Engineering - Q2 2026")));

        var sub = cut.Find(".sub");
        Assert.Equal("Engineering - Q2 2026", sub.TextContent);
    }

    [Fact]
    public void Header_NowLabel_ReflectsCurrentMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(currentMonth: "December")));

        Assert.Contains("Now (December 2026)", cut.Markup);
    }

    #endregion

    #region Re-render on Parameter Change

    [Fact]
    public void Header_ReRendersTitle_WhenParameterChanges()
    {
        var data = CreateData(title: "Original Title");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Original Title", cut.Find("h1").TextContent);

        var newData = CreateData(title: "Updated Title");
        cut.SetParametersAndRender(p => p.Add(x => x.Data, newData));

        Assert.Contains("Updated Title", cut.Find("h1").TextContent);
        Assert.DoesNotContain("Original Title", cut.Find("h1").TextContent);
    }

    [Fact]
    public void Header_ReRendersSubtitle_WhenParameterChanges()
    {
        var data = CreateData(subtitle: "Team A");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Equal("Team A", cut.Find(".sub").TextContent);

        var newData = CreateData(subtitle: "Team B");
        cut.SetParametersAndRender(p => p.Add(x => x.Data, newData));

        Assert.Equal("Team B", cut.Find(".sub").TextContent);
    }

    [Fact]
    public void Header_ReRendersNowLabel_WhenMonthChanges()
    {
        var data = CreateData(currentMonth: "March");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (March 2026)", cut.Markup);

        var newData = CreateData(currentMonth: "July");
        cut.SetParametersAndRender(p => p.Add(x => x.Data, newData));

        Assert.Contains("Now (July 2026)", cut.Markup);
    }

    [Fact]
    public void Header_ReRendersBacklogLink_WhenLinkBecomesEmpty()
    {
        var data = CreateData();
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Single(cut.FindAll("a"));

        var newData = CreateData();
        newData.BacklogLink = "";
        cut.SetParametersAndRender(p => p.Add(x => x.Data, newData));

        Assert.Empty(cut.FindAll("a"));
    }

    [Fact]
    public void Header_ReRendersBacklogLink_WhenLinkAppearsFromEmpty()
    {
        var data = CreateData();
        data.BacklogLink = "";
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Empty(cut.FindAll("a"));

        var newData = CreateData();
        newData.BacklogLink = "https://new-link";
        cut.SetParametersAndRender(p => p.Add(x => x.Data, newData));

        Assert.Single(cut.FindAll("a"));
        Assert.Equal("https://new-link", cut.Find("a").GetAttribute("href"));
    }

    #endregion
}