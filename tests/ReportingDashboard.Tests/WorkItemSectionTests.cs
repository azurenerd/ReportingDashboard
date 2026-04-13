using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests;

public class WorkItemSectionTests : TestContext
{
    private static List<string> DefaultMonths => new() { "March", "April", "May", "June" };
    private const int DefaultActiveIndex = 1;

    private static List<WorkItem> CreateShippedItems() => new()
    {
        new WorkItem
        {
            Title = "API v2 Endpoint Migration",
            Description = "Migrated all endpoints.",
            Category = "shipped",
            Owner = "Jordan Kim",
            Priority = "High",
            Notes = "Zero downtime achieved"
        },
        new WorkItem
        {
            Title = "Auth Token Refresh Flow",
            Description = "Implemented silent token refresh.",
            Category = "shipped",
            Owner = "Priya Patel",
            Priority = "High"
        }
    };

    private static List<WorkItem> CreateInProgressItems() => new()
    {
        new WorkItem
        {
            Title = "Mobile Responsive Redesign",
            Category = "in-progress",
            Owner = "Sam Rivera",
            Priority = "High",
            Notes = "70% complete"
        }
    };

    private static List<WorkItem> CreateCarriedOverItems() => new()
    {
        new WorkItem
        {
            Title = "Legacy Report Export",
            Category = "carried-over",
            Owner = "Priya Patel",
            Priority = "Low",
            Notes = "Deprioritized for API migration"
        },
        new WorkItem
        {
            Title = "SSO Integration for Partner Portal",
            Category = "carried-over",
            Owner = "Alex Chen",
            Priority = "Medium"
        }
    };

    [Fact]
    public void Renders_RowHeader_WithCorrectCssClass()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, DefaultActiveIndex));

        var header = cut.Find(".hm-row-hdr");
        Assert.NotNull(header);
        Assert.Contains("ship-hdr", header.ClassName);
    }

    [Fact]
    public void RowHeader_DisplaysTitleAndIcon()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, DefaultActiveIndex));

        var header = cut.Find(".hm-row-hdr");
        Assert.Contains("✓", header.TextContent);
        Assert.Contains("Shipped", header.TextContent);
    }

    [Fact]
    public void Renders_CorrectNumberOfCells_MatchingMonthCount()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, DefaultActiveIndex));

        var cells = cut.FindAll(".hm-cell");
        Assert.Equal(4, cells.Count);
    }

    [Fact]
    public void ActiveMonth_Cell_HasActiveCssClass()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.ActiveCellCssClass, "hm-cell-active")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var cells = cut.FindAll(".hm-cell");
        Assert.Contains("hm-cell-active", cells[1].ClassName);
    }

    [Fact]
    public void ActiveMonth_Cell_ContainsAllItems()
    {
        var items = CreateShippedItems();
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, items)
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var cells = cut.FindAll(".hm-cell");
        var activeCell = cells[1];
        var itemElements = activeCell.QuerySelectorAll(".it:not(.hm-empty)");
        Assert.Equal(items.Count, itemElements.Length);
    }

    [Fact]
    public void Items_DisplayTitle_WithColoredDotIndicator()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var cells = cut.FindAll(".hm-cell");
        var activeCell = cells[1];
        var itemEls = activeCell.QuerySelectorAll(".it:not(.hm-empty)");

        Assert.Contains(itemEls, el => el.TextContent.Contains("API v2 Endpoint Migration"));
        Assert.Contains(itemEls, el => el.TextContent.Contains("Auth Token Refresh Flow"));
    }

    [Fact]
    public void NonActiveMonth_Cells_ShowEmptyDash()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var cells = cut.FindAll(".hm-cell");
        // Cells 0, 2, 3 are non-active and should have empty dash
        var emptyCell = cells[0];
        var emptyItems = emptyCell.QuerySelectorAll(".hm-empty");
        Assert.Equal(1, emptyItems.Length);
    }

    [Fact]
    public void EmptyItemList_ShowsDashInAllCells()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, new List<WorkItem>())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var cells = cut.FindAll(".hm-cell");
        foreach (var cell in cells)
        {
            var emptyItems = cell.QuerySelectorAll(".hm-empty");
            Assert.Equal(1, emptyItems.Length);
        }
    }

    [Fact]
    public void ShippedSection_UsesGreenAccentStyles()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var header = cut.Find(".hm-row-hdr");
        Assert.Contains("ship-hdr", header.ClassName);

        var cells = cut.FindAll(".hm-cell");
        Assert.All(cells, c => Assert.Contains("ship-cell", c.ClassName));
    }

    [Fact]
    public void InProgressSection_UsesBlueAccentStyles()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "In Progress")
            .Add(x => x.Icon, "●")
            .Add(x => x.HeaderCssClass, "prog-hdr")
            .Add(x => x.CellCssClass, "prog-cell")
            .Add(x => x.Items, CreateInProgressItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var header = cut.Find(".hm-row-hdr");
        Assert.Contains("prog-hdr", header.ClassName);

        var cells = cut.FindAll(".hm-cell");
        Assert.All(cells, c => Assert.Contains("prog-cell", c.ClassName));
    }

    [Fact]
    public void CarriedOverSection_UsesAmberAccentStyles()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Carryover")
            .Add(x => x.Icon, "↩")
            .Add(x => x.HeaderCssClass, "carry-hdr")
            .Add(x => x.CellCssClass, "carry-cell")
            .Add(x => x.Items, CreateCarriedOverItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var header = cut.Find(".hm-row-hdr");
        Assert.Contains("carry-hdr", header.ClassName);

        var cells = cut.FindAll(".hm-cell");
        Assert.All(cells, c => Assert.Contains("carry-cell", c.ClassName));
    }

    [Fact]
    public void LastCell_HasLastCellClass()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var cells = cut.FindAll(".hm-cell");
        var lastCell = cells[^1];
        Assert.Contains("hm-cell-last", lastCell.ClassName);
    }

    [Fact]
    public void MonthItemMap_DistributesItems_AcrossMonths()
    {
        var marchItems = new List<WorkItem>
        {
            new() { Title = "March Task", Category = "shipped", Owner = "A", Priority = "High" }
        };
        var aprilItems = new List<WorkItem>
        {
            new() { Title = "April Task 1", Category = "shipped", Owner = "B", Priority = "Medium" },
            new() { Title = "April Task 2", Category = "shipped", Owner = "C", Priority = "Low" }
        };
        var monthMap = new Dictionary<int, List<WorkItem>>
        {
            { 0, marchItems },
            { 1, aprilItems }
        };

        var allItems = marchItems.Concat(aprilItems).ToList();
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, allItems)
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1)
            .Add(x => x.MonthItemMap, monthMap));

        var cells = cut.FindAll(".hm-cell");

        // March cell (index 0) should have 1 item
        var marchCell = cells[0];
        var marchItemEls = marchCell.QuerySelectorAll(".it:not(.hm-empty)");
        Assert.Equal(1, marchItemEls.Length);
        Assert.Contains("March Task", marchItemEls[0].TextContent);

        // April cell (index 1) should have 2 items
        var aprilCell = cells[1];
        var aprilItemEls = aprilCell.QuerySelectorAll(".it:not(.hm-empty)");
        Assert.Equal(2, aprilItemEls.Length);
    }

    [Fact]
    public void SingleMonth_RendersSingleCell()
    {
        var months = new List<string> { "April" };
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, months)
            .Add(x => x.ActiveMonthIndex, 0));

        var cells = cut.FindAll(".hm-cell");
        Assert.Single(cells);
    }

    [Fact]
    public void NullItems_DoesNotRender()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.Items, (List<WorkItem>)null!)
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, 1));

        var headers = cut.FindAll(".hm-row-hdr");
        Assert.Empty(headers);
    }

    [Fact]
    public void ActiveMonthNegativeOne_NoActiveHighlight()
    {
        var cut = RenderComponent<WorkItemSection>(p => p
            .Add(x => x.Title, "Shipped")
            .Add(x => x.Icon, "✓")
            .Add(x => x.HeaderCssClass, "ship-hdr")
            .Add(x => x.CellCssClass, "ship-cell")
            .Add(x => x.ActiveCellCssClass, "hm-cell-active")
            .Add(x => x.Items, CreateShippedItems())
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.ActiveMonthIndex, -1));

        var cells = cut.FindAll(".hm-cell");
        Assert.All(cells, c => Assert.DoesNotContain("hm-cell-active", c.ClassName));

        // All cells should show empty dashes since no active month matches
        foreach (var cell in cells)
        {
            var emptyItems = cell.QuerySelectorAll(".hm-empty");
            Assert.Equal(1, emptyItems.Length);
        }
    }
}