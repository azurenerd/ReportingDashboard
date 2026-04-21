using Xunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests;

public class HeatmapLayoutEngineTests
{
    private static Heatmap CreateHeatmap(
        IReadOnlyList<IReadOnlyList<string>>? shippedCells = null,
        int maxItems = 4,
        int? currentMonthIndex = null)
    {
        return new Heatmap
        {
            Months = new[] { "Jan", "Feb", "Mar", "Apr" },
            CurrentMonthIndex = currentMonthIndex,
            MaxItemsPerCell = maxItems,
            Rows = new[]
            {
                new HeatmapRow
                {
                    Category = HeatmapCategory.Shipped,
                    Cells = shippedCells ?? new IReadOnlyList<string>[]
                    {
                        new[] { "A" }, Array.Empty<string>(),
                        Array.Empty<string>(), Array.Empty<string>()
                    }
                },
                new HeatmapRow
                {
                    Category = HeatmapCategory.InProgress,
                    Cells = new IReadOnlyList<string>[]
                    {
                        Array.Empty<string>(), Array.Empty<string>(),
                        Array.Empty<string>(), Array.Empty<string>()
                    }
                },
                new HeatmapRow
                {
                    Category = HeatmapCategory.Carryover,
                    Cells = new IReadOnlyList<string>[]
                    {
                        Array.Empty<string>(), Array.Empty<string>(),
                        Array.Empty<string>(), Array.Empty<string>()
                    }
                },
                new HeatmapRow
                {
                    Category = HeatmapCategory.Blockers,
                    Cells = new IReadOnlyList<string>[]
                    {
                        Array.Empty<string>(), Array.Empty<string>(),
                        Array.Empty<string>(), Array.Empty<string>()
                    }
                }
            }
        };
    }

    [Fact]
    public void NoTruncation_When_Items_LessOrEqual_MaxItems()
    {
        var heatmap = CreateHeatmap(
            shippedCells: new IReadOnlyList<string>[]
            {
                new[] { "A", "B", "C" },
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
            },
            maxItems: 4);

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 1, 15));

        var shippedRow = vm.Rows.First(r => r.Category == HeatmapCategory.Shipped);
        shippedRow.Cells[0].Items.Should().HaveCount(3);
        shippedRow.Cells[0].OverflowCount.Should().Be(0);
        shippedRow.Cells[0].IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Truncation_When_Items_Exceed_MaxItems()
    {
        var heatmap = CreateHeatmap(
            shippedCells: new IReadOnlyList<string>[]
            {
                new[] { "A", "B", "C", "D", "E", "F", "G" },
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
            },
            maxItems: 4);

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 1, 15));

        var shippedRow = vm.Rows.First(r => r.Category == HeatmapCategory.Shipped);
        shippedRow.Cells[0].Items.Should().HaveCount(3); // maxItems - 1
        shippedRow.Cells[0].OverflowCount.Should().Be(4); // 7 - 3
    }

    [Fact]
    public void EmptyCell_Flagged_IsEmpty()
    {
        var heatmap = CreateHeatmap();
        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 1, 15));

        var shippedRow = vm.Rows.First(r => r.Category == HeatmapCategory.Shipped);
        shippedRow.Cells[1].IsEmpty.Should().BeTrue();
        shippedRow.Cells[1].Items.Should().BeEmpty();
    }

    [Fact]
    public void CurrentMonthIndex_AutoDetected_From_Today()
    {
        var heatmap = CreateHeatmap(currentMonthIndex: null);
        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 3, 15)); // March

        vm.CurrentMonthIndex.Should().Be(2); // "Mar" is index 2
    }

    [Fact]
    public void CurrentMonthIndex_Explicit_Override()
    {
        var heatmap = CreateHeatmap(currentMonthIndex: 1);
        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 3, 15));

        vm.CurrentMonthIndex.Should().Be(1);
    }

    [Fact]
    public void CurrentMonthIndex_Negative_When_NoMatch()
    {
        var heatmap = CreateHeatmap(currentMonthIndex: null);
        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 8, 15)); // August

        vm.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    public void Rows_Are_In_Fixed_Category_Order()
    {
        var heatmap = CreateHeatmap();
        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 1, 15));

        vm.Rows.Should().HaveCount(4);
        vm.Rows[0].Category.Should().Be(HeatmapCategory.Shipped);
        vm.Rows[1].Category.Should().Be(HeatmapCategory.InProgress);
        vm.Rows[2].Category.Should().Be(HeatmapCategory.Carryover);
        vm.Rows[3].Category.Should().Be(HeatmapCategory.Blockers);
    }

    [Fact]
    public void Truncation_At_MaxItems_Boundary()
    {
        var heatmap = CreateHeatmap(
            shippedCells: new IReadOnlyList<string>[]
            {
                new[] { "A", "B", "C", "D", "E" },
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
            },
            maxItems: 4);

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 1, 15));

        var cell = vm.Rows.First(r => r.Category == HeatmapCategory.Shipped).Cells[0];
        cell.Items.Should().HaveCount(3); // 4 - 1
        cell.OverflowCount.Should().Be(2); // 5 - 3
    }
}