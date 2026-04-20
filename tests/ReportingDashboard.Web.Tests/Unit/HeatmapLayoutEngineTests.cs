using System;
using System.Collections.Generic;
using FluentAssertions;
using ReportingDashboard.Web.Layout;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.ViewModels;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class HeatmapLayoutEngineTests
{
    private static readonly string[] Months = { "Jan", "Feb", "Mar", "Apr" };

    private static Heatmap MakeHeatmap(
        IReadOnlyList<HeatmapRow>? rows = null,
        int maxItemsPerCell = 4,
        int? currentMonthIndex = null,
        IReadOnlyList<string>? months = null)
    {
        return new Heatmap
        {
            Months = months ?? Months,
            MaxItemsPerCell = maxItemsPerCell,
            CurrentMonthIndex = currentMonthIndex,
            Rows = rows ?? Array.Empty<HeatmapRow>(),
        };
    }

    private static HeatmapRow MakeRow(HeatmapCategory category, params IReadOnlyList<string>[] cells)
    {
        return new HeatmapRow
        {
            Category = category,
            Cells = cells,
        };
    }

    [Theory]
    [InlineData(1, 1, 0, false)]
    [InlineData(4, 4, 0, false)]
    [InlineData(5, 3, 2, false)]
    [InlineData(9, 3, 6, false)]
    public void Build_Truncates_Items_And_Sets_OverflowCount(int input, int expectedItems, int expectedOverflow, bool expectedEmpty)
    {
        var items = new List<string>();
        for (int i = 0; i < input; i++) items.Add($"item{i}");

        IReadOnlyList<string> empty = Array.Empty<string>();
        var heatmap = MakeHeatmap(new HeatmapRow[]
        {
            MakeRow(HeatmapCategory.Shipped, items, empty, empty, empty),
        });

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 4, 19));

        var cell = vm.Rows[0].Cells[0];
        cell.Items.Count.Should().Be(expectedItems);
        cell.OverflowCount.Should().Be(expectedOverflow);
        cell.IsEmpty.Should().Be(expectedEmpty);
    }

    [Fact]
    public void Build_EmptyInput_ProducesEmptyCell()
    {
        IReadOnlyList<string> empty = Array.Empty<string>();
        var heatmap = MakeHeatmap(new HeatmapRow[]
        {
            MakeRow(HeatmapCategory.Shipped, empty, empty, empty, empty),
        });

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 4, 19));

        var cell = vm.Rows[0].Cells[0];
        cell.IsEmpty.Should().BeTrue();
        cell.Items.Should().BeEmpty();
        cell.OverflowCount.Should().Be(0);
    }

    [Theory]
    [InlineData(2026, 4, 19, 3)]
    [InlineData(2026, 7, 1, -1)]
    public void Build_Resolves_CurrentMonthIndex_From_Today(int y, int m, int d, int expected)
    {
        var heatmap = MakeHeatmap(currentMonthIndex: null);

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(y, m, d));

        vm.CurrentMonthIndex.Should().Be(expected);
    }

    [Fact]
    public void Build_Explicit_CurrentMonthIndex_Overrides_Today_And_OutOfRange_Falls_Back()
    {
        var inRange = MakeHeatmap(currentMonthIndex: 1);
        var oor = MakeHeatmap(currentMonthIndex: 99);

        var vmInRange = HeatmapLayoutEngine.Build(inRange, new DateOnly(2026, 4, 19));
        var vmOor = HeatmapLayoutEngine.Build(oor, new DateOnly(2026, 4, 19));

        vmInRange.CurrentMonthIndex.Should().Be(1);
        vmOor.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    public void Build_Enforces_Canonical_Row_Order_With_Uppercase_Labels_And_Fills_Missing()
    {
        IReadOnlyList<string> empty = Array.Empty<string>();
        var heatmap = MakeHeatmap(new HeatmapRow[]
        {
            MakeRow(HeatmapCategory.Blockers, empty, empty, empty, empty),
            MakeRow(HeatmapCategory.Shipped, empty, empty, empty, empty),
        });

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 4, 19));

        vm.Rows.Should().HaveCount(4);
        vm.Rows[0].Category.Should().Be(HeatmapCategory.Shipped);
        vm.Rows[1].Category.Should().Be(HeatmapCategory.InProgress);
        vm.Rows[2].Category.Should().Be(HeatmapCategory.Carryover);
        vm.Rows[3].Category.Should().Be(HeatmapCategory.Blockers);

        vm.Rows[0].HeaderLabel.Should().Be("SHIPPED");
        vm.Rows[1].HeaderLabel.Should().Be("IN PROGRESS");
        vm.Rows[2].HeaderLabel.Should().Be("CARRYOVER");
        vm.Rows[3].HeaderLabel.Should().Be("BLOCKERS");

        vm.Rows[1].Cells.Should().HaveCount(Months.Length);
        vm.Rows[1].Cells.Should().OnlyContain(c => c.IsEmpty);
    }
}