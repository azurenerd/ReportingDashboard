using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Heatmap.razor targeting the ACTUAL component implementation.
/// Existing HeatmapTests.cs uses wrong namespace (Components.Sections) and
/// wrong CSS classes (cur-hdr instead of apr-hdr, checks for "Now" label that doesn't exist).
/// </summary>
[Trait("Category", "Unit")]
public class HeatmapActualTests : TestContext
{
    private static HeatmapData CreateEmptyHeatmap() => new()
    {
        Shipped = new Dictionary<string, List<string>>(),
        InProgress = new Dictionary<string, List<string>>(),
        Carryover = new Dictionary<string, List<string>>(),
        Blockers = new Dictionary<string, List<string>>()
    };

    #region Wrapper and Title

    [Fact]
    public void Heatmap_RendersHmWrapDiv()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        Assert.NotNull(cut.Find(".hm-wrap"));
    }

    [Fact]
    public void Heatmap_TitleContainsExpectedText()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        var title = cut.Find(".hm-title");
        var text = title.TextContent.ToUpperInvariant();
        Assert.Contains("MONTHLY EXECUTION HEATMAP", text);
        Assert.Contains("SHIPPED", text);
        Assert.Contains("IN PROGRESS", text);
        Assert.Contains("CARRYOVER", text);
        Assert.Contains("BLOCKERS", text);
    }

    #endregion

    #region Grid Structure

    [Fact]
    public void Heatmap_GridHasCorrectColumnTemplate()
    {
        var months = new List<string> { "Jan", "Feb", "Mar" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Jan"));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("160px", style);
        Assert.Contains("repeat(3, 1fr)", style);
    }

    [Fact]
    public void Heatmap_GridHasCorrectRowTemplate()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("36px repeat(4, 1fr)", style);
    }

    [Fact]
    public void Heatmap_SingleMonth_GridRepeat1()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        var style = cut.Find(".hm-grid").GetAttribute("style") ?? "";
        Assert.Contains("repeat(1, 1fr)", style);
    }

    #endregion

    #region Corner Cell

    [Fact]
    public void Heatmap_RendersCornerCellWithSTATUS()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var corner = cut.Find(".hm-corner");
        Assert.Equal("STATUS", corner.TextContent);
    }

    #endregion

    #region Column Headers

    [Fact]
    public void Heatmap_RendersCorrectNumberOfColumnHeaders()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Equal(4, headers.Count);
    }

    [Fact]
    public void Heatmap_ColumnHeadersShowMonthNames()
    {
        var months = new List<string> { "January", "February" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.Contains("January", headers[0].TextContent);
        Assert.Contains("February", headers[1].TextContent);
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_HasAprHdrClass()
    {
        var months = new List<string> { "Jan", "Feb" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Feb"));

        var headers = cut.FindAll(".hm-col-hdr");
        Assert.DoesNotContain("apr-hdr", headers[0].GetAttribute("class") ?? "");
        Assert.Contains("apr-hdr", headers[1].GetAttribute("class") ?? "");
    }

    [Fact]
    public void Heatmap_NoCurrentMonth_NoAprHdrClass()
    {
        var months = new List<string> { "Jan", "Feb" };
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var headers = cut.FindAll(".hm-col-hdr");
        foreach (var h in headers)
        {
            Assert.DoesNotContain("apr-hdr", h.GetAttribute("class") ?? "");
        }
    }

    [Fact]
    public void Heatmap_CaseInsensitiveCurrentMonth_AprHdrApplied()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "APRIL" })
            .Add(x => x.CurrentMonth, "april"));

        var header = cut.Find(".hm-col-hdr");
        Assert.Contains("apr-hdr", header.GetAttribute("class") ?? "");
    }

    #endregion

    #region Category Rows

    [Fact]
    public void Heatmap_RendersFourRowHeaders()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, rowHeaders.Count);
    }

    [Fact]
    public void Heatmap_RowHeadersContainExpectedLabels()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Contains("SHIPPED", rowHeaders[0].TextContent);
        Assert.Contains("IN PROGRESS", rowHeaders[1].TextContent);
        Assert.Contains("CARRYOVER", rowHeaders[2].TextContent);
        Assert.Contains("BLOCKERS", rowHeaders[3].TextContent);
    }

    [Fact]
    public void Heatmap_AllFourCssPrefixHeadersPresent()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".ship-hdr"));
        Assert.NotNull(cut.Find(".prog-hdr"));
        Assert.NotNull(cut.Find(".carry-hdr"));
        Assert.NotNull(cut.Find(".block-hdr"));
    }

    #endregion

    #region Data Flow Through Rows

    [Fact]
    public void Heatmap_ShippedDataRendersInShipCells()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["jan"] = new() { "Ship Item" } },
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        };

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("Ship Item", cut.Markup);
        Assert.NotNull(cut.Find(".ship-cell"));
    }

    [Fact]
    public void Heatmap_AllCategoriesRenderData()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["jan"] = new() { "S1" } },
            InProgress = new Dictionary<string, List<string>> { ["jan"] = new() { "P1" } },
            Carryover = new Dictionary<string, List<string>> { ["jan"] = new() { "C1" } },
            Blockers = new Dictionary<string, List<string>> { ["jan"] = new() { "B1" } }
        };

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, heatmap)
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, ""));

        Assert.Contains("S1", cut.Markup);
        Assert.Contains("P1", cut.Markup);
        Assert.Contains("C1", cut.Markup);
        Assert.Contains("B1", cut.Markup);
    }

    [Fact]
    public void Heatmap_MissingMonthKeys_RenderDashes()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, ""));

        // 4 rows × 2 months = 8 cells, all should have dashes
        var empties = cut.FindAll(".hm-empty");
        Assert.Equal(8, empties.Count);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Heatmap_EmptyMonthsList_RendersOnlyCornerAndRowHeaders()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, ""));

        Assert.NotNull(cut.Find(".hm-corner"));
        Assert.Equal(4, cut.FindAll(".hm-row-hdr").Count);
        Assert.Empty(cut.FindAll(".hm-col-hdr"));
        Assert.Empty(cut.FindAll(".hm-cell"));
    }

    [Fact]
    public void Heatmap_GridColumnsAdjustForMonthCount()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, CreateEmptyHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" })
            .Add(x => x.CurrentMonth, ""));

        var style = cut.Find(".hm-grid").GetAttribute("style") ?? "";
        Assert.Contains("repeat(6, 1fr)", style);
    }

    #endregion
}