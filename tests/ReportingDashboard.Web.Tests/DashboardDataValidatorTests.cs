using Xunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests;

public class DashboardDataValidatorTests
{
    private static DashboardData CreateValidData() => new()
    {
        Project = new Project
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com"
        },
        Timeline = new Timeline
        {
            Start = new DateOnly(2026, 1, 1),
            End = new DateOnly(2026, 6, 30),
            Lanes = new[]
            {
                new TimelineLane
                {
                    Id = "M1", Label = "Lane 1", Color = "#0078D4",
                    Milestones = new[]
                    {
                        new Milestone
                        {
                            Date = new DateOnly(2026, 3, 15),
                            Type = MilestoneType.Poc,
                            Label = "Test"
                        }
                    }
                }
            }
        },
        Heatmap = new Heatmap
        {
            Months = new[] { "Jan", "Feb", "Mar", "Apr" },
            MaxItemsPerCell = 4,
            Rows = new[]
            {
                new HeatmapRow { Category = HeatmapCategory.Shipped, Cells = EmptyCells() },
                new HeatmapRow { Category = HeatmapCategory.InProgress, Cells = EmptyCells() },
                new HeatmapRow { Category = HeatmapCategory.Carryover, Cells = EmptyCells() },
                new HeatmapRow { Category = HeatmapCategory.Blockers, Cells = EmptyCells() },
            }
        }
    };

    private static IReadOnlyList<IReadOnlyList<string>> EmptyCells() =>
        new IReadOnlyList<string>[]
        {
            Array.Empty<string>(), Array.Empty<string>(),
            Array.Empty<string>(), Array.Empty<string>()
        };

    [Fact]
    public void ValidData_ReturnsNoErrors()
    {
        var errors = DashboardDataValidator.Validate(CreateValidData());
        errors.Should().BeEmpty();
    }

    [Fact]
    public void EmptyTitle_ReturnsError()
    {
        var data = CreateValidData();
        var modified = new DashboardData
        {
            Project = new Project { Title = "", Subtitle = "Sub" },
            Timeline = data.Timeline,
            Heatmap = data.Heatmap
        };

        var errors = DashboardDataValidator.Validate(modified);
        errors.Should().ContainSingle(e => e.Contains("project.title"));
    }

    [Fact]
    public void InvalidBacklogUrl_ReturnsError()
    {
        var data = CreateValidData();
        var modified = new DashboardData
        {
            Project = new Project { Title = "Test", Subtitle = "Sub", BacklogUrl = "not-a-url" },
            Timeline = data.Timeline,
            Heatmap = data.Heatmap
        };

        var errors = DashboardDataValidator.Validate(modified);
        errors.Should().ContainSingle(e => e.Contains("backlogUrl"));
    }

    [Fact]
    public void StartAfterEnd_ReturnsError()
    {
        var data = CreateValidData();
        var modified = new DashboardData
        {
            Project = data.Project,
            Timeline = new Timeline
            {
                Start = new DateOnly(2026, 6, 30),
                End = new DateOnly(2026, 1, 1),
                Lanes = data.Timeline.Lanes
            },
            Heatmap = data.Heatmap
        };

        var errors = DashboardDataValidator.Validate(modified);
        errors.Should().Contain(e => e.Contains("timeline.start"));
    }

    [Fact]
    public void InvalidLaneColor_ReturnsError()
    {
        var data = CreateValidData();
        var modified = new DashboardData
        {
            Project = data.Project,
            Timeline = new Timeline
            {
                Start = data.Timeline.Start,
                End = data.Timeline.End,
                Lanes = new[]
                {
                    new TimelineLane
                    {
                        Id = "M1", Label = "Lane 1", Color = "red",
                        Milestones = Array.Empty<Milestone>()
                    }
                }
            },
            Heatmap = data.Heatmap
        };

        var errors = DashboardDataValidator.Validate(modified);
        errors.Should().Contain(e => e.Contains("#RRGGBB"));
    }

    [Fact]
    public void MilestoneOutOfRange_ReturnsError()
    {
        var data = CreateValidData();
        var modified = new DashboardData
        {
            Project = data.Project,
            Timeline = new Timeline
            {
                Start = data.Timeline.Start,
                End = data.Timeline.End,
                Lanes = new[]
                {
                    new TimelineLane
                    {
                        Id = "M1", Label = "Lane 1", Color = "#0078D4",
                        Milestones = new[]
                        {
                            new Milestone
                            {
                                Date = new DateOnly(2026, 8, 1),
                                Type = MilestoneType.Poc,
                                Label = "Out of range"
                            }
                        }
                    }
                }
            },
            Heatmap = data.Heatmap
        };

        var errors = DashboardDataValidator.Validate(modified);
        errors.Should().Contain(e => e.Contains("outside timeline range"));
    }

    [Fact]
    public void DuplicateLaneIds_ReturnsError()
    {
        var data = CreateValidData();
        var modified = new DashboardData
        {
            Project = data.Project,
            Timeline = new Timeline
            {
                Start = data.Timeline.Start,
                End = data.Timeline.End,
                Lanes = new[]
                {
                    new TimelineLane { Id = "M1", Label = "A", Color = "#000000", Milestones = Array.Empty<Milestone>() },
                    new TimelineLane { Id = "M1", Label = "B", Color = "#111111", Milestones = Array.Empty<Milestone>() }
                }
            },
            Heatmap = data.Heatmap
        };

        var errors = DashboardDataValidator.Validate(modified);
        errors.Should().Contain(e => e.Contains("Duplicate lane id"));
    }

    [Fact]
    public void MissingHeatmapCategory_ReturnsError()
    {
        var data = CreateValidData();
        var modified = new DashboardData
        {
            Project = data.Project,
            Timeline = data.Timeline,
            Heatmap = new Heatmap
            {
                Months = new[] { "Jan", "Feb", "Mar", "Apr" },
                MaxItemsPerCell = 4,
                Rows = new[]
                {
                    new HeatmapRow { Category = HeatmapCategory.Shipped, Cells = EmptyCells() },
                    new HeatmapRow { Category = HeatmapCategory.Shipped, Cells = EmptyCells() },
                    new HeatmapRow { Category = HeatmapCategory.Carryover, Cells = EmptyCells() },
                    new HeatmapRow { Category = HeatmapCategory.Blockers, Cells = EmptyCells() },
                }
            }
        };

        var errors = DashboardDataValidator.Validate(modified);
        errors.Should().Contain(e => e.Contains("Missing heatmap categories"));
    }
}