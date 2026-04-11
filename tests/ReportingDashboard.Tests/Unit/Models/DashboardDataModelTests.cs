using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void DashboardData_DefaultValues_AreCorrect()
    {
        var data = new DashboardData();

        data.Project.Should().BeNull();
        data.Milestones.Should().NotBeNull().And.BeEmpty();
        data.Shipped.Should().NotBeNull().And.BeEmpty();
        data.InProgress.Should().NotBeNull().And.BeEmpty();
        data.CarriedOver.Should().NotBeNull().And.BeEmpty();
        data.CurrentMonth.Should().BeNull();
        data.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ProjectInfo_DefaultValues_AreCorrect()
    {
        var info = new ProjectInfo();

        info.Name.Should().Be("Untitled Project");
        info.Lead.Should().BeNull();
        info.Status.Should().Be("Unknown");
        info.LastUpdated.Should().BeNull();
        info.Summary.Should().BeNull();
    }

    [Fact]
    public void Milestone_DefaultValues_AreCorrect()
    {
        var milestone = new Milestone();

        milestone.Title.Should().Be("");
        milestone.TargetDate.Should().BeNull();
        milestone.Status.Should().Be("Upcoming");
    }

    [Fact]
    public void WorkItem_DefaultValues_AreCorrect()
    {
        var item = new WorkItem();

        item.Title.Should().Be("");
        item.Description.Should().BeNull();
        item.Category.Should().BeNull();
        item.PercentComplete.Should().Be(0);
        item.CarryOverReason.Should().BeNull();
    }

    [Fact]
    public void MonthSummary_DefaultValues_AreCorrect()
    {
        var summary = new MonthSummary();

        summary.Month.Should().BeNull();
        summary.TotalItems.Should().Be(0);
        summary.CompletedItems.Should().Be(0);
        summary.CarriedItems.Should().Be(0);
        summary.OverallHealth.Should().Be("Unknown");
    }

    [Fact]
    public void DashboardData_JsonRoundTrip_PreservesAllFields()
    {
        var original = new DashboardData
        {
            Project = new ProjectInfo
            {
                Name = "Test",
                Lead = "Alice",
                Status = "On Track",
                LastUpdated = "2026-04-01",
                Summary = "Summary text"
            },
            Milestones = [new Milestone { Title = "M1", TargetDate = "2026-05-01", Status = "Completed" }],
            Shipped = [new WorkItem { Title = "S1", Description = "Done", Category = "Core", PercentComplete = 100 }],
            InProgress = [new WorkItem { Title = "I1", PercentComplete = 50 }],
            CarriedOver = [new WorkItem { Title = "C1", CarryOverReason = "Delayed" }],
            CurrentMonth = new MonthSummary
            {
                Month = "April",
                TotalItems = 10,
                CompletedItems = 7,
                CarriedItems = 2,
                OverallHealth = "On Track"
            }
        };

        var json = JsonSerializer.Serialize(original, CamelCaseOptions);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, CamelCaseOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Project!.Name.Should().Be("Test");
        deserialized.Project.Lead.Should().Be("Alice");
        deserialized.Milestones.Should().HaveCount(1);
        deserialized.Milestones[0].Title.Should().Be("M1");
        deserialized.Shipped.Should().HaveCount(1);
        deserialized.InProgress.Should().HaveCount(1);
        deserialized.CarriedOver.Should().HaveCount(1);
        deserialized.CurrentMonth!.Month.Should().Be("April");
        deserialized.CurrentMonth.TotalItems.Should().Be(10);
    }

    [Fact]
    public void Serialization_ProducesCamelCaseKeys()
    {
        var data = new DashboardData
        {
            Project = new ProjectInfo { Name = "Test" },
            CurrentMonth = new MonthSummary { TotalItems = 5 }
        };

        var json = JsonSerializer.Serialize(data, CamelCaseOptions);

        json.Should().Contain("\"project\"");
        json.Should().Contain("\"milestones\"");
        json.Should().Contain("\"shipped\"");
        json.Should().Contain("\"inProgress\"");
        json.Should().Contain("\"carriedOver\"");
        json.Should().Contain("\"currentMonth\"");
        json.Should().Contain("\"totalItems\"");
    }

    [Fact]
    public void Milestone_JsonRoundTrip_PreservesFields()
    {
        var original = new Milestone { Title = "Beta Release", TargetDate = "2026-06-15", Status = "In Progress" };

        var json = JsonSerializer.Serialize(original, CamelCaseOptions);
        var deserialized = JsonSerializer.Deserialize<Milestone>(json, CamelCaseOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Title.Should().Be("Beta Release");
        deserialized.TargetDate.Should().Be("2026-06-15");
        deserialized.Status.Should().Be("In Progress");
    }

    [Fact]
    public void WorkItem_JsonRoundTrip_PreservesFields()
    {
        var original = new WorkItem
        {
            Title = "Feature X",
            Description = "Important feature",
            Category = "Backend",
            PercentComplete = 75,
            CarryOverReason = "Dependencies"
        };

        var json = JsonSerializer.Serialize(original, CamelCaseOptions);
        var deserialized = JsonSerializer.Deserialize<WorkItem>(json, CamelCaseOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Title.Should().Be("Feature X");
        deserialized.Description.Should().Be("Important feature");
        deserialized.Category.Should().Be("Backend");
        deserialized.PercentComplete.Should().Be(75);
        deserialized.CarryOverReason.Should().Be("Dependencies");
    }
}