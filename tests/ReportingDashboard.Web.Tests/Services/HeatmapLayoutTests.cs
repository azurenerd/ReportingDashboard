using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Services;

[Trait("Category", "Unit")]
public class HeatmapLayoutTests
{
    private static Heatmap MakeHeatmap(
        IReadOnlyList<string>? months = null,
        int? currentMonthIndex = null,
        int maxItemsPerCell = 0,
        IReadOnlyList<HeatmapRow>? rows = null)
    {
        return new Heatmap
        {
            Months = months ?? new[] { "Jan", "Feb", "Mar", "Apr" },
            CurrentMonthIndex = currentMonthIndex,
            MaxItemsPerCell = maxItemsPerCell,
            Rows = rows ?? Array.Empty<HeatmapRow>()
        };
    }

    [Fact]
    public void Build_EnforcesCanonicalRowOrder_AndFillsMissingCategories()
    {
        var rows = new[]
        {
            new HeatmapRow { Category = HeatmapCategory.Blockers, Cells = new List<IReadOnlyList<string>> { new[] { "b1" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() } },
            new HeatmapRow { Category = HeatmapCategory.Shipped, Cells = new List<IReadOnlyList<string>> { new[] { "s1" }, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() } },
        };
        var hm = MakeHeatmap(rows: rows);

        var vm = HeatmapLayout.Build(hm, new DateOnly(2026, 4, 10));

        vm.Rows.Select(r => r.Category).Should().ContainInOrder(
            HeatmapCategory.Shipped, HeatmapCategory.InProgress,
            HeatmapCategory.Carryover, HeatmapCategory.Blockers);

        vm.Rows[1].Cells.All(c => c.IsEmpty).Should().BeTrue();
        vm.Rows[2].Cells.All(c => c.IsEmpty).Should().BeTrue();
        vm.Rows[0].Cells[0].Items.Should().ContainSingle().Which.Should().Be("s1");
    }

    [Fact]
    public void Build_HonorsExplicitCurrentMonthIndex()
    {
        var hm = MakeHeatmap(currentMonthIndex: 1);
        var vm = HeatmapLayout.Build(hm, new DateOnly(2026, 3, 15));
        vm.CurrentMonthIndex.Should().Be(1);
    }

    [Fact]
    public void Build_MaxItemsPerCellZero_FallsBackToDefault()
    {
        var manyItems = Enumerable.Range(0, 10).Select(i => $"x{i}").ToArray();
        var rows = new[]
        {
            new HeatmapRow { Category = HeatmapCategory.Shipped, Cells = new List<IReadOnlyList<string>> { manyItems, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>() } }
        };
        var hm = MakeHeatmap(maxItemsPerCell: 0, rows: rows);

        var vm = HeatmapLayout.Build(hm, new DateOnly(2026, 4, 10), defaultMaxItems: 4);

        var cell = vm.Rows[0].Cells[0];
        cell.IsEmpty.Should().BeFalse();
        cell.OverflowCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Build_ProducesFourCanonicalRows_FromEmptyHeatmap()
    {
        var hm = MakeHeatmap(rows: Array.Empty<HeatmapRow>());
        var vm = HeatmapLayout.Build(hm, new DateOnly(2026, 4, 10));
        vm.Rows.Should().HaveCount(4);
        vm.Rows.SelectMany(r => r.Cells).All(c => c.IsEmpty).Should().BeTrue();
    }

    [Fact]
    public void Build_UnderInvariantCultureTrap_StillResolvesMonth()
    {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
            var hm = MakeHeatmap(months: new[] { "Jan", "Feb", "Mar", "Apr" });
            var vm = HeatmapLayout.Build(hm, new DateOnly(2026, 1, 5));
            vm.CurrentMonthIndex.Should().BeInRange(-1, 3);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}