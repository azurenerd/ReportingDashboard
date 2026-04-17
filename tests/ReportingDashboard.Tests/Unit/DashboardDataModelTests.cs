using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void DashboardData_DefaultValues_AreEmptyAndSafe()
    {
        var data = new DashboardData();

        data.Title.Should().Be("");
        data.Subtitle.Should().Be("");
        data.BacklogUrl.Should().Be("");
        data.CurrentDate.Should().Be("");
        data.TimelineStartMonth.Should().Be("");
        data.TimelineEndMonth.Should().Be("");
        data.Milestones.Should().NotBeNull().And.BeEmpty();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void HeatmapData_DefaultValues_AreEmptyAndSafe()
    {
        var heatmap = new HeatmapData();

        heatmap.Months.Should().NotBeNull().And.BeEmpty();
        heatmap.CurrentMonthIndex.Should().Be(0);
        heatmap.Rows.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void MilestoneEvent_LabelPosition_IsNullableAndDefaultsToNull()
    {
        var evt = new MilestoneEvent();

        evt.LabelPosition.Should().BeNull();
        evt.Date.Should().Be("");
        evt.Type.Should().Be("");
        evt.Label.Should().Be("");
    }

    [Fact]
    public void HeatmapRow_Items_DefaultsToEmptyDictionary()
    {
        var row = new HeatmapRow();

        row.Category.Should().Be("");
        row.Items.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DashboardData_InitProperties_CanBeSet()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Milestones = new List<Milestone>
            {
                new Milestone { Id = "M1", Label = "Alpha", Color = "#FF0000" }
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "Jan", "Feb" },
                CurrentMonthIndex = 1
            }
        };

        data.Title.Should().Be("Test Project");
        data.Milestones.Should().HaveCount(1);
        data.Milestones[0].Id.Should().Be("M1");
        data.Heatmap.Months.Should().HaveCount(2);
        data.Heatmap.CurrentMonthIndex.Should().Be(1);
    }
}