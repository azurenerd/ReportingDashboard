using Bunit;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying HeatmapRow correctly composes and delegates to HeatmapCell children,
/// testing the full parent-child rendering pipeline with real data dictionaries.
/// </summary>
[Trait("Category", "Integration")]
public class HeatmapRowCellIntegrationTests : IDisposable
{
    private readonly Bunit.TestContext _ctx;

    public HeatmapRowCellIntegrationTests()
    {
        _ctx = new Bunit.TestContext();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    // ──────────────────────────────────────────────
    // Full row rendering with real data
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_WithFullData_RendersHeaderAndAllCellsWithItems()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Auth module shipped", "API v2 released" } },
            { "feb", new List<string> { "Dashboard MVP" } },
            { "mar", new List<string>() },
            { "apr", new List<string> { "Heatmap component", "Timeline SVG", "Legend update" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        // Header rendered
        var header = cut.Find(".hm-row-hdr");
        header.TextContent.Should().Be("✅ SHIPPED");
        header.ClassList.Should().Contain("ship-hdr");

        // 4 cells rendered
        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(4);

        // Jan: 2 items
        cells[0].QuerySelectorAll(".it").Length.Should().Be(2);
        cells[0].ClassList.Should().Contain("ship-cell");
        cells[0].ClassList.Should().NotContain("apr");

        // Feb: 1 item
        cells[1].QuerySelectorAll(".it").Length.Should().Be(1);

        // Mar: empty list -> dash
        cells[2].InnerHtml.Should().Contain("-");
        cells[2].QuerySelectorAll(".it").Length.Should().Be(0);

        // Apr: 3 items + current month highlight
        cells[3].QuerySelectorAll(".it").Length.Should().Be(3);
        cells[3].ClassList.Should().Contain("apr");
    }

    [Fact]
    public void HeatmapRow_ShippedCategory_ProducesCorrectCssClassesEndToEnd()
    {
        var months = new List<string> { "Jan" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Feature X" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Jan"));

        cut.Find(".hm-row-hdr").ClassList.Should().Contain("ship-hdr");
        var cell = cut.Find(".hm-cell");
        cell.ClassList.Should().Contain("ship-cell");
        cell.ClassList.Should().Contain("apr");
        cell.QuerySelector(".it")!.TextContent.Should().Be("Feature X");
    }

    [Fact]
    public void HeatmapRow_InProgressCategory_ProducesCorrectCssClassesEndToEnd()
    {
        var months = new List<string> { "Feb" };
        var items = new Dictionary<string, List<string>>
        {
            { "feb", new List<string> { "Refactoring" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🔄 IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Feb"));

        cut.Find(".hm-row-hdr").ClassList.Should().Contain("prog-hdr");
        cut.Find(".hm-cell").ClassList.Should().Contain("prog-cell");
        cut.Find(".hm-cell").ClassList.Should().Contain("apr");
    }

    [Fact]
    public void HeatmapRow_CarryoverCategory_ProducesCorrectCssClassesEndToEnd()
    {
        var months = new List<string> { "Mar" };
        var items = new Dictionary<string, List<string>>
        {
            { "mar", new List<string> { "Tech debt" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "📦 CARRYOVER")
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Mar"));

        cut.Find(".hm-row-hdr").ClassList.Should().Contain("carry-hdr");
        cut.Find(".hm-cell").ClassList.Should().Contain("carry-cell");
        cut.Find(".hm-cell").ClassList.Should().Contain("apr");
    }

    [Fact]
    public void HeatmapRow_BlockersCategory_ProducesCorrectCssClassesEndToEnd()
    {
        var months = new List<string> { "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "apr", new List<string> { "Dependency issue" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🚫 BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        cut.Find(".hm-row-hdr").ClassList.Should().Contain("block-hdr");
        cut.Find(".hm-cell").ClassList.Should().Contain("block-cell");
        cut.Find(".hm-cell").ClassList.Should().Contain("apr");
    }

    // ──────────────────────────────────────────────
    // Dictionary lookup integration
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_MixedCaseMonthKeys_ResolvesItemsCorrectly()
    {
        var months = new List<string> { "JAN", "Feb", "mar", "APR" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Item A" } },
            { "feb", new List<string> { "Item B" } },
            { "mar", new List<string> { "Item C" } },
            { "apr", new List<string> { "Item D" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "apr"));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(4);

        // All four should have items (case-insensitive lookup via ToLowerInvariant)
        for (int i = 0; i < 4; i++)
        {
            cells[i].QuerySelectorAll(".it").Length.Should().Be(1,
                $"Cell {i} should have resolved its item via case-insensitive lookup");
        }
    }

    [Fact]
    public void HeatmapRow_MonthNotInDictionary_RendersEmptyCellWithDash()
    {
        var months = new List<string> { "Jan", "May" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Shipped feature" } }
            // "may" key intentionally missing
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(2);

        // Jan has item
        cells[0].QuerySelectorAll(".it").Length.Should().Be(1);
        // May has dash
        cells[1].InnerHtml.Should().Contain("-");
        cells[1].QuerySelectorAll(".it").Length.Should().Be(0);
    }

    // ──────────────────────────────────────────────
    // Current month highlight integration
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_OnlyMatchingMonthCell_GetsAprClass()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "A" } },
            { "feb", new List<string> { "B" } },
            { "mar", new List<string> { "C" } },
            { "apr", new List<string> { "D" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Mar"));

        var cells = cut.FindAll(".hm-cell");

        cells[0].ClassList.Should().NotContain("apr", "Jan is not current month");
        cells[1].ClassList.Should().NotContain("apr", "Feb is not current month");
        cells[2].ClassList.Should().Contain("apr", "Mar IS current month");
        cells[3].ClassList.Should().NotContain("apr", "Apr is not current month");
    }

    [Fact]
    public void HeatmapRow_CurrentMonthCaseInsensitive_HighlightsCorrectCell()
    {
        var months = new List<string> { "jan", "feb" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "X" } },
            { "feb", new List<string> { "Y" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "FEB"));

        var cells = cut.FindAll(".hm-cell");
        cells[0].ClassList.Should().NotContain("apr");
        cells[1].ClassList.Should().Contain("apr");
    }

    [Fact]
    public void HeatmapRow_CurrentMonthNotInMonthsList_NoCellHighlighted()
    {
        var months = new List<string> { "Jan", "Feb" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "A" } },
            { "feb", new List<string> { "B" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Dec"));

        var cells = cut.FindAll(".hm-cell");
        foreach (var cell in cells)
        {
            cell.ClassList.Should().NotContain("apr");
        }
    }

    // ──────────────────────────────────────────────
    // Null/empty data integration
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_NullItemsDictionary_AllCellsRenderDashes()
    {
        var months = new List<string> { "Jan", "Feb", "Mar" };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, (Dictionary<string, List<string>>?)null)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Feb"));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(3);

        foreach (var cell in cells)
        {
            cell.InnerHtml.Should().Contain("-");
            cell.QuerySelectorAll(".it").Length.Should().Be(0);
        }

        // Current month still gets highlight even with no data
        cells[1].ClassList.Should().Contain("apr");
    }

    [Fact]
    public void HeatmapRow_NullMonths_RendersOnlyHeader()
    {
        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🔄 IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, new Dictionary<string, List<string>> { { "jan", new List<string> { "X" } } })
            .Add(x => x.Months, (List<string>?)null)
            .Add(x => x.CurrentMonth, "Jan"));

        cut.Find(".hm-row-hdr").TextContent.Should().Be("🔄 IN PROGRESS");
        cut.FindAll(".hm-cell").Should().BeEmpty();
    }

    [Fact]
    public void HeatmapRow_EmptyMonths_RendersOnlyHeader()
    {
        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "📦 CARRYOVER")
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, ""));

        cut.Find(".hm-row-hdr").Should().NotBeNull();
        cut.FindAll(".hm-cell").Should().BeEmpty();
    }

    [Fact]
    public void HeatmapRow_EmptyDictionary_AllCellsRenderDashes()
    {
        var months = new List<string> { "Jan", "Feb" };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(2);
        foreach (var cell in cells)
        {
            cell.InnerHtml.Should().Contain("-");
        }
    }

    // ──────────────────────────────────────────────
    // Realistic four-row heatmap scenario
    // ──────────────────────────────────────────────

    [Fact]
    public void FourCategoryRows_RenderCompleteHeatmapGrid()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var currentMonth = "Apr";

        var shippedItems = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Auth module", "Login flow" } },
            { "feb", new List<string> { "API v2" } },
            { "mar", new List<string>() },
            { "apr", new List<string> { "Dashboard" } }
        };
        var progItems = new Dictionary<string, List<string>>
        {
            { "jan", new List<string>() },
            { "feb", new List<string>() },
            { "mar", new List<string> { "Heatmap" } },
            { "apr", new List<string> { "Timeline", "Legend" } }
        };
        var carryItems = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Tech debt" } },
            { "apr", new List<string> { "Perf fixes" } }
        };
        var blockItems = new Dictionary<string, List<string>>
        {
            { "apr", new List<string> { "Dependency blocked" } }
        };

        // Render all four rows
        var shipped = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED").Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, shippedItems).Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, currentMonth));

        var prog = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🔄 IN PROGRESS").Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, progItems).Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, currentMonth));

        var carry = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "📦 CARRYOVER").Add(x => x.CssPrefix, "carry")
            .Add(x => x.Items, carryItems).Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, currentMonth));

        var block = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🚫 BLOCKERS").Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, blockItems).Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, currentMonth));

        // Verify each row has correct header class
        shipped.Find(".hm-row-hdr").ClassList.Should().Contain("ship-hdr");
        prog.Find(".hm-row-hdr").ClassList.Should().Contain("prog-hdr");
        carry.Find(".hm-row-hdr").ClassList.Should().Contain("carry-hdr");
        block.Find(".hm-row-hdr").ClassList.Should().Contain("block-hdr");

        // Each row has 4 cells
        shipped.FindAll(".hm-cell").Should().HaveCount(4);
        prog.FindAll(".hm-cell").Should().HaveCount(4);
        carry.FindAll(".hm-cell").Should().HaveCount(4);
        block.FindAll(".hm-cell").Should().HaveCount(4);

        // Shipped row: Jan=2, Feb=1, Mar=dash, Apr=1+highlighted
        shipped.FindAll(".hm-cell")[0].QuerySelectorAll(".it").Length.Should().Be(2);
        shipped.FindAll(".hm-cell")[1].QuerySelectorAll(".it").Length.Should().Be(1);
        shipped.FindAll(".hm-cell")[2].InnerHtml.Should().Contain("-");
        shipped.FindAll(".hm-cell")[3].QuerySelectorAll(".it").Length.Should().Be(1);
        shipped.FindAll(".hm-cell")[3].ClassList.Should().Contain("apr");

        // In Progress row: Jan=dash, Feb=dash, Mar=1, Apr=2+highlighted
        prog.FindAll(".hm-cell")[0].InnerHtml.Should().Contain("-");
        prog.FindAll(".hm-cell")[1].InnerHtml.Should().Contain("-");
        prog.FindAll(".hm-cell")[2].QuerySelectorAll(".it").Length.Should().Be(1);
        prog.FindAll(".hm-cell")[3].QuerySelectorAll(".it").Length.Should().Be(2);
        prog.FindAll(".hm-cell")[3].ClassList.Should().Contain("apr");

        // Carryover: Jan=1, Feb=dash, Mar=dash, Apr=1+highlighted
        carry.FindAll(".hm-cell")[0].QuerySelectorAll(".it").Length.Should().Be(1);
        carry.FindAll(".hm-cell")[1].InnerHtml.Should().Contain("-");
        carry.FindAll(".hm-cell")[2].InnerHtml.Should().Contain("-");
        carry.FindAll(".hm-cell")[3].QuerySelectorAll(".it").Length.Should().Be(1);

        // Blockers: Jan-Mar=dash, Apr=1+highlighted
        block.FindAll(".hm-cell")[0].InnerHtml.Should().Contain("-");
        block.FindAll(".hm-cell")[1].InnerHtml.Should().Contain("-");
        block.FindAll(".hm-cell")[2].InnerHtml.Should().Contain("-");
        block.FindAll(".hm-cell")[3].QuerySelectorAll(".it").Length.Should().Be(1);
        block.FindAll(".hm-cell")[3].ClassList.Should().Contain("apr");
    }

    // ──────────────────────────────────────────────
    // Cell count = 1 header + N cells
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(6)]
    public void HeatmapRow_CellCount_MatchesMonthCount(int monthCount)
    {
        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var months = monthNames.Take(monthCount).ToList();

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        cut.FindAll(".hm-row-hdr").Should().HaveCount(1);
        cut.FindAll(".hm-cell").Should().HaveCount(monthCount);
    }

    // ──────────────────────────────────────────────
    // Item content fidelity through parent-child chain
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_ItemTextPreservedThroughCellRendering()
    {
        var months = new List<string> { "Jan" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Auth module v2.1 - JWT integration", "SSO rollout (Phase 1)" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var itDivs = cut.FindAll(".it");
        itDivs.Should().HaveCount(2);
        itDivs[0].TextContent.Should().Be("Auth module v2.1 - JWT integration");
        itDivs[1].TextContent.Should().Be("SSO rollout (Phase 1)");
    }

    [Fact]
    public void HeatmapRow_ManyItemsInCell_AllRenderedInOrder()
    {
        var expectedItems = Enumerable.Range(1, 8).Select(i => $"Work item #{i}").ToList();
        var months = new List<string> { "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "apr", expectedItems }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        var itDivs = cut.FindAll(".it");
        itDivs.Should().HaveCount(8);
        for (int i = 0; i < 8; i++)
        {
            itDivs[i].TextContent.Should().Be($"Work item #{i + 1}");
        }
    }

    // ──────────────────────────────────────────────
    // CategoryKey optional parameter
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_WithCategoryKey_RendersNormally()
    {
        var months = new List<string> { "Jan" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Item" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.CategoryKey, "shipped")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Jan"));

        cut.Find(".hm-row-hdr").TextContent.Should().Be("✅ SHIPPED");
        cut.FindAll(".hm-cell").Should().HaveCount(1);
        cut.Find(".it").TextContent.Should().Be("Item");
    }

    // ──────────────────────────────────────────────
    // Empty dash structure in cells
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_EmptyCells_DashHasCorrectInlineStyle()
    {
        var months = new List<string> { "Jan" };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var cell = cut.Find(".hm-cell");
        var dashDiv = cell.QuerySelector("div");
        dashDiv!.TextContent.Should().Be("-");
        dashDiv.GetAttribute("style").Should().Contain("color:#AAA");
        dashDiv.GetAttribute("style").Should().Contain("text-align:center");
    }

    // ──────────────────────────────────────────────
    // Sparse dictionary (some months present, some not)
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_SparseDictionary_MissingMonthsGetDashes()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        var items = new Dictionary<string, List<string>>
        {
            { "feb", new List<string> { "Feature B" } },
            { "may", new List<string> { "Feature E" } }
        };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🔄 IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(6);

        // Jan: dash
        cells[0].QuerySelectorAll(".it").Length.Should().Be(0);
        cells[0].InnerHtml.Should().Contain("-");

        // Feb: item
        cells[1].QuerySelectorAll(".it").Length.Should().Be(1);
        cells[1].QuerySelector(".it")!.TextContent.Should().Be("Feature B");

        // Mar: dash
        cells[2].InnerHtml.Should().Contain("-");

        // Apr: dash
        cells[3].InnerHtml.Should().Contain("-");

        // May: item
        cells[4].QuerySelectorAll(".it").Length.Should().Be(1);

        // Jun: dash
        cells[5].InnerHtml.Should().Contain("-");
    }

    // ──────────────────────────────────────────────
    // Current month highlight with null items
    // ──────────────────────────────────────────────

    [Fact]
    public void HeatmapRow_NullItems_CurrentMonthStillHighlighted()
    {
        var months = new List<string> { "Apr" };

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🚫 BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Items, (Dictionary<string, List<string>>?)null)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        var cell = cut.Find(".hm-cell");
        cell.ClassList.Should().Contain("block-cell");
        cell.ClassList.Should().Contain("apr");
        cell.InnerHtml.Should().Contain("-");
    }
}