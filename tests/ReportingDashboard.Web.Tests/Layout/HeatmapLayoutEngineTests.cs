using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using ReportingDashboard.Web.Layout;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Layout;

public class HeatmapLayoutEngineTests
{
    private static readonly IReadOnlyList<string> DefaultMonths = new[] { "Jan", "Feb", "Mar", "Apr" };

    private static Heatmap MakeHeatmap(
        IReadOnlyList<IReadOnlyList<string>>? shippedCells = null,
        int? currentMonthIndex = null,
        int maxItemsPerCell = 4,
        IReadOnlyList<string>? months = null)
    {
        var ms = months ?? DefaultMonths;
        var empty = new IReadOnlyList<string>[ms.Count];
        for (int i = 0; i < ms.Count; i++) empty[i] = Array.Empty<string>();

        return new Heatmap
        {
            Months = ms,
            CurrentMonthIndex = currentMonthIndex,
            MaxItemsPerCell = maxItemsPerCell,
            Rows = new[]
            {
                new HeatmapRow { Category = HeatmapCategory.Shipped,    Cells = shippedCells ?? empty },
                new HeatmapRow { Category = HeatmapCategory.InProgress, Cells = empty },
                new HeatmapRow { Category = HeatmapCategory.Carryover,  Cells = empty },
                new HeatmapRow { Category = HeatmapCategory.Blockers,   Cells = empty },
            }
        };
    }

    [Fact]
    public void Truncation_OneItem_NoOverflow()
    {
        var cells = new IReadOnlyList<string>[]
        {
            new[] { "A" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
        };
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(cells, maxItemsPerCell: 4), new DateOnly(2026, 4, 1));

        var c = vm.Rows[0].Cells[0];
        c.IsEmpty.Should().BeFalse();
        c.Items.Should().Equal("A");
        c.OverflowCount.Should().Be(0);
    }

    [Fact]
    public void Truncation_ExactlyN_NoOverflow()
    {
        var items = new[] { "A", "B", "C", "D" };
        var cells = new IReadOnlyList<string>[]
        {
            items, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
        };
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(cells, maxItemsPerCell: 4), new DateOnly(2026, 4, 1));

        var c = vm.Rows[0].Cells[0];
        c.Items.Should().Equal("A", "B", "C", "D");
        c.OverflowCount.Should().Be(0);
        c.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Truncation_NPlus1_KeepsFirst3AndOverflow2()
    {
        var items = new[] { "A", "B", "C", "D", "E" };
        var cells = new IReadOnlyList<string>[]
        {
            items, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
        };
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(cells, maxItemsPerCell: 4), new DateOnly(2026, 4, 1));

        var c = vm.Rows[0].Cells[0];
        c.Items.Should().Equal("A", "B", "C");
        c.OverflowCount.Should().Be(2);
    }

    [Fact]
    public void Truncation_NPlus5_KeepsFirst3AndOverflow6()
    {
        var items = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
        var cells = new IReadOnlyList<string>[]
        {
            items, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
        };
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(cells, maxItemsPerCell: 4), new DateOnly(2026, 4, 1));

        var c = vm.Rows[0].Cells[0];
        c.Items.Should().Equal("A", "B", "C");
        c.OverflowCount.Should().Be(6);
        (c.Items.Count + c.OverflowCount).Should().Be(items.Length);
    }

    [Fact]
    public void EmptyCell_IsFlaggedAndHasNoItems()
    {
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(), new DateOnly(2026, 4, 1));
        var c = vm.Rows[0].Cells[0];
        c.IsEmpty.Should().BeTrue();
        c.Items.Should().BeEmpty();
        c.OverflowCount.Should().Be(0);
    }

    [Fact]
    public void CurrentMonthIndex_AutoResolves_MatchingMonth()
    {
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(), new DateOnly(2026, 4, 15));
        vm.CurrentMonthIndex.Should().Be(3);
    }

    [Fact]
    public void CurrentMonthIndex_NonMatching_ReturnsMinus1()
    {
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(), new DateOnly(2026, 7, 15));
        vm.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    public void CurrentMonthIndex_ExplicitIndex_Overrides()
    {
        var vm = HeatmapLayoutEngine.Build(
            MakeHeatmap(currentMonthIndex: 1),
            new DateOnly(2026, 4, 15));
        vm.CurrentMonthIndex.Should().Be(1);
    }

    [Fact]
    public void CurrentMonthIndex_ExplicitOutOfRange_ReturnsMinus1()
    {
        var vm = HeatmapLayoutEngine.Build(
            MakeHeatmap(currentMonthIndex: 99),
            new DateOnly(2026, 4, 15));
        vm.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    public void RowOrdering_IsShippedInProgressCarryoverBlockers_RegardlessOfInputOrder()
    {
        var empty = new IReadOnlyList<string>[] { Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() };
        var hm = new Heatmap
        {
            Months = DefaultMonths,
            CurrentMonthIndex = null,
            MaxItemsPerCell = 4,
            Rows = new[]
            {
                new HeatmapRow { Category = HeatmapCategory.Blockers,   Cells = empty },
                new HeatmapRow { Category = HeatmapCategory.Carryover,  Cells = empty },
                new HeatmapRow { Category = HeatmapCategory.InProgress, Cells = empty },
                new HeatmapRow { Category = HeatmapCategory.Shipped,    Cells = empty },
            }
        };

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 4, 1));

        vm.Rows.Select(r => r.Category).Should().Equal(
            HeatmapCategory.Shipped,
            HeatmapCategory.InProgress,
            HeatmapCategory.Carryover,
            HeatmapCategory.Blockers);

        vm.Rows.Select(r => r.HeaderLabel).Should().Equal(
            "SHIPPED", "IN PROGRESS", "CARRYOVER", "BLOCKERS");
    }

    [Fact]
    public void HeaderLabels_AreUppercase()
    {
        var vm = HeatmapLayoutEngine.Build(MakeHeatmap(), new DateOnly(2026, 4, 1));
        foreach (var r in vm.Rows)
        {
            r.HeaderLabel.Should().Be(r.HeaderLabel.ToUpperInvariant());
        }
    }

    [Fact]
    public void MissingRow_IsPaddedWithEmptyCells()
    {
        var hm = new Heatmap
        {
            Months = DefaultMonths,
            CurrentMonthIndex = null,
            MaxItemsPerCell = 4,
            Rows = new[]
            {
                new HeatmapRow
                {
                    Category = HeatmapCategory.Shipped,
                    Cells = new IReadOnlyList<string>[]
                    {
                        new[] { "A" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
                    }
                }
            }
        };

        var vm = HeatmapLayoutEngine.Build(hm, new DateOnly(2026, 4, 1));

        vm.Rows.Should().HaveCount(4);
        vm.Rows[1].Cells.Should().HaveCount(4);
        vm.Rows[1].Cells.Should().AllSatisfy(c => c.IsEmpty.Should().BeTrue());
    }

    [Fact]
    public void EmptyFactory_HasNoMonthsOrRows()
    {
        var vm = ReportingDashboard.Web.ViewModels.HeatmapViewModel.Empty;
        vm.Should().NotBeNull();
        vm.Months.Should().BeEmpty();
        vm.Rows.Should().BeEmpty();
        vm.CurrentMonthIndex.Should().Be(-1);
        vm.IsEmpty.Should().BeTrue();
    }
}