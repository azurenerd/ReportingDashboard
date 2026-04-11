using Bunit;
using FluentAssertions;
using ReportingDashboard.Tests.Unit.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapRowTests : BunitTestBase
{
    [Fact]
    public void Render_WithCategoryLabel_RendersHeaderWithLabel()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "apr"));

        var header = cut.Find(".hm-row-hdr");
        header.TextContent.Should().Be("✅ SHIPPED");
    }

    [Fact]
    public void Render_ShipPrefix_HeaderHasShipHdrClass()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "apr"));

        var header = cut.Find(".hm-row-hdr");
        header.ClassList.Should().Contain("ship-hdr");
    }

    [Fact]
    public void Render_ProgPrefix_HeaderHasProgHdrClass()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🔄 IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "apr"));

        var header = cut.Find(".hm-row-hdr");
        header.ClassList.Should().Contain("prog-hdr");
    }

    [Fact]
    public void Render_CarryPrefix_HeaderHasCarryHdrClass()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "📦 CARRYOVER")
            .Add(x => x.CssPrefix, "carry")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "apr"));

        var header = cut.Find(".hm-row-hdr");
        header.ClassList.Should().Contain("carry-hdr");
    }

    [Fact]
    public void Render_BlockPrefix_HeaderHasBlockHdrClass()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🚫 BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "apr"));

        var header = cut.Find(".hm-row-hdr");
        header.ClassList.Should().Contain("block-hdr");
    }

    [Fact]
    public void Render_WithMonths_RendersCorrectNumberOfCells()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "A" } },
            { "feb", new List<string> { "B" } },
            { "mar", new List<string> { "C" } },
            { "apr", new List<string> { "D" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "apr"));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(4);
    }

    [Fact]
    public void Render_WithNullMonths_RendersOnlyHeader()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Months, (List<string>?)null)
            .Add(x => x.CurrentMonth, "apr"));

        cut.Find(".hm-row-hdr").Should().NotBeNull();
        cut.FindAll(".hm-cell").Should().BeEmpty();
    }

    [Fact]
    public void Render_WithEmptyMonths_RendersOnlyHeader()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "apr"));

        cut.Find(".hm-row-hdr").Should().NotBeNull();
        cut.FindAll(".hm-cell").Should().BeEmpty();
    }

    [Fact]
    public void Render_WithNullItems_AllCellsShowDash()
    {
        var months = new List<string> { "Jan", "Feb" };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, (Dictionary<string, List<string>>?)null)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "apr"));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(2);
        foreach (var cell in cells)
        {
            cell.InnerHtml.Should().Contain("-");
        }
    }

    [Fact]
    public void Render_MonthNotInItems_CellShowsDash()
    {
        var months = new List<string> { "Jan", "Feb" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Feature X" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "apr"));

        var cells = cut.FindAll(".hm-cell");
        cells.Should().HaveCount(2);

        // Jan has item
        cells[0].QuerySelectorAll(".it").Length.Should().Be(1);
        // Feb has no items, shows dash
        cells[1].InnerHtml.Should().Contain("-");
    }

    [Fact]
    public void Render_CaseInsensitiveMonthLookup_ResolvesDictionaryKey()
    {
        var months = new List<string> { "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "apr", new List<string> { "Shipped Item" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "apr"));

        var itDivs = cut.FindAll(".it");
        itDivs.Should().HaveCount(1);
        itDivs[0].TextContent.Should().Be("Shipped Item");
    }

    [Fact]
    public void Render_UpperCaseMonthKey_ResolvesViaToLowerInvariant()
    {
        var months = new List<string> { "APR" };
        var items = new Dictionary<string, List<string>>
        {
            { "apr", new List<string> { "Item" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "apr"));

        cut.FindAll(".it").Should().HaveCount(1);
    }

    [Fact]
    public void Render_CurrentMonthCell_HasAprClass()
    {
        var months = new List<string> { "Jan", "Apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "A" } },
            { "apr", new List<string> { "B" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        var cells = cut.FindAll(".hm-cell");
        // Jan cell should NOT have apr
        cells[0].ClassList.Should().NotContain("apr");
        // Apr cell should have apr
        cells[1].ClassList.Should().Contain("apr");
    }

    [Fact]
    public void Render_CurrentMonthCaseInsensitive_MatchesCorrectCell()
    {
        var months = new List<string> { "apr" };
        var items = new Dictionary<string, List<string>>
        {
            { "apr", new List<string> { "A" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "APR"));

        cut.Find(".hm-cell").ClassList.Should().Contain("apr");
    }

    [Fact]
    public void Render_CurrentMonthNotInMonths_NoCellHasAprClass()
    {
        var months = new List<string> { "Jan", "Feb" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "A" } },
            { "feb", new List<string> { "B" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
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

    [Fact]
    public void Render_CellsUseCssPrefixFromRow()
    {
        var months = new List<string> { "Jan" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "A" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🔄 IN PROGRESS")
            .Add(x => x.CssPrefix, "prog")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        cut.Find(".hm-cell").ClassList.Should().Contain("prog-cell");
    }

    [Fact]
    public void Render_MultipleCellsWithMultipleItems_EachCellRendersCorrectItems()
    {
        var months = new List<string> { "Jan", "Feb" };
        var items = new Dictionary<string, List<string>>
        {
            { "jan", new List<string> { "Item A", "Item B" } },
            { "feb", new List<string> { "Item C" } }
        };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, items)
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        var cells = cut.FindAll(".hm-cell");
        cells[0].QuerySelectorAll(".it").Length.Should().Be(2);
        cells[1].QuerySelectorAll(".it").Length.Should().Be(1);
    }

    [Fact]
    public void Render_CategoryKeyParameter_AcceptedWithoutError()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.CategoryKey, "shipped")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, "apr"));

        cut.Find(".hm-row-hdr").Should().NotBeNull();
    }

    [Fact]
    public void Render_HeaderAlwaysPresent_RegardlessOfMonths()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "🚫 BLOCKERS")
            .Add(x => x.CssPrefix, "block")
            .Add(x => x.Months, (List<string>?)null)
            .Add(x => x.CurrentMonth, ""));

        cut.FindAll(".hm-row-hdr").Should().HaveCount(1);
        cut.Find(".hm-row-hdr").TextContent.Should().Be("🚫 BLOCKERS");
        cut.Find(".hm-row-hdr").ClassList.Should().Contain("block-hdr");
    }

    [Fact]
    public void Render_EmptyCategoryLabel_RendersEmptyHeader()
    {
        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Months, new List<string>())
            .Add(x => x.CurrentMonth, ""));

        cut.Find(".hm-row-hdr").TextContent.Should().BeEmpty();
    }

    [Fact]
    public void Render_SixMonths_RendersAllSixCells()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        cut.FindAll(".hm-cell").Should().HaveCount(6);
    }

    [Fact]
    public void Render_TotalChildrenCount_IsHeaderPlusCells()
    {
        var months = new List<string> { "Jan", "Feb", "Mar" };

        var cut = Context.RenderComponent<ReportingDashboard.Components.HeatmapRow>(p => p
            .Add(x => x.CategoryLabel, "✅ SHIPPED")
            .Add(x => x.CssPrefix, "ship")
            .Add(x => x.Items, new Dictionary<string, List<string>>())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, ""));

        // 1 header + 3 cells = 4 top-level children
        cut.FindAll(".hm-row-hdr").Should().HaveCount(1);
        cut.FindAll(".hm-cell").Should().HaveCount(3);
    }
}