using FluentAssertions;
using ReportingDashboard.Models;
using System.Text.Json;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataTests
{
    // --- Default Values ---

    [Fact]
    public void DashboardData_DefaultTitle_IsEmptyString()
    {
        var data = new DashboardData();
        data.Title.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultSubtitle_IsEmptyString()
    {
        var data = new DashboardData();
        data.Subtitle.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultBacklogLink_IsEmptyString()
    {
        var data = new DashboardData();
        data.BacklogLink.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultCurrentMonth_IsEmptyString()
    {
        var data = new DashboardData();
        data.CurrentMonth.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultMonths_IsEmptyList()
    {
        var data = new DashboardData();
        data.Months.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DashboardData_DefaultTimeline_IsNull()
    {
        var data = new DashboardData();
        data.Timeline.Should().BeNull();
    }

    [Fact]
    public void DashboardData_DefaultHeatmap_IsNull()
    {
        var data = new DashboardData();
        data.Heatmap.Should().BeNull();
    }

    [Fact]
    public void DashboardData_DefaultErrorMessage_IsNull()
    {
        var data = new DashboardData();
        data.ErrorMessage.Should().BeNull();
    }

    // --- JSON Deserialization ---

    [Fact]
    public void DashboardData_Deserialize_Title()
    {
        var json = """{"title":"My Dashboard"}""";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("My Dashboard");
    }

    [Fact]
    public void DashboardData_Deserialize_Subtitle()
    {
        var json = """{"subtitle":"Team A · April 2026"}""";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.Subtitle.Should().Be("Team A · April 2026");
    }

    [Fact]
    public void DashboardData_Deserialize_BacklogLink()
    {
        var json = """{"backlogLink":"https://dev.azure.com/org/project"}""";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.BacklogLink.Should().Be("https://dev.azure.com/org/project");
    }

    [Fact]
    public void DashboardData_Deserialize_CurrentMonth()
    {
        var json = """{"currentMonth":"April"}""";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.CurrentMonth.Should().Be("April");
    }

    [Fact]
    public void DashboardData_Deserialize_Months()
    {
        var json = """{"months":["Jan","Feb","Mar"]}""";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.Months.Should().HaveCount(3);
        data.Months.Should().ContainInOrder("Jan", "Feb", "Mar");
    }

    [Fact]
    public void DashboardData_Deserialize_EmptyObject_UsesDefaults()
    {
        var json = "{}";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.Months.Should().BeEmpty();
        data.Timeline.Should().BeNull();
        data.Heatmap.Should().BeNull();
    }

    [Fact]
    public void DashboardData_Deserialize_CompleteObject()
    {
        var json = """
        {
            "title": "Executive Dashboard",
            "subtitle": "Engineering · Core Platform",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "April",
            "months": ["Jan", "Feb", "Mar", "Apr"]
        }
        """;
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.Title.Should().Be("Executive Dashboard");
        data.Subtitle.Should().Be("Engineering · Core Platform");
        data.BacklogLink.Should().Be("https://dev.azure.com/test");
        data.CurrentMonth.Should().Be("April");
        data.Months.Should().HaveCount(4);
    }

    [Fact]
    public void DashboardData_ErrorMessage_IsNotSerialized()
    {
        var data = new DashboardData { ErrorMessage = "some error" };
        var json = JsonSerializer.Serialize(data);

        json.Should().NotContain("ErrorMessage");
        json.Should().NotContain("errorMessage");
    }

    [Fact]
    public void DashboardData_Deserialize_IgnoresUnknownFields()
    {
        var json = """{"title":"Test","unknownField":"value"}""";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data!.Title.Should().Be("Test");
    }

    // --- Property Assignment ---

    [Fact]
    public void DashboardData_CanSetAndGetAllProperties()
    {
        var timeline = new TimelineData();
        var heatmap = new HeatmapData();

        var data = new DashboardData
        {
            Title = "Title",
            Subtitle = "Subtitle",
            BacklogLink = "https://link",
            CurrentMonth = "May",
            Months = new List<string> { "May", "Jun" },
            Timeline = timeline,
            Heatmap = heatmap,
            ErrorMessage = "Error"
        };

        data.Title.Should().Be("Title");
        data.Subtitle.Should().Be("Subtitle");
        data.BacklogLink.Should().Be("https://link");
        data.CurrentMonth.Should().Be("May");
        data.Months.Should().HaveCount(2);
        data.Timeline.Should().BeSameAs(timeline);
        data.Heatmap.Should().BeSameAs(heatmap);
        data.ErrorMessage.Should().Be("Error");
    }
}