using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Services;

[Trait("Category", "Unit")]
public class DashboardDataValidatorEdgeCaseTests
{
    private static Project MakeProject(string? backlogUrl = "https://example.com/b") => new()
    {
        Title = "T",
        Subtitle = "S",
        BacklogUrl = backlogUrl,
        BacklogLinkText = "ADO Backlog"
    };

    private static List<IReadOnlyList<string>> EmptyCells() => new()
    {
        new List<string>(), new List<string>(), new List<string>(), new List<string>()
    };

    private static Timeline MakeTimeline(IReadOnlyList<TimelineLane>? lanes = null) => new()
    {
        Start = new DateOnly(2026, 1, 1),
        End = new DateOnly(2026, 6, 30),
        Lanes = lanes ?? new List<TimelineLane>
        {
            new() { Id = "M1", Label = "L1", Color = "#0078D4", Milestones = new List<Milestone>() }
        }
    };

    private static Heatmap MakeHeatmap(int? currentMonthIndex = null) => new()
    {
        Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
        CurrentMonthIndex = currentMonthIndex,
        MaxItemsPerCell = 4,
        Rows = new List<HeatmapRow>
        {
            new() { Category = HeatmapCategory.Shipped,    Cells = EmptyCells() },
            new() { Category = HeatmapCategory.InProgress, Cells = EmptyCells() },
            new() { Category = HeatmapCategory.Carryover,  Cells = EmptyCells() },
            new() { Category = HeatmapCategory.Blockers,   Cells = EmptyCells() }
        }
    };

    private static DashboardData ValidBaseline() => new()
    {
        Project = MakeProject(),
        Timeline = MakeTimeline(),
        Heatmap = MakeHeatmap()
    };

    [Fact]
    public void Validate_Null_ReturnsDataIsNullError()
    {
        var errs = DashboardDataValidator.Validate(null);
        errs.Should().ContainSingle(e => e.Contains("data is null"));
    }

    [Fact]
    public void Validate_ValidBaseline_ReturnsEmpty()
    {
        DashboardDataValidator.Validate(ValidBaseline()).Should().BeEmpty();
    }

    [Fact]
    public void Validate_NonHttpBacklogUrl_ReportsError()
    {
        var d = new DashboardData
        {
            Project = MakeProject("not-a-url"),
            Timeline = MakeTimeline(),
            Heatmap = MakeHeatmap()
        };
        var errs = DashboardDataValidator.Validate(d);
        errs.Should().Contain(e => e.Contains("backlogUrl"));
    }

    [Fact]
    public void Validate_DuplicateLaneIds_ReportsError()
    {
        var lanes = new List<TimelineLane>
        {
            new() { Id = "M1", Label = "L1", Color = "#111111", Milestones = new List<Milestone>() },
            new() { Id = "M1", Label = "L2", Color = "#222222", Milestones = new List<Milestone>() }
        };
        var d = new DashboardData
        {
            Project = MakeProject(),
            Timeline = MakeTimeline(lanes),
            Heatmap = MakeHeatmap()
        };
        var errs = DashboardDataValidator.Validate(d);
        errs.Should().Contain(e => e.Contains("duplicated"));
    }

    [Fact]
    public void Validate_MilestoneOutsideRange_AndBadHex_AndCurrentMonthIndexOutOfBounds()
    {
        var lanes = new List<TimelineLane>
        {
            new()
            {
                Id = "M1", Label = "L1", Color = "#ZZZZZZ",
                Milestones = new List<Milestone>
                {
                    new() { Date = new DateOnly(2025, 12, 31), Type = MilestoneType.Poc, Label = "early" }
                }
            }
        };
        var d = new DashboardData
        {
            Project = MakeProject(),
            Timeline = MakeTimeline(lanes),
            Heatmap = MakeHeatmap(currentMonthIndex: 99)
        };

        var errs = DashboardDataValidator.Validate(d);
        errs.Should().Contain(e => e.Contains("color"));
        errs.Should().Contain(e => e.Contains("outside timeline range"));
        errs.Should().Contain(e => e.Contains("currentMonthIndex"));
    }
}