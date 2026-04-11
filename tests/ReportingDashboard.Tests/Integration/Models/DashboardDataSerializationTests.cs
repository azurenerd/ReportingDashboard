using FluentAssertions;
using ReportingDashboard.Models;
using System.Text.Json;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Models;

/// <summary>
/// Integration tests verifying the full JSON deserialization pipeline for DashboardData,
/// simulating the data flow from data.json through System.Text.Json into model objects
/// consumed by Header.razor and Dashboard.razor.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardDataSerializationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false
    };

    [Fact]
    public void Deserialize_FullDashboardJson_AllHeaderFieldsPopulated()
    {
        var json = """
        {
            "title": "Executive Reporting Dashboard",
            "subtitle": "Engineering · Core Platform · April 2026",
            "backlogLink": "https://dev.azure.com/org/project/_backlogs",
            "currentMonth": "April",
            "months": ["January", "February", "March", "April"],
            "timeline": {
                "nowDate": "2026-04-11",
                "tracks": []
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Executive Reporting Dashboard");
        data.Subtitle.Should().Be("Engineering · Core Platform · April 2026");
        data.BacklogLink.Should().Be("https://dev.azure.com/org/project/_backlogs");
        data.CurrentMonth.Should().Be("April");
        data.Months.Should().HaveCount(4);
        data.Timeline.Should().NotBeNull();
        data.Timeline!.NowDate.Should().Be("2026-04-11");
    }

    [Fact]
    public void Deserialize_MinimalJson_HeaderFieldsDefaultToEmpty()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.CurrentMonth.Should().BeEmpty();
        data.Months.Should().BeEmpty();
        data.Timeline.Should().BeNull();
        data.Heatmap.Should().BeNull();
    }

    [Fact]
    public void Deserialize_JsonWithOnlyHeaderFields_TimelineAndHeatmapNull()
    {
        var json = """
        {
            "title": "Header Only",
            "subtitle": "Sub Only",
            "backlogLink": "https://example.com"
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Title.Should().Be("Header Only");
        data.Timeline.Should().BeNull();
        data.Heatmap.Should().BeNull();
    }

    [Fact]
    public void Deserialize_UnicodeCharactersInTitle_PreservedCorrectly()
    {
        var json = """{"title": "Dashboard · Αlpha → Beta"}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Title.Should().Be("Dashboard · Αlpha → Beta");
    }

    [Fact]
    public void Deserialize_EmptyStringFields_NotNull()
    {
        var json = """
        {
            "title": "",
            "subtitle": "",
            "backlogLink": "",
            "currentMonth": ""
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogLink.Should().BeEmpty();
        data.CurrentMonth.Should().BeEmpty();
    }

    [Fact]
    public void Deserialize_ExtraFieldsInJson_IgnoredGracefully()
    {
        var json = """
        {
            "title": "Test",
            "unknownField": "should be ignored",
            "anotherUnknown": 42
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Title.Should().Be("Test");
    }

    [Fact]
    public void Deserialize_TimelineWithNowDate_TracksArePopulated()
    {
        var json = """
        {
            "title": "Timeline Test",
            "timeline": {
                "nowDate": "2026-06-15",
                "tracks": [
                    {
                        "name": "M1",
                        "label": "Milestone 1",
                        "color": "#FF0000"
                    }
                ]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Timeline.Should().NotBeNull();
        data.Timeline!.NowDate.Should().Be("2026-06-15");
        data.Timeline.Tracks.Should().HaveCount(1);
    }

    [Fact]
    public void Serialize_DashboardData_ErrorMessageExcluded()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            ErrorMessage = "This should not be serialized"
        };

        var json = JsonSerializer.Serialize(data, Options);

        json.Should().NotContain("ErrorMessage");
        json.Should().NotContain("errorMessage");
        json.Should().NotContain("This should not be serialized");
    }

    [Fact]
    public void RoundTrip_DashboardData_PreservesAllFields()
    {
        var original = new DashboardData
        {
            Title = "Round Trip Test",
            Subtitle = "Engineering · Q2",
            BacklogLink = "https://test.azure.com",
            CurrentMonth = "May",
            Months = new List<string> { "Apr", "May", "Jun" }
        };

        var json = JsonSerializer.Serialize(original, Options);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, Options);

        deserialized!.Title.Should().Be(original.Title);
        deserialized.Subtitle.Should().Be(original.Subtitle);
        deserialized.BacklogLink.Should().Be(original.BacklogLink);
        deserialized.CurrentMonth.Should().Be(original.CurrentMonth);
        deserialized.Months.Should().BeEquivalentTo(original.Months);
    }

    [Fact]
    public void Deserialize_MonthsArray_EmptyArray_DeserializesToEmptyList()
    {
        var json = """{"months": []}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Months.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Deserialize_MonthsArray_SixMonths()
    {
        var json = """{"months": ["Jan","Feb","Mar","Apr","May","Jun"]}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data!.Months.Should().HaveCount(6);
        data.Months[0].Should().Be("Jan");
        data.Months[5].Should().Be("Jun");
    }
}