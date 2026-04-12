using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class TimelineComponentTests : TestContext
{
    public TimelineComponentTests()
    {
        Services.AddScoped<TimelineCalculationService>();
    }

    [Fact]
    public void Timeline_WithValidMilestones_RendersSuccessfully()
    {
        var reportData = new ReportData
        {
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "M1",
                    Title = "Test Milestone",
                    Color = "#0078D4",
                    StartDate = new DateTime(2026, 1, 12),
                    Checkpoints = new List<Checkpoint>(),
                    PocMilestone = new MilestoneMarker
                    {
                        Date = new DateTime(2026, 3, 26),
                        Label = "PoC",
                        Color = "#F4B400"
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(parameters => parameters
            .Add(p => p.ReportData, reportData));

        Assert.NotNull(cut);
        var svg = cut.Find("svg");
        Assert.NotNull(svg);
    }

    [Fact]
    public void Timeline_WithEmptyMilestones_RendersEmptyState()
    {
        var reportData = new ReportData
        {
            Milestones = new List<Milestone>()
        };

        var cut = RenderComponent<Timeline>(parameters => parameters
            .Add(p => p.ReportData, reportData));

        var content = cut.Markup;
        Assert.Contains("No milestones to display", content);
    }

    [Fact]
    public void Timeline_WithNullMilestones_RendersEmptyState()
    {
        var reportData = new ReportData
        {
            Milestones = null
        };

        var cut = RenderComponent<Timeline>(parameters => parameters
            .Add(p => p.ReportData, reportData));

        var content = cut.Markup;
        Assert.Contains("No milestones to display", content);
    }

    [Fact]
    public void Timeline_SVG_ContainsMonthGridlines()
    {
        var reportData = new ReportData
        {
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "M1",
                    Title = "Test",
                    Color = "#0078D4",
                    StartDate = new DateTime(2026, 1, 12)
                }
            }
        };

        var cut = RenderComponent<Timeline>(parameters => parameters
            .Add(p => p.ReportData, reportData));

        var svgContent = cut.Find("svg").InnerHtml;
        Assert.Contains("Jan", svgContent);
        Assert.Contains("Feb", svgContent);
        Assert.Contains("Mar", svgContent);
        Assert.Contains("Apr", svgContent);
        Assert.Contains("May", svgContent);
        Assert.Contains("Jun", svgContent);
    }

    [Fact]
    public void Timeline_SVG_ContainsNowLine()
    {
        var reportData = new ReportData
        {
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "M1",
                    Title = "Test",
                    Color = "#0078D4",
                    StartDate = new DateTime(2026, 1, 12)
                }
            }
        };

        var cut = RenderComponent<Timeline>(parameters => parameters
            .Add(p => p.ReportData, reportData));

        var svgContent = cut.Find("svg").InnerHtml;
        Assert.Contains("NOW", svgContent);
        Assert.Contains("#EA4335", svgContent);
    }

    [Fact]
    public void Timeline_Legend_DisplaysMilestoneIds()
    {
        var reportData = new ReportData
        {
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "M1",
                    Title = "First Milestone",
                    Color = "#0078D4",
                    StartDate = new DateTime(2026, 1, 12)
                },
                new Milestone
                {
                    Id = "M2",
                    Title = "Second Milestone",
                    Color = "#00897B",
                    StartDate = new DateTime(2026, 2, 1)
                }
            }
        };

        var cut = RenderComponent<Timeline>(parameters => parameters
            .Add(p => p.ReportData, reportData));

        var markup = cut.Markup;
        Assert.Contains("M1", markup);
        Assert.Contains("M2", markup);
        Assert.Contains("First Milestone", markup);
        Assert.Contains("Second Milestone", markup);
    }
}