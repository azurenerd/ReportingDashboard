using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void DashboardData_DefaultConstructor_InitializesWithEmptyCollections()
    {
        var data = new DashboardData();

        data.Project.Should().NotBeNull();
        data.Project.Name.Should().BeEmpty();
        data.Project.RagStatus.Should().Be("Green");
        data.Milestones.Should().NotBeNull().And.BeEmpty();
        data.WorkItems.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultConstructor_InitializesWithDefaults()
    {
        var milestone = new Milestone();

        milestone.Title.Should().BeEmpty();
        milestone.TargetDate.Should().Be(default(DateTime));
        milestone.CompletionDate.Should().BeNull();
        milestone.Status.Should().Be("upcoming");
    }

    [Fact]
    public void WorkItem_DefaultConstructor_InitializesWithDefaults()
    {
        var item = new WorkItem();

        item.Title.Should().BeEmpty();
        item.Description.Should().BeNull();
        item.Category.Should().BeEmpty();
        item.Owner.Should().BeEmpty();
        item.Priority.Should().Be("Medium");
        item.Notes.Should().BeNull();
    }

    [Fact]
    public void ProjectInfo_DefaultConstructor_InitializesWithDefaults()
    {
        var info = new ProjectInfo();

        info.Name.Should().BeEmpty();
        info.ReportingPeriod.Should().BeEmpty();
        info.RagStatus.Should().Be("Green");
        info.Summary.Should().BeEmpty();
    }
}