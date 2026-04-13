using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests;

public class MilestoneTimelineTests : TestContext
{
    private static List<Milestone> CreateSampleMilestones() => new()
    {
        new Milestone
        {
            Title = "Architecture Approval",
            TargetDate = new DateTime(2026, 1, 15),
            CompletionDate = new DateTime(2026, 1, 12),
            Status = "completed"
        },
        new Milestone
        {
            Title = "API v2 Migration",
            TargetDate = new DateTime(2026, 2, 28),
            CompletionDate = new DateTime(2026, 2, 24),
            Status = "completed"
        },
        new Milestone
        {
            Title = "Beta Launch",
            TargetDate = new DateTime(2026, 3, 15),
            CompletionDate = new DateTime(2026, 3, 15),
            Status = "completed"
        },
        new Milestone
        {
            Title = "Performance Audit",
            TargetDate = new DateTime(2026, 4, 1),
            CompletionDate = null,
            Status = "in-progress"
        },
        new Milestone
        {
            Title = "GA Release",
            TargetDate = new DateTime(2026, 4, 30),
            CompletionDate = null,
            Status = "upcoming"
        },
        new Milestone
        {
            Title = "Post-Launch Review",
            TargetDate = new DateTime(2026, 5, 15),
            CompletionDate = null,
            Status = "upcoming"
        }
    };

    [Fact]
    public void Renders_TimelineArea_Container()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var tlArea = cut.Find(".tl-area");
        Assert.NotNull(tlArea);
    }

    [Fact]
    public void Renders_LeftSidebar_With_MilestoneLabels()
    {
        var milestones = CreateSampleMilestones();
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        var labels = cut.FindAll(".tl-label");
        Assert.Equal(milestones.Count, labels.Count);
    }

    [Fact]
    public void Renders_Milestone_Names_In_Sidebar()
    {
        var milestones = CreateSampleMilestones();
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        var labelNames = cut.FindAll(".tl-label-name");
        Assert.Contains(labelNames, l => l.TextContent.Contains("Architecture Approval"));
        Assert.Contains(labelNames, l => l.TextContent.Contains("GA Release"));
    }

    [Fact]
    public void Renders_Milestone_Identifiers_M1_Through_MN()
    {
        var milestones = CreateSampleMilestones();
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        var labelIds = cut.FindAll(".tl-label-id");
        Assert.Equal("M1", labelIds[0].TextContent);
        Assert.Equal("M6", labelIds[5].TextContent);
    }

    [Fact]
    public void Renders_SVG_Element()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svg = cut.Find("svg");
        Assert.NotNull(svg);
    }

    [Fact]
    public void Renders_Horizontal_Lines_For_Each_Milestone()
    {
        var milestones = CreateSampleMilestones();
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        // Each milestone gets a horizontal connecting line (stroke-width="3")
        var svg = cut.Find("svg");
        var lines = svg.QuerySelectorAll("line[stroke-width='3']");
        Assert.Equal(milestones.Count, lines.Length);
    }

    [Fact]
    public void Renders_CurrentDate_Indicator_AsRedDashedLine()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svg = cut.Find("svg");
        var redLine = svg.QuerySelector("line[stroke='#EA4335']");
        Assert.NotNull(redLine);
        Assert.Equal("2", redLine!.GetAttribute("stroke-width"));
        Assert.Equal("5,3", redLine.GetAttribute("stroke-dasharray"));
    }

    [Fact]
    public void Renders_NowLabel_Text()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svg = cut.Find("svg");
        var nowTexts = svg.QuerySelectorAll("text[fill='#EA4335']");
        Assert.True(nowTexts.Length >= 1);
        Assert.Contains(nowTexts, t => t.TextContent == "NOW");
    }

    [Fact]
    public void Renders_Diamond_Markers_For_Release_Milestones()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        // "GA Release" and "Beta Launch" should render as diamonds (polygons)
        var svg = cut.Find("svg");
        var diamonds = svg.QuerySelectorAll("polygon");
        Assert.True(diamonds.Length >= 1, "Expected at least one diamond marker for release milestones");
    }

    [Fact]
    public void Renders_Circle_Markers_For_Checkpoint_Milestones()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        // Non-release milestones like "Architecture Approval" should render as circles
        var svg = cut.Find("svg");
        var circles = svg.QuerySelectorAll("circle");
        Assert.True(circles.Length >= 1, "Expected at least one circle marker for checkpoint milestones");
    }

    [Fact]
    public void Completed_Milestones_UseGreenColor()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svg = cut.Find("svg");
        // Completed milestones should have green (#34A853) horizontal lines
        var greenLines = svg.QuerySelectorAll("line[stroke='#34A853']");
        Assert.True(greenLines.Length >= 1, "Expected green lines for completed milestones");
    }

    [Fact]
    public void InProgress_Milestones_UseBlueColor()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svg = cut.Find("svg");
        var blueLines = svg.QuerySelectorAll("line[stroke='#0078D4']");
        Assert.True(blueLines.Length >= 1, "Expected blue lines for in-progress milestones");
    }

    [Fact]
    public void Upcoming_Milestones_UseGrayColor()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svg = cut.Find("svg");
        var grayLines = svg.QuerySelectorAll("line[stroke='#6c757d']");
        Assert.True(grayLines.Length >= 1, "Expected gray lines for upcoming milestones");
    }

    [Fact]
    public void Milestones_Are_Ordered_By_TargetDate()
    {
        var milestones = new List<Milestone>
        {
            new() { Title = "Third", TargetDate = new DateTime(2026, 3, 1), Status = "upcoming" },
            new() { Title = "First", TargetDate = new DateTime(2026, 1, 1), Status = "completed" },
            new() { Title = "Second", TargetDate = new DateTime(2026, 2, 1), Status = "in-progress" }
        };

        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        var labelNames = cut.FindAll(".tl-label-name");
        Assert.Equal("First", labelNames[0].TextContent);
        Assert.Equal("Second", labelNames[1].TextContent);
        Assert.Equal("Third", labelNames[2].TextContent);
    }

    [Fact]
    public void Handles_MinimumThreeMilestones()
    {
        var milestones = new List<Milestone>
        {
            new() { Title = "Start", TargetDate = new DateTime(2026, 1, 1), Status = "completed" },
            new() { Title = "Middle", TargetDate = new DateTime(2026, 3, 1), Status = "in-progress" },
            new() { Title = "End", TargetDate = new DateTime(2026, 5, 1), Status = "upcoming" }
        };

        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        var labels = cut.FindAll(".tl-label");
        Assert.Equal(3, labels.Count);
    }

    [Fact]
    public void Handles_EightMilestones()
    {
        var milestones = Enumerable.Range(1, 8).Select(i => new Milestone
        {
            Title = $"Milestone {i}",
            TargetDate = new DateTime(2026, 1, 1).AddMonths(i - 1),
            Status = i <= 3 ? "completed" : i <= 5 ? "in-progress" : "upcoming"
        }).ToList();

        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        var labels = cut.FindAll(".tl-label");
        Assert.Equal(8, labels.Count);
    }

    [Fact]
    public void Renders_MonthGridLines()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svg = cut.Find("svg");
        // Month grid lines have stroke-opacity="0.4"
        var gridLines = svg.QuerySelectorAll("line[stroke-opacity='0.4']");
        Assert.True(gridLines.Length >= 3, "Expected at least 3 month grid lines");
    }

    [Fact]
    public void Empty_Milestones_DoesNotThrow()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, new List<Milestone>()));

        var tlArea = cut.Find(".tl-area");
        Assert.NotNull(tlArea);
    }

    [Fact]
    public void Renders_SvgBox_Container()
    {
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, CreateSampleMilestones()));

        var svgBox = cut.Find(".tl-svg-box");
        Assert.NotNull(svgBox);
    }

    [Fact]
    public void Sidebar_Labels_HaveCorrectColorForStatus()
    {
        var milestones = CreateSampleMilestones();
        var cut = RenderComponent<MilestoneTimeline>(parameters =>
            parameters.Add(p => p.Milestones, milestones));

        var labels = cut.FindAll(".tl-label");

        // First 3 are completed (green), ordered by date
        Assert.Contains("color: #34A853", labels[0].GetAttribute("style")!);
        // Index 3 is in-progress (blue)
        Assert.Contains("color: #0078D4", labels[3].GetAttribute("style")!);
        // Index 4 is upcoming (gray)
        Assert.Contains("color: #6c757d", labels[4].GetAttribute("style")!);
    }
}