using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class MilestoneTimelineComponentTests : TestContext
{
    private static Milestone CreateMilestone(
        string title = "Checkpoint Alpha",
        string status = "Upcoming",
        DateTime? targetDate = null,
        DateTime? completionDate = null)
    {
        return new Milestone
        {
            Title = title,
            Status = status,
            TargetDate = targetDate ?? new DateTime(2026, 6, 1),
            CompletionDate = completionDate
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EmptyMilestones_RendersContainerWithNoMarkers()
    {
        // Arrange & Act
        var cut = RenderComponent<MilestoneTimeline>(p =>
            p.Add(x => x.Milestones, new List<Milestone>()));

        // Assert - container renders
        cut.Find(".tl-area").Should().NotBeNull();
        cut.Find(".tl-sidebar").Should().NotBeNull();
        cut.Find(".tl-svg-box").Should().NotBeNull();

        // No milestone labels in sidebar
        var labels = cut.FindAll(".tl-label");
        labels.Should().BeEmpty();

        // SVG exists but has no polygon or circle markers for milestones
        var svg = cut.Find("svg");
        svg.Should().NotBeNull();
        var polygons = cut.FindAll("polygon");
        polygons.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Milestones_RenderInChronologicalOrder_InSidebar()
    {
        // Arrange - provide milestones out of order
        var milestones = new List<Milestone>
        {
            CreateMilestone("Late Checkpoint", "Upcoming", new DateTime(2026, 9, 1)),
            CreateMilestone("Early Checkpoint", "Completed", new DateTime(2026, 3, 1)),
            CreateMilestone("Middle Checkpoint", "In-Progress", new DateTime(2026, 6, 1))
        };

        // Act
        var cut = RenderComponent<MilestoneTimeline>(p =>
            p.Add(x => x.Milestones, milestones));

        // Assert - sidebar labels should be ordered chronologically
        var labelNames = cut.FindAll(".tl-label-name");
        labelNames.Should().HaveCount(3);
        labelNames[0].TextContent.Should().Be("Early Checkpoint");
        labelNames[1].TextContent.Should().Be("Middle Checkpoint");
        labelNames[2].TextContent.Should().Be("Late Checkpoint");

        // Verify M1, M2, M3 identifiers
        var labelIds = cut.FindAll(".tl-label-id");
        labelIds[0].TextContent.Should().Be("M1");
        labelIds[1].TextContent.Should().Be("M2");
        labelIds[2].TextContent.Should().Be("M3");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void StatusColors_AppliedCorrectly_ToSidebarLabels()
    {
        // Arrange
        var milestones = new List<Milestone>
        {
            CreateMilestone("Done Item", "Completed", new DateTime(2026, 3, 1)),
            CreateMilestone("Active Item", "In-Progress", new DateTime(2026, 6, 1)),
            CreateMilestone("Future Item", "Upcoming", new DateTime(2026, 9, 1))
        };

        // Act
        var cut = RenderComponent<MilestoneTimeline>(p =>
            p.Add(x => x.Milestones, milestones));

        // Assert - check inline style colors on tl-label divs
        var labels = cut.FindAll(".tl-label");
        labels.Should().HaveCount(3);

        // Completed => #34A853 (green)
        labels[0].GetAttribute("style").Should().Contain("#34A853");
        // In-Progress => #0078D4 (blue)
        labels[1].GetAttribute("style").Should().Contain("#0078D4");
        // Upcoming => #6c757d (gray)
        labels[2].GetAttribute("style").Should().Contain("#6c757d");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DiamondMarkers_RenderedForMajorMilestones()
    {
        // Arrange - titles containing "Release", "PoC", "Production" trigger diamond
        var milestones = new List<Milestone>
        {
            CreateMilestone("PoC Demo", "Completed", new DateTime(2026, 3, 15)),
            CreateMilestone("Production Release", "Upcoming", new DateTime(2026, 8, 1))
        };

        // Act
        var cut = RenderComponent<MilestoneTimeline>(p =>
            p.Add(x => x.Milestones, milestones));

        // Assert - diamond milestones render as <polygon> elements
        var polygons = cut.FindAll("polygon");
        polygons.Should().HaveCount(2, "both PoC and Production Release should be diamond markers");

        // No circle markers (r="7") for these milestones
        var circles = cut.FindAll("circle[r='7']");
        circles.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CircleMarkers_RenderedForCheckpointMilestones()
    {
        // Arrange - titles NOT containing release/launch/poc/etc get circle markers
        var milestones = new List<Milestone>
        {
            CreateMilestone("Sprint Review", "Completed", new DateTime(2026, 4, 1)),
            CreateMilestone("Design Approval", "In-Progress", new DateTime(2026, 5, 15))
        };

        // Act
        var cut = RenderComponent<MilestoneTimeline>(p =>
            p.Add(x => x.Milestones, milestones));

        // Assert - checkpoint milestones render as <circle> with r="7"
        var circles = cut.FindAll("circle[r='7']");
        circles.Should().HaveCount(2, "both checkpoints should be circle markers");

        // No polygon/diamond markers
        var polygons = cut.FindAll("polygon");
        polygons.Should().BeEmpty();
    }
}