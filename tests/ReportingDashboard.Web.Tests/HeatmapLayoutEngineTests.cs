using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using ReportingDashboard.Web.Layout;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests;

[Trait("Category", "Unit")]
public class HeatmapLayoutEngineTests
{
    private static Heatmap MakeHeatmap(
        IReadOnlyList<string> months,
        IReadOnlyList<IReadOnlyList<string>> shippedCells,
        int? currentMonthIndex = null,
        int maxItemsPerCell = 4)
    {
        IReadOnlyList<IReadOnlyList<string>> EmptyCells() =>
            Enumerable.Range(0, months.Count)
                .Select(_ => (IReadOnlyList<string>)Array.Empty<string>())
                .ToList();

        return new Heatmap
        {
            Months = months,
            CurrentMonthIndex = currentMonthIndex,
            MaxItemsPerCell = maxItemsPerCell,
            Rows = new List<HeatmapRow>
            {
                new() { Category = HeatmapCategory.Shipped,    Cells = shippedCells },
                new() { Category = HeatmapCategory.InProgress, Cells = EmptyCells() },
                new() { Category = HeatmapCategory.Carryover,  Cells = EmptyCells() },
                new() { Category = HeatmapCategory.Blockers,   Cells = EmptyCells() },
            }
        };
    }

    private static IReadOnlyList<string> Items(int n) =>
        Enumerable.Range(1, n).Select(i => $"item-{i}").ToList();

    [Theory]
    [InlineData(1, 4, 1, 0, false)]    // 1 item, N=4 -> 1 item, no overflow
    [InlineData(4, 4, 4, 0, false)]    // exactly N, no truncation
    [InlineData(5, 4, 4, 2, false)]    // 5 items, N=4 -> keep 3 + "+2 more"
    [InlineData(9, 4, 4, 6, false)]    // 9 items, N=4 -> keep 3 + "+6 more"
    [InlineData(9, 5, 5, 5, false)]    // configured N=5 -> keep 4 + "+5 more"
    [InlineData(0, 4, 0, 0, true)]     // empty cell
    public void BuildCell_TruncatesPerMaxItems_AndFlagsEmpty(
        int itemCount, int maxItems, int expectedCount, int expectedOverflow, bool expectedEmpty)
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr" };
        var cells = new IReadOnlyList<string>[] { Items(itemCount), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() };
        var hm = MakeHeatmap(months, cells, currentMonthIndex: null, maxItemsPerCell: maxItems);

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 4, 15));

        var shippedCell = vm.Rows[0].Cells[0];
        shippedCell.Items.Should().HaveCount(expectedCount);
        shippedCell.OverflowCount.Should().Be(expectedOverflow);
        shippedCell.IsEmpty.Should().Be(expectedEmpty);

        if (expectedOverflow > 0)
        {
            shippedCell.Items.Last().Should().Be($"+{expectedOverflow} more");
        }
    }

    [Fact]
    public void Build_EmptyCell_IsFlaggedAndHasNoItems()
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr" };
        var cells = new IReadOnlyList<string>[] { Array.Empty<string>(), Items(2), Array.Empty<string>(), Items(1) };
        var hm = MakeHeatmap(months, cells);

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 4, 15));
        var row = vm.Rows[0];

        row.Cells[0].IsEmpty.Should().BeTrue();
        row.Cells[0].Items.Should().BeEmpty();
        row.Cells[1].IsEmpty.Should().BeFalse();
        row.Cells[1].Items.Should().HaveCount(2);
        row.Cells[2].IsEmpty.Should().BeTrue();
        row.Cells[3].IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Build_CurrentMonthIndex_ExplicitValueIsHonored()
    {
        var hm = MakeHeatmap(
            new[] { "Jan", "Feb", "Mar", "Apr" },
            new IReadOnlyList<string>[] { Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() },
            currentMonthIndex: 2);

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 6, 1));

        vm.CurrentMonthIndex.Should().Be(2);
    }

    [Fact]
    public void Build_CurrentMonthIndex_NullAutoResolvesFromTodayMonth()
    {
        var hm = MakeHeatmap(
            new[] { "Jan", "Feb", "Mar", "Apr" },
            new IReadOnlyList<string>[] { Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() },
            currentMonthIndex: null);

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 3, 10));

        vm.CurrentMonthIndex.Should().Be(2, "Mar is at index 2 in Months");
    }

    [Fact]
    public void Build_CurrentMonthIndex_NoMatchReturnsMinusOne()
    {
        var hm = MakeHeatmap(
            new[] { "Jan", "Feb", "Mar", "Apr" },
            new IReadOnlyList<string>[] { Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() },
            currentMonthIndex: null);

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 9, 10));

        vm.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    public void Build_ProducesRowsInFixedOrderEvenIfInputReordered()
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr" };
        IReadOnlyList<IReadOnlyList<string>> Empty() => Enumerable.Range(0, 4).Select(_ => (IReadOnlyList<string>)Array.Empty<string>()).ToList();

        var hm = new Heatmap
        {
            Months = months,
            MaxItemsPerCell = 4,
            Rows = new List<HeatmapRow>
            {
                new() { Category = HeatmapCategory.Blockers,   Cells = Empty() },
                new() { Category = HeatmapCategory.Carryover,  Cells = Empty() },
                new() { Category = HeatmapCategory.InProgress, Cells = Empty() },
                new() { Category = HeatmapCategory.Shipped,    Cells = Empty() },
            }
        };

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 4, 10));

        vm.Rows.Select(r => r.Category).Should().Equal(
            HeatmapCategory.Shipped,
            HeatmapCategory.InProgress,
            HeatmapCategory.Carryover,
            HeatmapCategory.Blockers);
    }
}
