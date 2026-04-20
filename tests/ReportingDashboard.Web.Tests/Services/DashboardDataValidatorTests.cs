using System.Collections.Generic;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Services;

[Trait("Category", "Unit")]
public class DashboardDataValidatorTests
{
    private static DashboardData ValidData() => FakeDashboardDataService.SampleData();

    private static IReadOnlyList<IReadOnlyList<string>> Empty4() => new IReadOnlyList<string>[]
    {
        System.Array.Empty<string>(), System.Array.Empty<string>(),
        System.Array.Empty<string>(), System.Array.Empty<string>()
    };

    [Fact]
    public void Validate_OnValidData_ReturnsEmpty()
    {
        var errors = DashboardDataValidator.Validate(ValidData());
        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullData_ReturnsError()
    {
        DashboardDataValidator.Validate(null).Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankTitle_IsError(string title)
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = new Project { Title = title, Subtitle = d.Project.Subtitle },
            Timeline = d.Timeline,
            Heatmap = d.Heatmap
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("project.title"));
    }

    [Fact]
    public void Validate_InvalidBacklogUrl_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = new Project { Title = "T", Subtitle = "S", BacklogUrl = "not a url" },
            Timeline = d.Timeline,
            Heatmap = d.Heatmap
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("backlogUrl"));
    }

    [Fact]
    public void Validate_TimelineStartAfterEnd_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = new Timeline
            {
                Start = new DateOnly(2026, 6, 30),
                End = new DateOnly(2026, 1, 1),
                Lanes = d.Timeline.Lanes
            },
            Heatmap = d.Heatmap
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("timeline.start"));
    }

    [Theory]
    [InlineData("blue")]
    [InlineData("#FFF")]
    [InlineData("#12345G")]
    [InlineData("")]
    public void Validate_BadLaneColor_IsError(string color)
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = new Timeline
            {
                Start = d.Timeline.Start,
                End = d.Timeline.End,
                Lanes = new[]
                {
                    new TimelineLane { Id = "M1", Label = "L", Color = color, Milestones = System.Array.Empty<Milestone>() }
                }
            },
            Heatmap = d.Heatmap
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains(".color"));
    }

    [Fact]
    public void Validate_DuplicateLaneIds_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = new Timeline
            {
                Start = d.Timeline.Start,
                End = d.Timeline.End,
                Lanes = new[]
                {
                    new TimelineLane { Id = "X", Label = "A", Color = "#0078D4", Milestones = System.Array.Empty<Milestone>() },
                    new TimelineLane { Id = "X", Label = "B", Color = "#0078D4", Milestones = System.Array.Empty<Milestone>() }
                }
            },
            Heatmap = d.Heatmap
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("duplicated"));
    }

    [Fact]
    public void Validate_MilestoneOutsideRange_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = new Timeline
            {
                Start = new DateOnly(2026, 1, 1),
                End = new DateOnly(2026, 6, 30),
                Lanes = new[]
                {
                    new TimelineLane
                    {
                        Id = "M1", Label = "L", Color = "#0078D4",
                        Milestones = new[]
                        {
                            new Milestone { Date = new DateOnly(2025, 12, 1), Type = MilestoneType.Poc, Label = "early" }
                        }
                    }
                }
            },
            Heatmap = d.Heatmap
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("outside timeline range"));
    }

    [Fact]
    public void Validate_LaneCountOutOfRange_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = new Timeline
            {
                Start = d.Timeline.Start,
                End = d.Timeline.End,
                Lanes = System.Array.Empty<TimelineLane>()
            },
            Heatmap = d.Heatmap
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("1..6"));
    }

    [Fact]
    public void Validate_CurrentMonthIndexOutOfRange_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = d.Timeline,
            Heatmap = new Heatmap
            {
                Months = new[] { "Jan", "Feb", "Mar", "Apr" },
                CurrentMonthIndex = 9,
                Rows = d.Heatmap.Rows
            }
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("currentMonthIndex"));
    }

    [Fact]
    public void Validate_MissingCategory_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = d.Timeline,
            Heatmap = new Heatmap
            {
                Months = new[] { "Jan", "Feb", "Mar", "Apr" },
                Rows = new[]
                {
                    new HeatmapRow { Category = HeatmapCategory.Shipped,    Cells = Empty4() },
                    new HeatmapRow { Category = HeatmapCategory.InProgress, Cells = Empty4() },
                    new HeatmapRow { Category = HeatmapCategory.Carryover,  Cells = Empty4() }
                }
            }
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("Blockers"));
    }

    [Fact]
    public void Validate_CellCountMismatch_IsError()
    {
        var d = ValidData();
        var bad = new DashboardData
        {
            Project = d.Project,
            Timeline = d.Timeline,
            Heatmap = new Heatmap
            {
                Months = new[] { "Jan", "Feb", "Mar", "Apr" },
                Rows = new[]
                {
                    new HeatmapRow { Category = HeatmapCategory.Shipped,    Cells = new IReadOnlyList<string>[] { System.Array.Empty<string>() } },
                    new HeatmapRow { Category = HeatmapCategory.InProgress, Cells = Empty4() },
                    new HeatmapRow { Category = HeatmapCategory.Carryover,  Cells = Empty4() },
                    new HeatmapRow { Category = HeatmapCategory.Blockers,   Cells = Empty4() }
                }
            }
        };
        DashboardDataValidator.Validate(bad).Should().Contain(e => e.Contains("cells"));
    }
}
