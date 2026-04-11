using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapTests : TestContext
{
    private HeatmapData CreateDefaultHeatmapData()
    {
        return new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new List<string> { "Feature A" },
                ["Feb"] = new List<string> { "Feature B" },
                ["Mar"] = new List<string> { "Feature C" },
                ["Apr"] = new List<string> { "Feature D" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["Jan"] = new List<string>(),
                ["Feb"] = new List<string>(),
                ["Mar"] = new List<string> { "Task X" },
                ["Apr"] = new List<string> { "Task Y", "Task Z" }
            },
            Carryover = new Dictionary<string, List<string>>
            {
                ["Jan"] = new List<string>(),
                ["Feb"] = new List<string> { "Old Item" },
                ["Mar"] = new List<string>(),
                ["Apr"] = new List<string>()
            },
            Blockers = new Dictionary<string, List<string>>
            {
                ["Jan"] = new List<string>(),
                ["Feb"] = new List<string>(),
                ["Mar"] = new List<string>(),
                ["Apr"] = new List<string> { "Blocker 1" }
            }
        };
    }

    private List<string> CreateDefaultMonths() => new() { "Jan", "Feb", "Mar", "Apr" };

    #region Section Title Tests

    [Fact]
    public void Render_WithDefaults_DisplaysSectionTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var titleDiv = cut.Find(".hm-title");
        titleDiv.Should().NotBeNull();
        titleDiv.TextContent.Should().Contain("MONTHLY EXECUTION HEATMAP");
        titleDiv.TextContent.Should().Contain("Shipped");
        titleDiv.TextContent.Should().Contain("In Progress");
        titleDiv.TextContent.Should().Contain("Carryover");
        titleDiv.TextContent.Should().Contain("Blockers");
    }

    [Fact]
    public void Render_SectionTitle_HasCorrectCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var titleDiv = cut.Find(".hm-title");
        titleDiv.ClassName.Should().Contain("hm-title");
    }

    #endregion

    #region Grid Container Tests

    [Fact]
    public void Render_WithFourMonths_GridHasCorrectColumnTemplate()
    {
        var months = CreateDefaultMonths();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-columns: 160px repeat(4, 1fr)");
    }

    [Fact]
    public void Render_WithFourMonths_GridHasCorrectRowTemplate()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-rows: 36px repeat(4, 1fr)");
    }

    [Fact]
    public void Render_WithSingleMonth_GridColumnsReflectOneMonth()
    {
        var months = new List<string> { "Apr" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-columns: 160px repeat(1, 1fr)");
    }

    [Fact]
    public void Render_WithTwelveMonths_GridColumnsReflectTwelveMonths()
    {
        var months = new List<string>
        {
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-columns: 160px repeat(12, 1fr)");
    }

    [Fact]
    public void Render_GridContainer_HasHmGridClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        grid.Should().NotBeNull();
    }

    #endregion

    #region Corner Cell Tests

    [Fact]
    public void Render_CornerCell_DisplaysStatusText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var corner = cut.Find(".hm-corner");
        corner.TextContent.Should().Contain("STATUS");
    }

    [Fact]
    public void Render_CornerCell_HasCorrectCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var corner = cut.Find(".hm-corner");
        corner.ClassName.Should().Contain("hm-corner");
    }

    #endregion

    #region Column Header Tests

    [Fact]
    public void Render_WithFourMonths_RendersFourColumnHeaders()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(4);
    }

    [Fact]
    public void Render_ColumnHeaders_DisplayMonthNames()
    {
        var months = CreateDefaultMonths();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers[0].TextContent.Should().Contain("Jan");
        headers[1].TextContent.Should().Contain("Feb");
        headers[2].TextContent.Should().Contain("Mar");
        headers[3].TextContent.Should().Contain("Apr");
    }

    [Fact]
    public void Render_CurrentMonth_HasCurrentMonthHighlightClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        var aprHeader = headers.First(h => h.TextContent.Contains("Apr"));
        aprHeader.ClassName.Should().Contain("cur-month-hdr");
    }

    [Fact]
    public void Render_CurrentMonth_DisplaysNowIndicator()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        var aprHeader = headers.First(h => h.TextContent.Contains("Apr"));
        aprHeader.TextContent.Should().Contain("Now");
    }

    [Fact]
    public void Render_NonCurrentMonth_DoesNotHaveHighlightClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        var janHeader = headers.First(h => h.TextContent.Contains("Jan"));
        janHeader.ClassName.Should().NotContain("cur-month-hdr");
    }

    [Fact]
    public void Render_NonCurrentMonth_DoesNotDisplayNowIndicator()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        var janHeader = headers.First(h => h.TextContent.Contains("Jan"));
        janHeader.TextContent.Should().NotContain("Now");
    }

    [Fact]
    public void Render_CurrentMonthNotInList_NoHeaderHighlighted()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Dec")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        foreach (var header in headers)
        {
            header.ClassName.Should().NotContain("cur-month-hdr");
        }
    }

    [Fact]
    public void Render_EmptyCurrentMonth_NoHeaderHighlighted()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        foreach (var header in headers)
        {
            header.ClassName.Should().NotContain("cur-month-hdr");
        }
    }

    [Fact]
    public void Render_WithSingleMonth_RendersOneColumnHeader()
    {
        var months = new List<string> { "Mar" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Mar")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(1);
        headers[0].TextContent.Should().Contain("Mar");
        headers[0].ClassName.Should().Contain("cur-month-hdr");
    }

    #endregion

    #region HeatmapRow Delegation Tests

    [Fact]
    public void Render_WithValidData_RendersFourHeatmapRows()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        cut.Markup.Should().Contain("shipped");
        cut.Markup.Should().Contain("prog");
        cut.Markup.Should().Contain("carry");
        cut.Markup.Should().Contain("block");
    }

    [Fact]
    public void Render_ShippedRow_HasCorrectCategoryAndLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        cut.Markup.Should().Contain("Shipped");
    }

    [Fact]
    public void Render_InProgressRow_HasCorrectCategoryAndLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        cut.Markup.Should().Contain("In Progress");
    }

    [Fact]
    public void Render_CarryoverRow_HasCorrectCategoryAndLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        cut.Markup.Should().Contain("Carryover");
    }

    [Fact]
    public void Render_BlockersRow_HasCorrectCategoryAndLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        cut.Markup.Should().Contain("Blockers");
    }

    #endregion

    #region Null/Empty HeatmapModel Tests

    [Fact]
    public void Render_NullHeatmapModel_DoesNotCrash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, null));

        cut.Find(".hm-title").Should().NotBeNull();
        cut.Find(".hm-grid").Should().NotBeNull();
        cut.Find(".hm-corner").Should().NotBeNull();
    }

    [Fact]
    public void Render_NullHeatmapModel_StillRendersColumnHeaders()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, null));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(4);
    }

    [Fact]
    public void Render_NullHeatmapModel_RowsGetEmptyDictionaries()
    {
        var exception = Record.Exception(() =>
        {
            RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
                .Add(x => x.Months, CreateDefaultMonths())
                .Add(x => x.CurrentMonth, "Apr")
                .Add(x => x.HeatmapModel, null));
        });

        exception.Should().BeNull();
    }

    #endregion

    #region Empty Months Tests

    [Fact]
    public void Render_EmptyMonthsList_RendersTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        cut.Find(".hm-title").Should().NotBeNull();
    }

    [Fact]
    public void Render_EmptyMonthsList_GridColumnsReflectZeroMonths()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("repeat(0, 1fr)");
    }

    [Fact]
    public void Render_EmptyMonthsList_NoColumnHeadersRendered()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(0);
    }

    #endregion

    #region Parameter Default Values Tests

    [Fact]
    public void Render_DefaultParameters_MonthsIsEmptyList()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>();

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(0);
    }

    [Fact]
    public void Render_DefaultParameters_CurrentMonthIsEmpty()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths()));

        var headers = cut.FindAll(".hm-col-hdr");
        foreach (var header in headers)
        {
            header.ClassName.Should().NotContain("cur-month-hdr");
        }
    }

    [Fact]
    public void Render_DefaultParameters_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            RenderComponent<ReportingDashboard.Components.Heatmap>();
        });

        exception.Should().BeNull();
    }

    #endregion

    #region Current Month Detection Tests

    [Fact]
    public void Render_CurrentMonthExactMatch_HighlightsCorrectHeader()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Feb")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        var febHeader = headers.First(h => h.TextContent.Contains("Feb"));
        febHeader.ClassName.Should().Contain("cur-month-hdr");

        var janHeader = headers.First(h => h.TextContent.Contains("Jan"));
        janHeader.ClassName.Should().NotContain("cur-month-hdr");
    }

    [Fact]
    public void Render_CurrentMonthIsFirstMonth_HighlightsFirstHeader()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers[0].ClassName.Should().Contain("cur-month-hdr");
    }

    [Fact]
    public void Render_CurrentMonthIsLastMonth_HighlightsLastHeader()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers[3].ClassName.Should().Contain("cur-month-hdr");
    }

    [Fact]
    public void Render_OnlyOneMonthHighlighted_WhenCurrentMonthSet()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Mar")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        var highlightedCount = headers.Count(h => h.ClassName?.Contains("cur-month-hdr") == true);
        highlightedCount.Should().Be(1);
    }

    #endregion

    #region Grid Structure Ordering Tests

    [Fact]
    public void Render_CornerCellAppearsBeforeHeaders_InMarkupOrder()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var gridMarkup = grid.InnerHtml;
        var cornerIndex = gridMarkup.IndexOf("hm-corner");
        var firstHeaderIndex = gridMarkup.IndexOf("hm-col-hdr");

        cornerIndex.Should().BeLessThan(firstHeaderIndex);
    }

    [Fact]
    public void Render_RowsAppearAfterHeaders_InMarkupOrder()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var gridMarkup = cut.Find(".hm-grid").InnerHtml;
        var lastHeaderIndex = gridMarkup.LastIndexOf("hm-col-hdr");
        var firstShippedIndex = gridMarkup.IndexOf("shipped");

        lastHeaderIndex.Should().BeLessThan(firstShippedIndex);
    }

    [Fact]
    public void Render_RowOrder_ShippedBeforeInProgress()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var markup = cut.Markup;
        var shippedIndex = markup.IndexOf("shipped");
        var progIndex = markup.IndexOf("prog");

        shippedIndex.Should().BeLessThan(progIndex);
    }

    [Fact]
    public void Render_RowOrder_InProgressBeforeCarryover()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var markup = cut.Markup;
        var progIndex = markup.IndexOf("prog");
        var carryIndex = markup.IndexOf("carry");

        progIndex.Should().BeLessThan(carryIndex);
    }

    [Fact]
    public void Render_RowOrder_CarryoverBeforeBlockers()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var markup = cut.Markup;
        var carryIndex = markup.IndexOf("carry");
        var blockIndex = markup.IndexOf("block");

        carryIndex.Should().BeLessThan(blockIndex);
    }

    #endregion

    #region Month Header Content Tests

    [Fact]
    public void Render_CustomMonthNames_DisplaysCorrectText()
    {
        var months = new List<string> { "Q1", "Q2", "Q3", "Q4" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Q2")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers[0].TextContent.Should().Contain("Q1");
        headers[1].TextContent.Should().Contain("Q2");
        headers[2].TextContent.Should().Contain("Q3");
        headers[3].TextContent.Should().Contain("Q4");
    }

    [Fact]
    public void Render_DuplicateMonthNames_RendersAllHeaders()
    {
        var months = new List<string> { "Jan", "Jan", "Feb", "Feb" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Mar")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(4);
    }

    #endregion

    #region Inline Style Tests

    [Fact]
    public void Render_GridStyle_ContainsBothColumnAndRowTemplates()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-columns");
        style.Should().Contain("grid-template-rows");
    }

    [Fact]
    public void Render_GridStyle_RowTemplateIsAlwaysFourDataRows()
    {
        var months = new List<string> { "Jan" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-rows: 36px repeat(4, 1fr)");
    }

    [Fact]
    public void Render_GridStyle_ColumnCountMatchesMonthsCount()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("repeat(6, 1fr)");
    }

    #endregion

    #region Parameter Binding Tests

    [Fact]
    public void Render_UpdateMonths_ReRendersWithNewMonths()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, new List<string> { "Jan", "Feb" })
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(2);

        cut.SetParametersAndRender(p => p
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar", "Apr", "May" }));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(5);
    }

    [Fact]
    public void Render_UpdateCurrentMonth_ChangesHighlighting()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Jan")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers[0].ClassName.Should().Contain("cur-month-hdr");
        headers[3].ClassName.Should().NotContain("cur-month-hdr");

        cut.SetParametersAndRender(p => p.Add(x => x.CurrentMonth, "Apr"));

        headers = cut.FindAll(".hm-col-hdr");
        headers[0].ClassName.Should().NotContain("cur-month-hdr");
        headers[3].ClassName.Should().Contain("cur-month-hdr");
    }

    #endregion

    #region HeatmapData With Empty Dictionaries Tests

    [Fact]
    public void Render_HeatmapDataWithEmptyDictionaries_DoesNotCrash()
    {
        var emptyData = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>(),
            InProgress = new Dictionary<string, List<string>>(),
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>()
        };

        var exception = Record.Exception(() =>
        {
            RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
                .Add(x => x.Months, CreateDefaultMonths())
                .Add(x => x.CurrentMonth, "Apr")
                .Add(x => x.HeatmapModel, emptyData));
        });

        exception.Should().BeNull();
    }

    [Fact]
    public void Render_HeatmapDataWithPartialNullProperties_UsesEmptyDefaults()
    {
        var partialData = new HeatmapData();

        var exception = Record.Exception(() =>
        {
            RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
                .Add(x => x.Months, CreateDefaultMonths())
                .Add(x => x.CurrentMonth, "Apr")
                .Add(x => x.HeatmapModel, partialData));
        });

        exception.Should().BeNull();
    }

    #endregion

    #region Title Content Verification Tests

    [Fact]
    public void Render_Title_ContainsMdashSeparator()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var titleDiv = cut.Find(".hm-title");
        var innerHtml = titleDiv.InnerHtml;
        var textContent = titleDiv.TextContent;
        // The mdash renders as &mdash; in HTML or as the Unicode em-dash character in text
        var hasMdash = innerHtml.Contains("&mdash;") || textContent.Contains("\u2014") || textContent.Contains("—");
        hasMdash.Should().BeTrue("Title should contain an em-dash separator");
    }

    [Fact]
    public void Render_Title_ContainsAllFourCategories()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, CreateDefaultMonths())
            .Add(x => x.CurrentMonth, "Apr")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var titleText = cut.Find(".hm-title").TextContent;
        titleText.Should().Contain("Shipped");
        titleText.Should().Contain("In Progress");
        titleText.Should().Contain("Carryover");
        titleText.Should().Contain("Blockers");
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void Render_LargeNumberOfMonths_HandlesCorrectly()
    {
        var months = Enumerable.Range(1, 24).Select(i => $"M{i}").ToList();
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "M12")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(24);

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("repeat(24, 1fr)");
    }

    [Fact]
    public void Render_MonthWithSpecialCharacters_DisplaysAsIs()
    {
        var months = new List<string> { "Jan '25", "Feb & Mar", "Q1/Q2" };
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "")
            .Add(x => x.HeatmapModel, CreateDefaultHeatmapData()));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Count.Should().Be(3);
    }

    #endregion
}