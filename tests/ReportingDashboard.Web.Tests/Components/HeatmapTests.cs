using System.Collections.Generic;
using System.Linq;
using Bunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;
using HeatmapComponent = ReportingDashboard.Web.Components.Pages.Partials.Heatmap;

namespace ReportingDashboard.Web.Tests.Components;

[Trait("Category", "Unit")]
public class HeatmapTests
{
    private static HeatmapViewModel BuildModel(
        IReadOnlyList<string>? months = null,
        int currentMonthIndex = -1,
        IReadOnlyList<HeatmapRowView>? rows = null)
    {
        months ??= new[] { "Jan", "Feb", "Mar", "Apr" };
        if (rows is null)
        {
            var emptyCells = Enumerable.Range(0, months.Count)
                .Select(_ => new HeatmapCellView(System.Array.Empty<string>(), 0, IsEmpty: true))
                .ToList();
            rows = new[]
            {
                new HeatmapRowView(HeatmapCategory.Shipped, "SHIPPED", emptyCells),
                new HeatmapRowView(HeatmapCategory.InProgress, "IN PROGRESS", emptyCells),
                new HeatmapRowView(HeatmapCategory.Carryover, "CARRYOVER", emptyCells),
                new HeatmapRowView(HeatmapCategory.Blockers, "BLOCKERS", emptyCells),
            };
        }
        return new HeatmapViewModel(months, currentMonthIndex, rows);
    }

    [Fact]
    public void Renders_FullShell_With25GridChildren()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<HeatmapComponent>(p => p.Add(x => x.Model, BuildModel()));

        cut.Find(".hm-wrap").Should().NotBeNull();
        cut.Find(".hm-title").TextContent.Should().Contain("Monthly Execution Heatmap");
        cut.FindAll(".hm-grid > *").Count.Should().Be(25);
    }

    [Fact]
    public void CurrentMonthIndex_AppliesCurrentHdrAndCurrentCellClasses()
    {
        using var ctx = new Bunit.TestContext();
        var model = BuildModel(currentMonthIndex: 2);
        var cut = ctx.RenderComponent<HeatmapComponent>(p => p.Add(x => x.Model, model));

        var monthHdrs = cut.FindAll(".hm-col-hdr");
        monthHdrs.Count.Should().Be(4);
        monthHdrs[2].ClassList.Should().Contain("current-hdr");
        monthHdrs.Where((_, i) => i != 2).All(h => !h.ClassList.Contains("current-hdr")).Should().BeTrue();

        cut.FindAll(".hm-cell").Where((_, i) => i % 4 == 2)
           .All(c => c.ClassList.Contains("current")).Should().BeTrue();
    }

    [Fact]
    public void RowHeader_CssClasses_MapToCategories()
    {
        using var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<HeatmapComponent>(p => p.Add(x => x.Model, BuildModel()));

        var rowHdrs = cut.FindAll(".hm-row-hdr");
        rowHdrs.Count.Should().Be(4);
        rowHdrs[0].ClassList.Should().Contain("ship-hdr");
        rowHdrs[1].ClassList.Should().Contain("prog-hdr");
        rowHdrs[2].ClassList.Should().Contain("carry-hdr");
        rowHdrs[3].ClassList.Should().Contain("block-hdr");
    }

    [Fact]
    public void Cell_Items_RenderEmptyAndOverflowVariants()
    {
        using var ctx = new Bunit.TestContext();
        var months = new[] { "Jan", "Feb", "Mar", "Apr" };
        var cells = new List<HeatmapCellView>
        {
            new(new[] { "Alpha", "Beta" }, 0, IsEmpty: false),
            new(System.Array.Empty<string>(), 0, IsEmpty: true),
            new(new[] { "A", "B", "C" }, 5, IsEmpty: false),
            new(System.Array.Empty<string>(), 0, IsEmpty: true),
        };
        var rows = new[]
        {
            new HeatmapRowView(HeatmapCategory.Shipped, "SHIPPED", cells),
            new HeatmapRowView(HeatmapCategory.InProgress, "IN PROGRESS",
                Enumerable.Range(0,4).Select(_ => new HeatmapCellView(System.Array.Empty<string>(),0,true)).ToList()),
            new HeatmapRowView(HeatmapCategory.Carryover, "CARRYOVER",
                Enumerable.Range(0,4).Select(_ => new HeatmapCellView(System.Array.Empty<string>(),0,true)).ToList()),
            new HeatmapRowView(HeatmapCategory.Blockers, "BLOCKERS",
                Enumerable.Range(0,4).Select(_ => new HeatmapCellView(System.Array.Empty<string>(),0,true)).ToList()),
        };
        var cut = ctx.RenderComponent<HeatmapComponent>(p => p.Add(x => x.Model, new HeatmapViewModel(months, -1, rows)));

        var dataCells = cut.FindAll(".hm-cell");
        var firstCellItems = dataCells[0].QuerySelectorAll(".it");
        firstCellItems.Length.Should().Be(2);
        firstCellItems[0].TextContent.Trim().Should().Be("Alpha");
        firstCellItems[1].TextContent.Trim().Should().Be("Beta");

        var emptyItems = dataCells[1].QuerySelectorAll(".it.empty");
        emptyItems.Length.Should().Be(1);
        emptyItems[0].TextContent.Trim().Should().Be("-");

        var overflowCellItems = dataCells[2].QuerySelectorAll(".it");
        overflowCellItems.Length.Should().Be(4);
        var last = overflowCellItems[3];
        last.ClassList.Should().Contain("overflow");
        last.TextContent.Trim().Should().Be("+5 more");
    }

    [Fact]
    public void ItemText_IsHtmlEncoded_PreventingXss()
    {
        using var ctx = new Bunit.TestContext();
        var months = new[] { "Jan", "Feb", "Mar", "Apr" };
        var payload = "<script>alert(1)</script>";
        var firstRowCells = new List<HeatmapCellView>
        {
            new(new[] { payload }, 0, IsEmpty: false),
            new(System.Array.Empty<string>(),0,true),
            new(System.Array.Empty<string>(),0,true),
            new(System.Array.Empty<string>(),0,true),
        };
        var rows = new[]
        {
            new HeatmapRowView(HeatmapCategory.Shipped, "SHIPPED", firstRowCells),
            new HeatmapRowView(HeatmapCategory.InProgress, "IN PROGRESS",
                Enumerable.Range(0,4).Select(_ => new HeatmapCellView(System.Array.Empty<string>(),0,true)).ToList()),
            new HeatmapRowView(HeatmapCategory.Carryover, "CARRYOVER",
                Enumerable.Range(0,4).Select(_ => new HeatmapCellView(System.Array.Empty<string>(),0,true)).ToList()),
            new HeatmapRowView(HeatmapCategory.Blockers, "BLOCKERS",
                Enumerable.Range(0,4).Select(_ => new HeatmapCellView(System.Array.Empty<string>(),0,true)).ToList()),
        };
        var cut = ctx.RenderComponent<HeatmapComponent>(p => p.Add(x => x.Model, new HeatmapViewModel(months, -1, rows)));

        cut.FindAll(".hm-cell script").Count.Should().Be(0);
        cut.Markup.Should().Contain("&lt;script&gt;");
        cut.Markup.Should().NotContain("<script>alert(1)</script>");
    }
}