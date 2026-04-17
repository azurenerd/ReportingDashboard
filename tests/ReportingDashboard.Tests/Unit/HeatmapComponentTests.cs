using Bunit;
using Xunit;
using ReportingDashboard.Models;
using ReportingDashboard.Components.Shared;

namespace ReportingDashboard.Tests.Unit;

public class HeatmapComponentTests : TestContext
{
    private static List<HeatmapCategory> CreateSampleCategories()
    {
        return new List<HeatmapCategory>
        {
            new()
            {
                Name = "Shipped",
                ColorScheme = "green",
                Rows = new Dictionary<string, List<string>>
                {
                    ["Jan 2026"] = new() { "Item A", "Item B" },
                    ["Feb 2026"] = new() { "Item C" }
                }
            },
            new()
            {
                Name = "In Progress",
                ColorScheme = "blue",
                Rows = new Dictionary<string, List<string>>
                {
                    ["Jan 2026"] = new() { "Task 1" },
                    ["Feb 2026"] = new() { "Task 2", "Task 3" }
                }
            },
            new()
            {
                Name = "Carryover",
                ColorScheme = "amber",
                Rows = new Dictionary<string, List<string>>
                {
                    ["Jan 2026"] = new(),
                    ["Feb 2026"] = new() { "Carried item" }
                }
            },
            new()
            {
                Name = "Blockers",
                ColorScheme = "red",
                Rows = new Dictionary<string, List<string>>
                {
                    ["Jan 2026"] = new(),
                    ["Feb 2026"] = new()
                }
            }
        };
    }

    private static List<string> CreateSampleMonths() => new() { "Jan 2026", "Feb 2026" };

    [Fact]
    public void HeatmapGrid_RendersAllFourCategoryRows()
    {
        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, CreateSampleCategories())
            .Add(p => p.Months, CreateSampleMonths())
            .Add(p => p.CurrentMonthIndex, 1));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, rowHeaders.Count);
    }

    [Fact]
    public void HeatmapGrid_RendersCorrectCssClassesForRows()
    {
        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, CreateSampleCategories())
            .Add(p => p.Months, CreateSampleMonths())
            .Add(p => p.CurrentMonthIndex, 1));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Contains("ship-hdr", rowHeaders[0].ClassList);
        Assert.Contains("prog-hdr", rowHeaders[1].ClassList);
        Assert.Contains("carry-hdr", rowHeaders[2].ClassList);
        Assert.Contains("block-hdr", rowHeaders[3].ClassList);
    }

    [Fact]
    public void HeatmapGrid_GridColumnCountMatchesMonthsLength()
    {
        var months = CreateSampleMonths();
        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, CreateSampleCategories())
            .Add(p => p.Months, months)
            .Add(p => p.CurrentMonthIndex, 1));

        var colHeaders = cut.FindAll(".hm-col-hdr");
        Assert.Equal(months.Count, colHeaders.Count);
    }

    [Fact]
    public void HeatmapGrid_CurrentMonthHeaderHasCurrentMonthClass()
    {
        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, CreateSampleCategories())
            .Add(p => p.Months, CreateSampleMonths())
            .Add(p => p.CurrentMonthIndex, 1));

        var colHeaders = cut.FindAll(".hm-col-hdr");
        Assert.DoesNotContain("current-month", colHeaders[0].ClassList);
        Assert.Contains("current-month", colHeaders[1].ClassList);
    }

    [Fact]
    public void HeatmapGrid_RendersHeatmapTitle()
    {
        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, CreateSampleCategories())
            .Add(p => p.Months, CreateSampleMonths())
            .Add(p => p.CurrentMonthIndex, 0));

        var title = cut.Find(".hm-title");
        Assert.Contains("Monthly Execution Heatmap", title.TextContent);
    }

    [Fact]
    public void HeatmapGrid_RendersCornerCell()
    {
        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, CreateSampleCategories())
            .Add(p => p.Months, CreateSampleMonths())
            .Add(p => p.CurrentMonthIndex, 0));

        var corner = cut.Find(".hm-corner");
        Assert.Equal("Status", corner.TextContent);
    }

    [Fact]
    public void HeatmapGrid_RendersEmojiPrefixes()
    {
        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, CreateSampleCategories())
            .Add(p => p.Months, CreateSampleMonths())
            .Add(p => p.CurrentMonthIndex, 0));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        Assert.Contains("✓", rowHeaders[0].TextContent);
        Assert.Contains("●", rowHeaders[1].TextContent);
        Assert.Contains("↩", rowHeaders[2].TextContent);
        Assert.Contains("⚠", rowHeaders[3].TextContent);
    }

    [Fact]
    public void HeatmapCell_RendersPopulatedItems()
    {
        var items = new List<string> { "Item A", "Item B" };
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ColorScheme, "green")
            .Add(p => p.IsCurrentMonth, false));

        var itemElements = cut.FindAll(".it");
        Assert.Equal(2, itemElements.Count);
        Assert.Equal("Item A", itemElements[0].TextContent);
        Assert.Equal("Item B", itemElements[1].TextContent);
    }

    [Fact]
    public void HeatmapCell_RendersEmptyDashForEmptyItems()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string>())
            .Add(p => p.ColorScheme, "green")
            .Add(p => p.IsCurrentMonth, false));

        var itemElements = cut.FindAll(".it");
        Assert.Single(itemElements);
        Assert.Equal("—", itemElements[0].TextContent);
        Assert.Contains("color:#AAA", itemElements[0].GetAttribute("style") ?? "");
    }

    [Fact]
    public void HeatmapCell_RendersEmptyDashForNullItems()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, (List<string>?)null)
            .Add(p => p.ColorScheme, "blue")
            .Add(p => p.IsCurrentMonth, false));

        var itemElements = cut.FindAll(".it");
        Assert.Single(itemElements);
        Assert.Equal("—", itemElements[0].TextContent);
    }

    [Fact]
    public void HeatmapCell_CurrentMonthAppliesCorrectCssClass()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "Test" })
            .Add(p => p.ColorScheme, "green")
            .Add(p => p.IsCurrentMonth, true));

        var cell = cut.Find(".hm-cell");
        Assert.Contains("current-month", cell.ClassList);
        Assert.Contains("ship-cell", cell.ClassList);
    }

    [Fact]
    public void HeatmapCell_NonCurrentMonthDoesNotHaveCurrentMonthClass()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "Test" })
            .Add(p => p.ColorScheme, "blue")
            .Add(p => p.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        Assert.DoesNotContain("current-month", cell.ClassList);
        Assert.Contains("prog-cell", cell.ClassList);
    }

    [Theory]
    [InlineData("green", "ship-cell")]
    [InlineData("blue", "prog-cell")]
    [InlineData("amber", "carry-cell")]
    [InlineData("red", "block-cell")]
    public void HeatmapCell_ColorSchemeMapsToCorrectCssClass(string colorScheme, string expectedClass)
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "Item" })
            .Add(p => p.ColorScheme, colorScheme)
            .Add(p => p.IsCurrentMonth, false));

        var cell = cut.Find(".hm-cell");
        Assert.Contains(expectedClass, cell.ClassList);
    }

    [Fact]
    public void HeatmapGrid_VariableMonthCount_RendersCorrectColumns()
    {
        var months = new List<string> { "Mar 2026", "Apr 2026", "May 2026" };
        var categories = new List<HeatmapCategory>
        {
            new()
            {
                Name = "Shipped",
                ColorScheme = "green",
                Rows = new Dictionary<string, List<string>>
                {
                    ["Mar 2026"] = new() { "A" },
                    ["Apr 2026"] = new() { "B" },
                    ["May 2026"] = new()
                }
            }
        };

        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Categories, categories)
            .Add(p => p.Months, months)
            .Add(p => p.CurrentMonthIndex, 1));

        var colHeaders = cut.FindAll(".hm-col-hdr");
        Assert.Equal(3, colHeaders.Count);

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style") ?? "";
        Assert.Contains("repeat(3", style);
    }
}