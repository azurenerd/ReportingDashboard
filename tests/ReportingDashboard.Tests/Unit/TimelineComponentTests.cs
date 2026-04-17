using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class TimelineComponentTests : TestContext
{
    private static List<Milestone> CreateSampleMilestones()
    {
        return new List<Milestone>
        {
            new Milestone
            {
                Id = "M1",
                Label = "Chatbot & MS Role",
                Color = "#0078D4",
                Events = new List<MilestoneEvent>
                {
                    new MilestoneEvent { Date = "2026-01-15", Type = "checkpoint", Label = "Jan 15 Kickoff" },
                    new MilestoneEvent { Date = "2026-03-01", Type = "poc", Label = "Mar 1 PoC" },
                    new MilestoneEvent { Date = "2026-05-15", Type = "production", Label = "May 15 Prod" }
                }
            },
            new Milestone
            {
                Id = "M2",
                Label = "PDS & Data Inventory",
                Color = "#00897B",
                Events = new List<MilestoneEvent>
                {
                    new MilestoneEvent { Date = "2026-02-11", Type = "checkpoint-small", Label = "Feb 11 Check" },
                    new MilestoneEvent { Date = "2026-03-15", Type = "checkpoint", Label = "Mar 15 Review" }
                }
            },
            new Milestone
            {
                Id = "M3",
                Label = "Auto Review DFD",
                Color = "#546E7A",
                Events = new List<MilestoneEvent>
                {
                    new MilestoneEvent { Date = "2026-04-01", Type = "production", Label = "Apr 1 Release" }
                }
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_WithMilestones_RendersSvgWithCorrectDimensions()
    {
        var milestones = CreateSampleMilestones();

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Timeline>(p => p
            .Add(x => x.Milestones, milestones)
            .Add(x => x.TimelineStart, new DateOnly(2026, 1, 1))
            .Add(x => x.TimelineEnd, new DateOnly(2026, 6, 30))
            .Add(x => x.CurrentDate, new DateOnly(2026, 4, 10)));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
        svg.GetAttribute("height").Should().Be("185");
        svg.GetAttribute("style").Should().Contain("overflow:visible");
        svg.GetAttribute("style").Should().Contain("display:block");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_WithMilestones_RendersSidebarWithMilestoneIdsAndLabels()
    {
        var milestones = CreateSampleMilestones();

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Timeline>(p => p
            .Add(x => x.Milestones, milestones)
            .Add(x => x.TimelineStart, new DateOnly(2026, 1, 1))
            .Add(x => x.TimelineEnd, new DateOnly(2026, 6, 30))
            .Add(x => x.CurrentDate, new DateOnly(2026, 4, 10)));

        var sidebar = cut.Find(".tl-sidebar");
        sidebar.Should().NotBeNull();

        var msIds = cut.FindAll(".tl-ms-id");
        msIds.Should().HaveCount(3);
        msIds[0].TextContent.Should().Be("M1");
        msIds[1].TextContent.Should().Be("M2");
        msIds[2].TextContent.Should().Be("M3");

        var msLabels = cut.FindAll(".tl-ms-label");
        msLabels[0].TextContent.Should().Be("Chatbot & MS Role");

        // Verify color styling on milestone IDs
        msIds[0].GetAttribute("style").Should().Contain("color:#0078D4");
        msIds[1].GetAttribute("style").Should().Contain("color:#00897B");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_WithMilestones_RendersCorrectEventMarkerTypes()
    {
        var milestones = CreateSampleMilestones();

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Timeline>(p => p
            .Add(x => x.Milestones, milestones)
            .Add(x => x.TimelineStart, new DateOnly(2026, 1, 1))
            .Add(x => x.TimelineEnd, new DateOnly(2026, 6, 30))
            .Add(x => x.CurrentDate, new DateOnly(2026, 4, 10)));

        var markup = cut.Markup;

        // Checkpoint circles: r="7" fill="white"
        var circles = cut.FindAll("circle");
        circles.Should().HaveCountGreaterOrEqualTo(2); // 1 checkpoint + 1 checkpoint-small + 1 checkpoint

        // PoC diamond: fill="#F4B400"
        var polygons = cut.FindAll("polygon");
        polygons.Should().HaveCountGreaterOrEqualTo(2); // 1 poc + 2 production

        // Verify poc diamond has correct fill
        markup.Should().Contain("fill=\"#F4B400\"");
        // Verify production diamond has correct fill
        markup.Should().Contain("fill=\"#34A853\"");
        // Verify checkpoint-small has fill="#999"
        markup.Should().Contain("fill=\"#999\"");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_WithEmptyMilestones_RendersOnlyTlAreaWrapper()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Shared.Timeline>(p => p
            .Add(x => x.Milestones, new List<Milestone>())
            .Add(x => x.TimelineStart, new DateOnly(2026, 1, 1))
            .Add(x => x.TimelineEnd, new DateOnly(2026, 6, 30))
            .Add(x => x.CurrentDate, new DateOnly(2026, 4, 10)));

        var tlArea = cut.Find(".tl-area");
        tlArea.Should().NotBeNull();

        // No SVG should be rendered
        cut.FindAll("svg").Should().BeEmpty();
        // No sidebar
        cut.FindAll(".tl-sidebar").Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersNowLineAndMonthGridlines()
    {
        var milestones = CreateSampleMilestones();

        var cut = RenderComponent<ReportingDashboard.Components.Shared.Timeline>(p => p
            .Add(x => x.Milestones, milestones)
            .Add(x => x.TimelineStart, new DateOnly(2026, 1, 1))
            .Add(x => x.TimelineEnd, new DateOnly(2026, 6, 30))
            .Add(x => x.CurrentDate, new DateOnly(2026, 4, 10)));

        var markup = cut.Markup;

        // NOW line: dashed red line
        markup.Should().Contain("stroke=\"#EA4335\"");
        markup.Should().Contain("stroke-dasharray=\"5,3\"");
        markup.Should().Contain("stroke-width=\"2\"");

        // NOW label
        markup.Should().Contain(">NOW</text>");

        // Month gridlines (Jan through Jun = 6 months)
        var gridLines = cut.FindAll("line[stroke='#bbb']");
        gridLines.Should().HaveCount(6); // Jan, Feb, Mar, Apr, May, Jun

        // Drop shadow filter
        markup.Should().Contain("filter id=\"sh\"");
        markup.Should().Contain("feDropShadow");
    }
}