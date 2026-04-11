using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Boundary and edge case tests for Components/Header.razor.
/// Tests extreme inputs, special characters in all fields,
/// and uncommon but valid data combinations.
/// </summary>
[Trait("Category", "Unit")]
public class HeaderBoundaryTests : TestContext
{
    private static DashboardData CreateMinimalData() => new()
    {
        Title = "",
        Subtitle = "",
        BacklogLink = "",
        CurrentMonth = "",
        Months = new List<string>(),
        Timeline = new TimelineData
        {
            StartDate = "",
            EndDate = "",
            NowDate = "",
            Tracks = new List<TimelineTrack>()
        },
        Heatmap = new HeatmapData()
    };

    private static DashboardData CreateFullData() => new()
    {
        Title = "Test",
        Subtitle = "Sub",
        BacklogLink = "https://link",
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

    #region All-Empty Fields

    [Fact]
    public void Header_WithAllEmptyFields_RendersWithoutException()
    {
        var data = CreateMinimalData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.NotNull(cut.Find(".hdr"));
    }

    [Fact]
    public void Header_WithAllEmptyFields_StillHasFourLegendItems()
    {
        var data = CreateMinimalData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        var items = cut.FindAll(".legend-item");
        Assert.Equal(4, items.Count);
    }

    [Fact]
    public void Header_WithAllEmptyFields_NowLabelShowsEmptyParens()
    {
        var data = CreateMinimalData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now ()", cut.Markup);
    }

    #endregion

    #region Very Long Strings

    [Fact]
    public void Header_WithVeryLongTitle_RendersWithoutError()
    {
        var data = CreateFullData();
        data.Title = new string('X', 1000);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains(new string('X', 100), cut.Find("h1").TextContent);
    }

    [Fact]
    public void Header_WithVeryLongSubtitle_RendersWithoutError()
    {
        var data = CreateFullData();
        data.Subtitle = new string('Y', 500);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains(new string('Y', 100), cut.Find(".sub").TextContent);
    }

    [Fact]
    public void Header_WithVeryLongBacklogUrl_RendersCorrectHref()
    {
        var data = CreateFullData();
        data.BacklogLink = "https://dev.azure.com/" + new string('a', 500);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Equal(data.BacklogLink, cut.Find("a").GetAttribute("href"));
    }

    [Fact]
    public void Header_WithVeryLongCurrentMonth_RendersInNowLabel()
    {
        var data = CreateFullData();
        data.CurrentMonth = new string('Z', 200);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains($"Now ({new string('Z', 200)} 2026)", cut.Markup);
    }

    #endregion

    #region HTML-Sensitive Characters

    [Fact]
    public void Header_Title_HtmlEncodesAngleBrackets()
    {
        var data = CreateFullData();
        data.Title = "<script>alert('xss')</script>";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        // Blazor auto-encodes, so no raw <script> tag in markup
        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("alert", cut.Find("h1").TextContent);
    }

    [Fact]
    public void Header_Subtitle_HtmlEncodesAmpersand()
    {
        var data = CreateFullData();
        data.Subtitle = "R&D Team - Q2 2026";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("R&amp;D Team", cut.Markup);
        Assert.Equal("R&D Team - Q2 2026", cut.Find(".sub").TextContent);
    }

    [Fact]
    public void Header_Title_HtmlEncodesQuotes()
    {
        var data = CreateFullData();
        data.Title = "Project \"Alpha\"";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Equal("Project \"Alpha\"", cut.Find("h1").TextContent.Split("ADO")[0].Trim());
    }

    #endregion

    #region Unicode and Special Characters

    [Fact]
    public void Header_Title_SupportsUnicode()
    {
        var data = CreateFullData();
        data.Title = "プロジェクト管理ダッシュボード";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("プロジェクト管理ダッシュボード", cut.Find("h1").TextContent);
    }

    [Fact]
    public void Header_Subtitle_SupportsEmoji()
    {
        var data = CreateFullData();
        data.Subtitle = "🚀 Team Rocket - April 2026";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("🚀 Team Rocket", cut.Find(".sub").TextContent);
    }

    [Fact]
    public void Header_CurrentMonth_SupportsNonLatinScript()
    {
        var data = CreateFullData();
        data.CurrentMonth = "أبريل";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (أبريل 2026)", cut.Markup);
    }

    #endregion

    #region Whitespace Handling

    [Fact]
    public void Header_Title_PreservesInternalWhitespace()
    {
        var data = CreateFullData();
        data.Title = "Project   Alpha   Dashboard";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Project   Alpha   Dashboard", cut.Find("h1").TextContent);
    }

    [Fact]
    public void Header_Subtitle_WithLeadingTrailingSpaces_Renders()
    {
        var data = CreateFullData();
        data.Subtitle = "  Team A  ";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Team A", cut.Find(".sub").TextContent);
    }

    #endregion

    #region Backlog Link Edge Cases

    [Fact]
    public void Header_BacklogLink_WithFragmentIdentifier_RendersCorrectly()
    {
        var data = CreateFullData();
        data.BacklogLink = "https://dev.azure.com/project#section1";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Equal("https://dev.azure.com/project#section1", cut.Find("a").GetAttribute("href"));
    }

    [Fact]
    public void Header_BacklogLink_WithQueryParams_RendersCorrectly()
    {
        var data = CreateFullData();
        data.BacklogLink = "https://dev.azure.com/project?a=1&b=2&c=3";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        var href = cut.Find("a").GetAttribute("href");
        Assert.Contains("a=1", href ?? "");
        Assert.Contains("b=2", href ?? "");
    }

    [Fact]
    public void Header_BacklogLink_WithNonHttpProtocol_RendersAsIs()
    {
        var data = CreateFullData();
        data.BacklogLink = "mailto:team@example.com";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Equal("mailto:team@example.com", cut.Find("a").GetAttribute("href"));
    }

    #endregion
}