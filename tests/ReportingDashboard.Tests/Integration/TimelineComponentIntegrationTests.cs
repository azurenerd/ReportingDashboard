using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the Timeline component with realistic data scenarios.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineComponentIntegrationTests : TestContext
{
    private static TimelineData CreateRealisticTimeline() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-10",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Name = "M1", Label = "Chatbot & MS Role", Color = "#0078D4",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-02-15", Type = "poc", Label = "Feb 15" },
                    new() { Date = "2026-04-01", Type = "checkpoint", Label = "Apr 1" },
                    new() { Date = "2026-06-01", Type = "production", Label = "Jun 1" }
                }
            },
            new()
            {
                Name = "M2", Label = "PDS & Data Inventory", Color = "#00897B",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-03-01", Type = "poc", Label = "Mar 1" },
                    new() { Date = "2026-05-15", Type = "production", Label = "May 15" }
                }
            },
            new()
            {
                Name = "M3", Label = "Auto Review DFD", Color = "#546E7A",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-04-01", Type = "checkpoint", Label = "Apr 1" }
                }
            }
        }
    };

    [Fact]
    public void Timeline_WithRealisticData_RendersAllTracks()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("M3", cut.Markup);
        Assert.Contains("Chatbot", cut.Markup);
        Assert.Contains("PDS", cut.Markup);
        Assert.Contains("Auto Review", cut.Markup);
    }

    [Fact]
    public void Timeline_WithRealisticData_RendersSvgWithCorrectDimensions()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        var svg = cut.Find("svg");
        Assert.Equal("1560", svg.GetAttribute("width"));
        // 3 tracks × 56 = 168, but min is 185
        Assert.Equal("185", svg.GetAttribute("height"));
    }

    [Fact]
    public void Timeline_FourTracks_SvgHeightScales()
    {
        var tl = CreateRealisticTimeline();
        tl.Tracks.Add(new TimelineTrack
        {
            Name = "M4", Label = "Extra", Color = "#333",
            Milestones = new List<Milestone>()
        });

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        // 4 tracks × 56 = 224, which is > 185
        Assert.Equal("224", svg.GetAttribute("height"));
    }

    [Fact]
    public void Timeline_WithRealisticData_ContainsPocDiamonds()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("#F4B400", cut.Markup);
        Assert.Contains("polygon", cut.Markup);
    }

    [Fact]
    public void Timeline_WithRealisticData_ContainsProductionDiamonds()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("#34A853", cut.Markup);
    }

    [Fact]
    public void Timeline_WithRealisticData_ContainsCheckpointCircles()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("circle", cut.Markup);
    }

    [Fact]
    public void Timeline_WithRealisticData_RendersNowLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("NOW", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);
        Assert.Contains("stroke-dasharray=\"5,3\"", cut.Markup);
    }

    [Fact]
    public void Timeline_WithRealisticData_ContainsMonthLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("Jan", cut.Markup);
        Assert.Contains("Apr", cut.Markup);
        Assert.Contains("Jul", cut.Markup);
    }

    [Fact]
    public void Timeline_NoNowDate_NoNowLine()
    {
        var tl = CreateRealisticTimeline();
        tl.NowDate = "";

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.DoesNotContain("NOW", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneLabels_AreHtmlEncoded()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "T1", Label = "Test", Color = "#000",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-01", Type = "poc", Label = "Q1 <Release>" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("Q1 &lt;Release&gt;", cut.Markup);
        Assert.DoesNotContain("<Release>", cut.Markup);
    }

    [Fact]
    public void Timeline_TrackColorsAppliedToLines()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("#0078D4", cut.Markup);
        Assert.Contains("#00897B", cut.Markup);
        Assert.Contains("#546E7A", cut.Markup);
    }

    [Fact]
    public void Timeline_DropShadowFilterDefined()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticTimeline()));

        Assert.Contains("<filter id=\"sh\"", cut.Markup);
        Assert.Contains("feDropShadow", cut.Markup);
    }
}