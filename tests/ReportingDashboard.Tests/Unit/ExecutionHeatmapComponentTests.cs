using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class ExecutionHeatmapComponentTests : TestContext
{
    private static HeatmapConfig BuildHeatmap(string currentCol = "Apr") => new()
    {
        Columns = ["Jan", "Feb", "Mar", "Apr"],
        CurrentColumn = currentCol,
        Rows =
        [
            new HeatmapRow
            {
                Label = "Shipped", Type = HeatmapRowType.Shipped,
                Cells = [new HeatmapCell { Month = "Jan", Items = ["Feature A"] }]
            },
            new HeatmapRow { Label = "In Progress", Type = HeatmapRowType.InProgress, Cells = [] },
            new HeatmapRow { Label = "Carryover", Type = HeatmapRowType.Carryover, Cells = [] },
            new HeatmapRow { Label = "Blockers", Type = HeatmapRowType.Blocker, Cells = [] }
        ]
    };

    [Fact]
    public void Renders_HmTitleText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ExecutionHeatmap>(p => p
            .Add(x => x.Heatmap, BuildHeatmap())
            .Add(x => x.CurrentColumn, "Apr"));
        Assert.Contains("Monthly Execution Heatmap", cut.Markup);
    }

    [Fact]
    public void Renders_StatusCornerCell()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ExecutionHeatmap>(p => p
            .Add(x => x.Heatmap, BuildHeatmap())
            .Add(x => x.CurrentColumn, "Apr"));
        cut.Find(".hm-corner");
        Assert.Contains("STATUS", cut.Markup);
    }

    [Fact]
    public void CurrentColumn_HasAprHdrClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ExecutionHeatmap>(p => p
            .Add(x => x.Heatmap, BuildHeatmap())
            .Add(x => x.CurrentColumn, "Apr"));
        Assert.Contains("apr-hdr", cut.Markup);
    }

    [Fact]
    public void EmptyCell_RendersDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ExecutionHeatmap>(p => p
            .Add(x => x.Heatmap, BuildHeatmap())
            .Add(x => x.CurrentColumn, "Apr"));
        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public void CellWithItems_RendersItemText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.ExecutionHeatmap>(p => p
            .Add(x => x.Heatmap, BuildHeatmap())
            .Add(x => x.CurrentColumn, "Apr"));
        Assert.Contains("Feature A", cut.Markup);
    }
}