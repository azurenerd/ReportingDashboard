using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DataModelTests
{
    [Fact]
    public void DashboardConfigConstructor_InitializesQuartersAsEmptyList()
    {
        var config = new DashboardConfig();

        config.Quarters.Should().NotBeNull();
        config.Quarters.Should().BeEmpty();
    }

    [Fact]
    public void DashboardConfigConstructor_InitializesMilestonesAsEmptyList()
    {
        var config = new DashboardConfig();

        config.Milestones.Should().NotBeNull();
        config.Milestones.Should().BeEmpty();
    }

    [Fact]
    public void DashboardConfigConstructor_InitializesPropertiesToEmptyString()
    {
        var config = new DashboardConfig();

        config.ProjectName.Should().Be(string.Empty);
        config.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void QuarterConstructor_InitializesAllStatusArraysToEmptyLists()
    {
        var quarter = new Quarter();

        quarter.Shipped.Should().NotBeNull();
        quarter.InProgress.Should().NotBeNull();
        quarter.Carryover.Should().NotBeNull();
        quarter.Blockers.Should().NotBeNull();
        
        quarter.Shipped.Should().BeEmpty();
        quarter.InProgress.Should().BeEmpty();
        quarter.Carryover.Should().BeEmpty();
        quarter.Blockers.Should().BeEmpty();
    }

    [Fact]
    public void QuarterConstructor_InitializesMonthAndYearToDefaults()
    {
        var quarter = new Quarter();

        quarter.Month.Should().Be(string.Empty);
        quarter.Year.Should().Be(0);
    }

    [Fact]
    public void MilestoneTypeField_AcceptsPocString()
    {
        var milestone = new Milestone { Type = "poc" };

        milestone.Type.Should().Be("poc");
    }

    [Fact]
    public void MilestoneTypeField_AcceptsReleaseString()
    {
        var milestone = new Milestone { Type = "release" };

        milestone.Type.Should().Be("release");
    }

    [Fact]
    public void MilestoneTypeField_AcceptsCheckpointString()
    {
        var milestone = new Milestone { Type = "checkpoint" };

        milestone.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void MilestoneProperties_AreAssignable()
    {
        var milestone = new Milestone
        {
            Id = "m1",
            Label = "Test Milestone",
            Date = "2026-03-15",
            Type = "release"
        };

        milestone.Id.Should().Be("m1");
        milestone.Label.Should().Be("Test Milestone");
        milestone.Date.Should().Be("2026-03-15");
        milestone.Type.Should().Be("release");
    }

    [Fact]
    public void MonthInfoIsCurrentMonthFlag_CanBeSet()
    {
        var monthInfo = new MonthInfo { IsCurrentMonth = true };

        monthInfo.IsCurrentMonth.Should().BeTrue();
    }

    [Fact]
    public void MonthInfoIsCurrentMonthFlag_CanBeUnset()
    {
        var monthInfo = new MonthInfo { IsCurrentMonth = false };

        monthInfo.IsCurrentMonth.Should().BeFalse();
    }

    [Fact]
    public void MonthInfoProperties_AreAssignable()
    {
        var monthInfo = new MonthInfo
        {
            Name = "March",
            Year = 2026,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31),
            GridColumnIndex = 1,
            IsCurrentMonth = true
        };

        monthInfo.Name.Should().Be("March");
        monthInfo.Year.Should().Be(2026);
        monthInfo.StartDate.Should().Be(new DateTime(2026, 3, 1));
        monthInfo.EndDate.Should().Be(new DateTime(2026, 3, 31));
        monthInfo.GridColumnIndex.Should().Be(1);
        monthInfo.IsCurrentMonth.Should().BeTrue();
    }

    [Fact]
    public void MilestoneShapeInfoProperties_AreAssignable()
    {
        var shapeInfo = new MilestoneShapeInfo
        {
            Type = "poc",
            Shape = "diamond",
            Color = "#F4B400",
            Size = 12
        };

        shapeInfo.Type.Should().Be("poc");
        shapeInfo.Shape.Should().Be("diamond");
        shapeInfo.Color.Should().Be("#F4B400");
        shapeInfo.Size.Should().Be(12);
    }

    [Fact]
    public void QuarterCanAddMultipleItemsToStatusArrays()
    {
        var quarter = new Quarter();
        quarter.Shipped.Add("Item 1");
        quarter.Shipped.Add("Item 2");
        quarter.InProgress.Add("Task A");

        quarter.Shipped.Should().HaveCount(2);
        quarter.InProgress.Should().HaveCount(1);
    }

    [Fact]
    public void DashboardConfigCanContainMultipleQuartersAndMilestones()
    {
        var config = new DashboardConfig();
        config.Quarters.Add(new Quarter { Month = "March", Year = 2026 });
        config.Quarters.Add(new Quarter { Month = "April", Year = 2026 });
        config.Milestones.Add(new Milestone { Id = "m1", Type = "poc" });

        config.Quarters.Should().HaveCount(2);
        config.Milestones.Should().HaveCount(1);
    }

    [Fact]
    public void MonthInfoDefaultValues_InitializeCorrectly()
    {
        var monthInfo = new MonthInfo();

        monthInfo.Name.Should().Be(string.Empty);
        monthInfo.Year.Should().Be(0);
        monthInfo.GridColumnIndex.Should().Be(0);
        monthInfo.IsCurrentMonth.Should().BeFalse();
    }

    [Fact]
    public void MilestoneShapeInfoDefaultValues_InitializeCorrectly()
    {
        var shapeInfo = new MilestoneShapeInfo();

        shapeInfo.Type.Should().Be(string.Empty);
        shapeInfo.Shape.Should().Be(string.Empty);
        shapeInfo.Color.Should().Be(string.Empty);
        shapeInfo.Size.Should().Be(0);
    }
}