using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying the data flow from TimelineData model
/// through the Timeline component, ensuring parameter binding and
/// dynamic rendering work correctly with various data shapes.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineDataFlowIntegrationTests : TestContext
{
    [Fact]
    public void Timeline_ParameterUpdate_ReRendersWithNewData()
    {
        var initialData = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track 1", Color = "#0078D4", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, initialData));

        cut.FindAll(".tl-label").Should().HaveCount(1);

        // Update with 3 tracks
        var updatedData = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track 1", Color = "#0078D4", Milestones = new() },
                new() { Id = "M2", Name = "Track 2", Color = "#00897B", Milestones = new() },
                new() { Id = "M3", Name = "Track 3", Color = "#546E7A", Milestones = new() }
            }
        };

        cut.SetParametersAndRender(p =>
            p.Add(x => x.TimelineModel, updatedData));

        cut.FindAll(".tl-label").Should().HaveCount(3);
    }

    [Fact]
    public void Timeline_ParameterChangedToNull_RendersEmpty()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#0078D4", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, data));

        cut.FindAll(".tl-label").Should().HaveCount(1);

        cut.SetParametersAndRender(p =>
            p.Add(x => x.TimelineModel, (TimelineData?)null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_AddMilestonesToExistingTrack_NewMarkersAppear()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Track 1", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "Initial", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, data));

        cut.FindAll("polygon").Should().HaveCount(1);

        // Add more milestones
        var updatedData = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Track 1", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "Initial", Type = "poc" },
                        new() { Date = "2026-05-01", Label = "GA", Type = "production" },
                        new() { Date = "2026-04-01", Label = "Check", Type = "checkpoint" }
                    }
                }
            }
        };

        cut.SetParametersAndRender(p =>
            p.Add(x => x.TimelineModel, updatedData));

        cut.FindAll("polygon").Should().HaveCount(2); // poc + production
        cut.FindAll("circle").Should().HaveCount(1); // checkpoint
    }

    [Fact]
    public void Timeline_ChangeNowDate_NowLineMovesOrDisappears()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#0078D4", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, data));

        cut.Markup.Should().Contain(">NOW<");

        // Move now date outside range
        data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2027-01-01",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#0078D4", Milestones = new() }
            }
        };

        cut.SetParametersAndRender(p =>
            p.Add(x => x.TimelineModel, data));

        cut.Markup.Should().NotContain(">NOW<");
    }

    [Fact]
    public void Timeline_ChangeDateRange_MonthLabelsUpdate()
    {
        var janToJun = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#0078D4", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, janToJun));

        cut.Markup.Should().Contain("Jan");
        cut.Markup.Should().Contain("Jun");

        // Switch to Jul-Dec
        var julToDec = new TimelineData
        {
            StartDate = "2026-07-01",
            EndDate = "2026-12-31",
            NowDate = "2026-09-15",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#0078D4", Milestones = new() }
            }
        };

        cut.SetParametersAndRender(p =>
            p.Add(x => x.TimelineModel, julToDec));

        cut.Markup.Should().Contain("Jul");
        cut.Markup.Should().Contain("Dec");
    }

    [Fact]
    public void Timeline_ChangeTrackColors_UpdatesInMarkup()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#FF0000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, data));

        cut.Markup.Should().Contain("#FF0000");

        var updatedData = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#00FF00", Milestones = new() }
            }
        };

        cut.SetParametersAndRender(p =>
            p.Add(x => x.TimelineModel, updatedData));

        cut.Markup.Should().Contain("#00FF00");
        cut.Markup.Should().NotContain("#FF0000");
    }

    [Fact]
    public void Timeline_FullDashboardData_TimelineExtractedCorrectly()
    {
        // Simulate what Dashboard.razor does: extract timeline from DashboardData
        var dashboardData = new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Q2 2026",
            BacklogLink = "https://example.com",
            CurrentMonth = "Apr",
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                NowDate = "2026-04-10",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Id = "M1", Name = "Chatbot", Color = "#0078D4",
                        Milestones = new List<MilestoneMarker>
                        {
                            new() { Date = "2026-03-26", Label = "PoC", Type = "poc" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData()
        };

        // Pass timeline from the model just like Dashboard.razor does
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, dashboardData.Timeline));

        cut.Find(".tl-id").TextContent.Should().Be("M1");
        cut.Find(".tl-name").TextContent.Should().Be("Chatbot");
        cut.FindAll("polygon").Should().HaveCount(1);
    }
}