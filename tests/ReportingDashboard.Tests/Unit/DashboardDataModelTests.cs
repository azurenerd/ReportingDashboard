using FluentAssertions;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void DashboardReport_DefaultConstructor_HasDefaultValues()
    {
        // Act
        var report = new DashboardReport();

        // Assert
        report.Header.Should().NotBeNull();
        report.TimelineTracks.Should().NotBeNull().And.BeEmpty();
        report.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void HeaderInfo_DefaultConstructor_HasEmptyStringDefaults()
    {
        // Act
        var header = new HeaderInfo();

        // Assert
        header.Title.Should().Be("");
        header.Subtitle.Should().Be("");
        header.BacklogLink.Should().Be("#");
        header.ReportDate.Should().Be("");
        header.TimelineStartDate.Should().Be("");
        header.TimelineEndDate.Should().Be("");
        header.TimelineMonths.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineTrack_DefaultConstructor_HasEmptyValuesAndDefaultColor()
    {
        // Act
        var track = new TimelineTrack();

        // Assert
        track.Id.Should().Be("");
        track.Name.Should().Be("");
        track.Description.Should().Be("");
        track.Color.Should().Be("#999");
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultConstructor_HasDefaults()
    {
        // Act
        var milestone = new Milestone();

        // Assert
        milestone.Label.Should().Be("");
        milestone.Date.Should().Be("");
        milestone.Type.Should().Be("checkpoint");
        milestone.LabelPosition.Should().BeNull();
    }

    [Fact]
    public void HeatmapData_DefaultConstructor_HasEmptyDefaults()
    {
        // Act
        var heatmap = new HeatmapData();

        // Assert
        heatmap.Columns.Should().NotBeNull().And.BeEmpty();
        heatmap.HighlightColumnIndex.Should().Be(0);
        heatmap.Rows.Should().NotBeNull().And.BeEmpty();
    }
}