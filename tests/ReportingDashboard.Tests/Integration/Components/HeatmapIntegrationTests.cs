using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests for the Heatmap component verifying it correctly composes
/// with child HeatmapRow components and renders the full grid structure.
/// </summary>
[Trait("Category", "Integration")]
public class HeatmapIntegrationTests : TestContext
{
    private static List<string> DefaultMonths => new() { "Jan", "Feb", "Mar", "Apr" };

    private static HeatmapData CreatePopulatedHeatmapData()
    {
        return new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new() { "Auth Module", "User API" },
                ["Feb"] = new() { "Dashboard v1" },
                ["Mar"] = new() { "Export Feature" },
                ["Apr"] = new() { "Heatmap Component" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["Jan"] = new(),
                ["Feb"] = new() { "Timeline Widget" },
                ["Mar"] = new() { "Search Module", "Filters" },
                ["Apr"] = new() { "Chart Rendering" }
            },
            Carryover = new Dictionary<string, List<string>>
            {
                ["Jan"] = new(),
                ["Feb"] = new(),
                ["Mar"] = new() { "Legacy Migration" },
                ["Apr"] = new() { "Data Cleanup" }
            },
            Blockers = new Dictionary<string, List<string>>
            {
                ["Jan"] = new(),
                ["Feb"] = new(),
                ["Mar"] = new(),
                ["Apr"] = new() { "Vendor Dependency" }
            }
        };
    }

    #region Full Component Tree Rendering

    [Fact]
    public void Heatmap_WithFullData_RendersCompleteTitleGridAndRows()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        // Title present
        cut.Find(".hm-title").Should().NotBeNull();

        // Grid present
        cut.Find(".hm-grid").Should().NotBeNull();

        // Corner cell present
        cut.Find(".hm-corner").TextContent.Should().Contain("STATUS");

        // 4 column headers
        cut.FindAll(".hm-col-hdr").Count.Should().Be(4);

        // All four category keywords appear in markup (from HeatmapRow child components)
        var markup = cut.Markup;
        markup.Should().Contain("shipped");
        markup.Should().Contain("prog");
        markup.Should().Contain("carry");
        markup.Should().Contain("block");
    }

    [Fact]
    public void Heatmap_WithFullData_GridContainsCorrectInlineStyle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var gridStyle = cut.Find(".hm-grid").GetAttribute("style");
        gridStyle.Should().Contain("grid-template-columns: 160px repeat(4, 1fr)");
        gridStyle.Should().Contain("grid-template-rows: 36px repeat(4, 1fr)");
    }

    #endregion

    #region Heatmap + HeatmapRow Integration

    [Fact]
    public void Heatmap_PassesShippedDataToHeatmapRow()
    {
        var data = CreatePopulatedHeatmapData();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, data));

        // The HeatmapRow for "shipped" should receive Items and render them
        var markup = cut.Markup;
        markup.Should().Contain("Shipped");
    }

    [Fact]
    public void Heatmap_PassesInProgressDataToHeatmapRow()
    {
        var data = CreatePopulatedHeatmapData();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, data));

        cut.Markup.Should().Contain("In Progress");
    }

    [Fact]
    public void Heatmap_PassesCarryoverDataToHeatmapRow()
    {
        var data = CreatePopulatedHeatmapData();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, data));

        cut.Markup.Should().Contain("Carryover");
    }

    [Fact]
    public void Heatmap_PassesBlockersDataToHeatmapRow()
    {
        var data = CreatePopulatedHeatmapData();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, data));

        cut.Markup.Should().Contain("Blockers");
    }

    [Fact]
    public void Heatmap_AllFourRowCategories_RenderInCorrectOrder()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var markup = cut.Markup;
        var shippedIdx = markup.IndexOf("shipped");
        var progIdx = markup.IndexOf("prog");
        var carryIdx = markup.IndexOf("carry");
        var blockIdx = markup.IndexOf("block");

        shippedIdx.Should().BeLessThan(progIdx, "Shipped should appear before In Progress");
        progIdx.Should().BeLessThan(carryIdx, "In Progress should appear before Carryover");
        carryIdx.Should().BeLessThan(blockIdx, "Carryover should appear before Blockers");
    }

    [Fact]
    public void Heatmap_NullHeatmapModel_RowsReceiveEmptyDictionaries()
    {
        // When HeatmapModel is null, the null-coalescing expressions provide empty dicts
        var exception = Record.Exception(() =>
        {
            var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
                .Add(x => x.Months, DefaultMonths)
                .Add(x => x.CurrentMonth, "Apr")
                .Add(x => x.HeatmapModel, null));

            // Component should still render grid structure and rows without crashing
            cut.Find(".hm-grid").Should().NotBeNull();
        });

        exception.Should().BeNull();
    }

    #endregion

    #region Column Header and Current Month Highlighting

    [Fact]
    public void Heatmap_CurrentMonthApr_OnlyAprHeaderHighlighted()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(4);

        // Jan, Feb, Mar should NOT be highlighted
        headers[0].ClassName.Should().NotContain("cur-month-hdr");
        headers[1].ClassName.Should().NotContain("cur-month-hdr");
        headers[2].ClassName.Should().NotContain("cur-month-hdr");

        // Apr SHOULD be highlighted
        headers[3].ClassName.Should().Contain("cur-month-hdr");
    }

    [Fact]
    public void Heatmap_CurrentMonthFeb_OnlyFebHeaderHighlighted()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Feb")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");

        headers[0].ClassName.Should().NotContain("cur-month-hdr");
        headers[1].ClassName.Should().Contain("cur-month-hdr");
        headers[2].ClassName.Should().NotContain("cur-month-hdr");
        headers[3].ClassName.Should().NotContain("cur-month-hdr");
    }

    [Fact]
    public void Heatmap_CurrentMonthNotInList_NoHeadersHighlighted()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Dec")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        foreach (var header in headers)
        {
            header.ClassName.Should().NotContain("cur-month-hdr");
        }
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_ShowsNowIndicator()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Mar")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        var marHeader = headers[2];
        marHeader.TextContent.Should().Contain("Now");
        marHeader.TextContent.Should().Contain("Mar");
    }

    [Fact]
    public void Heatmap_NonCurrentMonthHeaders_DoNotShowNowIndicator()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Mar")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers[0].TextContent.Should().NotContain("Now"); // Jan
        headers[1].TextContent.Should().NotContain("Now"); // Feb
        headers[3].TextContent.Should().NotContain("Now"); // Apr
    }

    #endregion

    #region Dynamic Month Count Integration

    [Fact]
    public void Heatmap_SingleMonth_GridAndHeadersRenderCorrectly()
    {
        var months = new List<string> { "Apr" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var gridStyle = cut.Find(".hm-grid").GetAttribute("style");
        gridStyle.Should().Contain("repeat(1, 1fr)");

        cut.FindAll(".hm-col-hdr").Count.Should().Be(1);
        cut.Find(".hm-corner").TextContent.Should().Contain("STATUS");
    }

    [Fact]
    public void Heatmap_SixMonths_GridAndHeadersRenderCorrectly()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var gridStyle = cut.Find(".hm-grid").GetAttribute("style");
        gridStyle.Should().Contain("repeat(6, 1fr)");

        cut.FindAll(".hm-col-hdr").Count.Should().Be(6);
    }

    [Fact]
    public void Heatmap_TwelveMonths_GridAndHeadersRenderCorrectly()
    {
        var months = new List<string>
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Jul")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var gridStyle = cut.Find(".hm-grid").GetAttribute("style");
        gridStyle.Should().Contain("repeat(12, 1fr)");

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(12);

        // Only Jul should be highlighted
        var highlightedHeaders = headers.Where(h => h.ClassName?.Contains("cur-month-hdr") == true).ToList();
        highlightedHeaders.Count.Should().Be(1);
        highlightedHeaders[0].TextContent.Should().Contain("Jul");
    }

    [Fact]
    public void Heatmap_EmptyMonths_RendersGridWithNoHeaders()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        cut.Find(".hm-title").Should().NotBeNull();
        cut.Find(".hm-grid").Should().NotBeNull();
        cut.FindAll(".hm-col-hdr").Count.Should().Be(0);
    }

    #endregion

    #region Parameter Re-rendering Integration

    [Fact]
    public void Heatmap_ChangeCurrentMonth_ReRendersHighlighting()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        // Initially Jan is highlighted
        var headers = cut.FindAll(".hm-col-hdr");
        headers[0].ClassName.Should().Contain("cur-month-hdr");
        headers[3].ClassName.Should().NotContain("cur-month-hdr");

        // Re-render with Apr as current
        cut.SetParametersAndRender(p => p.Add(x => x.CurrentMonth, "Apr"));

        headers = cut.FindAll(".hm-col-hdr");
        headers[0].ClassName.Should().NotContain("cur-month-hdr");
        headers[3].ClassName.Should().Contain("cur-month-hdr");
    }

    [Fact]
    public void Heatmap_ChangeMonthsList_ReRendersGrid()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(2);
        var gridStyle = cut.Find(".hm-grid").GetAttribute("style");
        gridStyle.Should().Contain("repeat(2, 1fr)");

        // Expand to 5 months
        cut.SetParametersAndRender(p => p
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr", "May" }));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(5);
        gridStyle = cut.Find(".hm-grid").GetAttribute("style");
        gridStyle.Should().Contain("repeat(5, 1fr)");
    }

    [Fact]
    public void Heatmap_ChangeHeatmapModel_ReRendersRows()
    {
        var initialData = CreatePopulatedHeatmapData();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, initialData));

        var markupBefore = cut.Markup;

        // Update with new data
        var updatedData = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Apr"] = new() { "New Feature X", "New Feature Y" }
            },
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        };

        cut.SetParametersAndRender(p => p.Add(x => x.HeatmapModel, updatedData));

        // Component should re-render without error
        cut.Find(".hm-grid").Should().NotBeNull();
    }

    [Fact]
    public void Heatmap_SetHeatmapModelToNull_GracefullyHandlesTransition()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        // Transition to null model
        var exception = Record.Exception(() =>
        {
            cut.SetParametersAndRender(p => p.Add(x => x.HeatmapModel, (HeatmapData?)null));
        });

        exception.Should().BeNull();
        cut.Find(".hm-grid").Should().NotBeNull();
    }

    #endregion

    #region Full Grid Structure Validation

    [Fact]
    public void Heatmap_FullGrid_CornerCellIsFirstChild()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var firstChild = grid.Children.First();
        firstChild.ClassName.Should().Contain("hm-corner");
    }

    [Fact]
    public void Heatmap_FullGrid_HeadersFollowCornerCell()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var gridMarkup = cut.Find(".hm-grid").InnerHtml;

        var cornerEnd = gridMarkup.IndexOf("</div>") + "</div>".Length;
        var afterCorner = gridMarkup.Substring(cornerEnd).TrimStart();
        afterCorner.Should().StartWith("<div class=\"hm-col-hdr");
    }

    [Fact]
    public void Heatmap_FullGrid_TitleAppearsBeforeGrid()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var markup = cut.Markup;
        var titleIdx = markup.IndexOf("hm-title");
        var gridIdx = markup.IndexOf("hm-grid");

        titleIdx.Should().BeLessThan(gridIdx, "Title should appear before the grid in DOM order");
    }

    [Fact]
    public void Heatmap_RowTemplateAlwaysHasFourDataRows_RegardlessOfMonthCount()
    {
        foreach (var monthCount in new[] { 1, 4, 6, 12 })
        {
            var months = Enumerable.Range(1, monthCount)
                .Select(i => new DateTime(2026, i, 1).ToString("MMM"))
                .ToList();

            var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
                .Add(x => x.Months, months)
                .Add(x => x.CurrentMonth, months[0])
                .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

            var gridStyle = cut.Find(".hm-grid").GetAttribute("style");
            gridStyle.Should().Contain("grid-template-rows: 36px repeat(4, 1fr)",
                $"Row template should always have 4 data rows for {monthCount} months");
        }
    }

    #endregion

    #region Title Content Integration

    [Fact]
    public void Heatmap_Title_ContainsAllRequiredCategoryNames()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var titleText = cut.Find(".hm-title").TextContent;
        titleText.Should().Contain("MONTHLY EXECUTION HEATMAP");
        titleText.Should().Contain("Shipped");
        titleText.Should().Contain("In Progress");
        titleText.Should().Contain("Carryover");
        titleText.Should().Contain("Blockers");
    }

    [Fact]
    public void Heatmap_Title_UsesCorrectSeparators()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var titleHtml = cut.Find(".hm-title").InnerHtml;
        // Should contain mdash entity or rendered em-dash
        (titleHtml.Contains("&mdash;") || titleHtml.Contains("\u2014")).Should().BeTrue(
            "Title should use em-dash separator");
    }

    #endregion

    #region Edge Case Integration

    [Fact]
    public void Heatmap_DefaultParameters_RendersWithoutCrash()
    {
        var exception = Record.Exception(() =>
        {
            RenderComponent<ReportingDashboard.Components.Heatmap>();
        });

        exception.Should().BeNull();
    }

    [Fact]
    public void Heatmap_EmptyMonthsAndNullModel_RendersMinimalStructure()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "")
            .Add(x => x.HeatmapModel, null));

        cut.Find(".hm-title").Should().NotBeNull();
        cut.Find(".hm-grid").Should().NotBeNull();
        cut.Find(".hm-corner").TextContent.Should().Contain("STATUS");
        cut.FindAll(".hm-col-hdr").Count.Should().Be(0);
    }

    [Fact]
    public void Heatmap_HeatmapDataWithMissingMonthKeys_DoesNotCrash()
    {
        // Data has keys that don't match Months list
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jul"] = new() { "Item A" } // Not in our Months
            },
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        };

        var exception = Record.Exception(() =>
        {
            RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
                .Add(x => x.Months, DefaultMonths)
                .Add(x => x.CurrentMonth, "Apr")
                .Add(x => x.HeatmapModel, data));
        });

        exception.Should().BeNull();
    }

    [Fact]
    public void Heatmap_LargeItemLists_RendersWithoutError()
    {
        var manyItems = Enumerable.Range(1, 50).Select(i => $"Work Item {i}").ToList();
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["Apr"] = manyItems },
            InProgress = new Dictionary<string, List<string>> { ["Apr"] = manyItems },
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        };

        var exception = Record.Exception(() =>
        {
            var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
                .Add(x => x.Months, DefaultMonths)
                .Add(x => x.CurrentMonth, "Apr")
                .Add(x => x.HeatmapModel, data));

            cut.Find(".hm-grid").Should().NotBeNull();
        });

        exception.Should().BeNull();
    }

    [Fact]
    public void Heatmap_MonthsWithSpecialCharacters_RendersCorrectly()
    {
        var months = new List<string> { "Q1 '26", "Q2 & Q3", "H1/H2" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Q1 '26")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(3);
        headers[0].ClassName.Should().Contain("cur-month-hdr");
    }

    #endregion

    #region Multiple Re-render Cycles

    [Fact]
    public void Heatmap_MultipleParameterUpdates_MaintainsConsistentState()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        // Cycle through multiple current months
        foreach (var month in DefaultMonths)
        {
            cut.SetParametersAndRender(p => p.Add(x => x.CurrentMonth, month));

            var headers = cut.FindAll(".hm-col-hdr");
            var highlightedCount = headers.Count(h => h.ClassName?.Contains("cur-month-hdr") == true);
            highlightedCount.Should().Be(1, $"Exactly one header should be highlighted when CurrentMonth={month}");

            var highlighted = headers.First(h => h.ClassName?.Contains("cur-month-hdr") == true);
            highlighted.TextContent.Should().Contain(month);
        }
    }

    [Fact]
    public void Heatmap_ShrinkAndExpandMonths_GridUpdatesCorrectly()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(4);

        // Shrink to 2
        cut.SetParametersAndRender(p => p
            .Add(x => x.Months, new List<string> { "Jan", "Feb" }));
        cut.FindAll(".hm-col-hdr").Count.Should().Be(2);
        cut.Find(".hm-grid").GetAttribute("style").Should().Contain("repeat(2, 1fr)");

        // Expand to 6
        cut.SetParametersAndRender(p => p
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" }));
        cut.FindAll(".hm-col-hdr").Count.Should().Be(6);
        cut.Find(".hm-grid").GetAttribute("style").Should().Contain("repeat(6, 1fr)");

        // Back to empty
        cut.SetParametersAndRender(p => p
            .Add(x => x.Months, new List<string>()));
        cut.FindAll(".hm-col-hdr").Count.Should().Be(0);
        cut.Find(".hm-grid").GetAttribute("style").Should().Contain("repeat(0, 1fr)");
    }

    #endregion

    #region Concurrent Data and Month Changes

    [Fact]
    public void Heatmap_SimultaneousMonthAndDataChange_RendersCorrectly()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, DefaultMonths)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreatePopulatedHeatmapData()));

        var newMonths = new List<string> { "May", "Jun", "Jul" };
        var newData = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["Jun"] = new() { "Shipped Item" } },
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        };

        cut.SetParametersAndRender(p => p
            .Add(x => x.Months, newMonths)
            .Add(x => x.CurrentMonth, "Jun")
            .Add(x => x.HeatmapModel, newData));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(3);
        cut.Find(".hm-grid").GetAttribute("style").Should().Contain("repeat(3, 1fr)");

        var headers = cut.FindAll(".hm-col-hdr");
        headers[0].TextContent.Should().Contain("May");
        headers[1].TextContent.Should().Contain("Jun");
        headers[1].ClassName.Should().Contain("cur-month-hdr");
        headers[2].TextContent.Should().Contain("Jul");
    }

    #endregion
}