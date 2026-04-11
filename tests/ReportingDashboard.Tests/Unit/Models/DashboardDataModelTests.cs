using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void DefaultConstructor_SetsEmptyDefaults()
    {
        var data = new DashboardData();

        data.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.CurrentMonth.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().BeNull();
        data.Heatmap.Should().BeNull();
        data.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        var data = new DashboardData
        {
            Title = "Test Title",
            Subtitle = "Test Subtitle",
            BacklogLink = "https://example.com",
            CurrentMonth = "Apr",
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
            Timeline = new TimelineData(),
            Heatmap = new HeatmapData()
        };

        data.Title.Should().Be("Test Title");
        data.Subtitle.Should().Be("Test Subtitle");
        data.BacklogLink.Should().Be("https://example.com");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().HaveCount(4);
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void ErrorMessage_IsJsonIgnored()
    {
        var data = new DashboardData { ErrorMessage = "Some error" };

        var json = JsonSerializer.Serialize(data);

        json.Should().NotContain("ErrorMessage");
        json.Should().NotContain("errorMessage");
    }

    [Fact]
    public void Deserialize_WithValidJson_SetsAllProperties()
    {
        var json = """
        {
            "title": "My Dashboard",
            "subtitle": "Team A - April 2026",
            "backlogLink": "https://ado.example.com",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"]
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("My Dashboard");
        data.Subtitle.Should().Be("Team A - April 2026");
        data.BacklogLink.Should().Be("https://ado.example.com");
        data.CurrentMonth.Should().Be("Apr");
        data.Months.Should().BeEquivalentTo(new[] { "Jan", "Feb", "Mar", "Apr" });
    }

    [Fact]
    public void Deserialize_WithMissingOptionalFields_UsesDefaults()
    {
        var json = """{ "title": "Test" }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Test");
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.CurrentMonth.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Timeline.Should().BeNull();
        data.Heatmap.Should().BeNull();
    }

    [Fact]
    public void Deserialize_EmptyJson_ReturnsDefaultObject()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Deserialize_WithEmptyMonthsArray_DoesNotThrow()
    {
        var json = """{ "title": "Test", "months": [] }""";

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Months.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Serialize_RoundTrip_PreservesJsonPropertyNames()
    {
        var data = new DashboardData
        {
            Title = "RT Title",
            Subtitle = "RT Subtitle",
            BacklogLink = "https://rt.com",
            CurrentMonth = "Mar",
            Months = new List<string> { "Jan", "Feb", "Mar" }
        };

        var json = JsonSerializer.Serialize(data);

        json.Should().Contain("\"title\"");
        json.Should().Contain("\"subtitle\"");
        json.Should().Contain("\"backlogLink\"");
        json.Should().Contain("\"currentMonth\"");
        json.Should().Contain("\"months\"");
    }

    [Fact]
    public void Months_CollectionInitializer_PreventsNullReference()
    {
        var data = new DashboardData();

        // Should be able to iterate without NullReferenceException
        var action = () =>
        {
            foreach (var _ in data.Months) { }
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void Deserialize_WithExtraUnknownFields_DoesNotThrow()
    {
        var json = """
        {
            "title": "Test",
            "unknownField": "should be ignored",
            "anotherUnknown": 42
        }
        """;

        var action = () => JsonSerializer.Deserialize<DashboardData>(json);

        action.Should().NotThrow();
    }
}