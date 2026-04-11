using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class TimelineIntegrationTests : TestContext
{
    [Fact]
    public void Timeline_FullData_ShouldRenderLabelsAndSvgTogether()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Platform", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-15", Label = "PoC Done", Type = "poc" },
                        new() { Date = "2026-04-01", Label = "GA Release", Type = "production" }
                    }
                },
                new()
                {
                    Id = "M2", Name = "API", Color = "#34A853",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "API Check", Type = "checkpoint" }
                    }
                },
                new()
                {
                    Id = "M3", Name = "UX", Color = "#EA4335",
                    Milestones = new List<MilestoneMarker>()
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        // Labels section
        cut.FindAll(".tl-label").Should().HaveCount(3);
        cut.FindAll(".tl-id")[0].TextContent.Should().Be("M1");
        cut.FindAll(".tl-id")[1].TextContent.Should().Be("M2");
        cut.FindAll(".tl-id")[2].TextContent.Should().Be("M3");

        // SVG section
        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");

        // Milestones rendered
        cut.Markup.Should().Contain("PoC Done");
        cut.Markup.Should().Contain("GA Release");
        cut.Markup.Should().Contain("API Check");

        // Marker colors
        cut.Markup.Should().Contain("#F4B400"); // PoC gold
        cut.Markup.Should().Contain("#34A853"); // Production green

        // Checkpoint circle
        cut.FindAll("circle").Should().NotBeEmpty();

        // NOW line
        cut.Markup.Should().Contain("NOW");
        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void Timeline_TrackColors_ShouldApplyToLabelsAndSvgLines()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "T1", Name = "Red Track", Color = "#FF0000", Milestones = new() },
                new() { Id = "T2", Name = "Blue Track", Color = "#0000FF", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        // Label colors
        var ids = cut.FindAll(".tl-id");
        ids[0].GetAttribute("style").Should().Contain("#FF0000");
        ids[1].GetAttribute("style").Should().Contain("#0000FF");

        // SVG track line colors
        cut.Markup.Should().Contain("stroke=\"#FF0000\"");
        cut.Markup.Should().Contain("stroke=\"#0000FF\"");
    }

    [Fact]
    public void Timeline_NowDateAtBoundary_StartDate_ShouldRenderNowLine()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-01-01",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain("NOW");
    }

    [Fact]
    public void Timeline_NowDateAtBoundary_EndDate_ShouldRenderNowLine()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-06-30",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain("NOW");
    }

    [Fact]
    public void Timeline_NowDateBeforeStartDate_ShouldNotRenderNowLine()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2025-12-01",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        // SVG should still render
        cut.Find("svg").Should().NotBeNull();
        // But no "NOW" text for the now-line (month labels may contain "Now" text, but
        // the dashed line + NOW label at bottom should not appear)
        cut.Markup.Should().NotContain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void Timeline_MonthGridLines_ShouldSpanJanToJun()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain("Jan");
        cut.Markup.Should().Contain("Feb");
        cut.Markup.Should().Contain("Mar");
        cut.Markup.Should().Contain("Apr");
        cut.Markup.Should().Contain("May");
        cut.Markup.Should().Contain("Jun");
    }

    [Fact]
    public void Timeline_MultipleMilestoneTypes_ShouldRenderCorrectShapes()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Mixed", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-01", Label = "PoC", Type = "poc" },
                        new() { Date = "2026-03-01", Label = "Prod", Type = "production" },
                        new() { Date = "2026-04-01", Label = "Check", Type = "checkpoint" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        // Polygons for poc and production diamonds
        cut.FindAll("polygon").Should().HaveCount(2);
        // Circle for checkpoint
        cut.FindAll("circle").Should().HaveCount(1);

        // Verify titles on shapes
        var titles = cut.FindAll("title");
        titles.Select(t => t.TextContent).Should().Contain("PoC");
        titles.Select(t => t.TextContent).Should().Contain("Prod");
        titles.Select(t => t.TextContent).Should().Contain("Check");
    }

    [Fact]
    public void Timeline_SvgHeightScalesWithTrackCount()
    {
        // 1 track: height = 42 + 0*56 + 30 = 72
        var timeline1 = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-06-30", NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "T1", Name = "One", Color = "#000", Milestones = new() }
            }
        };

        var cut1 = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline1));
        cut1.Find("svg").GetAttribute("height").Should().Be("72");

        // 3 tracks: height = 42 + 2*56 + 30 = 184
        var timeline3 = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-06-30", NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "T1", Name = "One", Color = "#000", Milestones = new() },
                new() { Id = "T2", Name = "Two", Color = "#111", Milestones = new() },
                new() { Id = "T3", Name = "Three", Color = "#222", Milestones = new() }
            }
        };

        var cut3 = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline3));
        cut3.Find("svg").GetAttribute("height").Should().Be("184");
    }

    [Fact]
    public void Timeline_DropShadowFilter_ShouldBeDefined()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-06-30", NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Track", Color = "#000",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-01", Label = "Test", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain("filter id=\"shadow\"");
        cut.Markup.Should().Contain("feDropShadow");
        cut.Markup.Should().Contain("filter=\"url(#shadow)\"");
    }
}