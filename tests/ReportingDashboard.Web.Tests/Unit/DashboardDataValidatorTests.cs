using System.Linq;
using FluentAssertions;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

public class DashboardDataValidatorTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithValidData_ReturnsNoErrors()
    {
        var data = TestDataFactory.ValidData();

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithEmptyProjectTitle_ReturnsTitleError()
    {
        var json = TestDataFactory.ValidJson.Replace("\"title\": \"My Project\"", "\"title\": \"\"");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("project.title is required"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithNonHttpBacklogUrl_ReturnsBacklogUrlError()
    {
        var json = TestDataFactory.ValidJson.Replace(
            "\"backlogUrl\": \"https://dev.azure.com/org/project\"",
            "\"backlogUrl\": \"ftp://example.com/backlog\"");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("backlogUrl"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithBadLaneColor_ReturnsColorError()
    {
        var json = TestDataFactory.ValidJson.Replace("\"#0078D4\"", "\"#GGGGGG\"");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("#RRGGBB"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithMismatchedCellCount_ReturnsCellCountError()
    {
        var json = TestDataFactory.ValidJson.Replace(
            "{ \"category\": \"shipped\",    \"cells\": [[], [], [], []] }",
            "{ \"category\": \"shipped\",    \"cells\": [[], [], []] }");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("cells count"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithTimelineStartNotBeforeEnd_ReturnsRangeError()
    {
        var json = TestDataFactory.ValidJson
            .Replace("\"start\": \"2026-01-01\"", "\"start\": \"2026-12-31\"")
            .Replace("\"end\": \"2026-12-31\"", "\"end\": \"2026-01-01\"");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("must be before"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithMilestoneOutsideRange_ReturnsDateError()
    {
        var json = TestDataFactory.ValidJson.Replace("\"2026-03-15\"", "\"2027-03-15\"");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("outside timeline range"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithDuplicateLaneIds_ReturnsDuplicateError()
    {
        var json = TestDataFactory.ValidJson.Replace(
            "\"lanes\": [",
            """
            "lanes": [
              { "id": "M1", "label": "Dup", "color": "#111111", "milestones": [] },
            """);
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("duplicated"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithSevenLanes_ReturnsMaxLanesError()
    {
        var extra = string.Join(",", Enumerable.Range(2, 6).Select(i =>
            $"{{ \"id\": \"M{i}\", \"label\": \"L{i}\", \"color\": \"#00{i:D2}{i:D2}\", \"milestones\": [] }}"));
        var json = TestDataFactory.ValidJson.Replace(
            "\"lanes\": [",
            "\"lanes\": [" + extra + ",");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("exceeds maximum of 6"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithCurrentMonthIndexOutOfRange_ReturnsIndexError()
    {
        var json = TestDataFactory.ValidJson.Replace("\"currentMonthIndex\": 3", "\"currentMonthIndex\": 99");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("currentMonthIndex"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithMaxItemsPerCellZero_ReturnsError()
    {
        var json = TestDataFactory.ValidJson.Replace("\"maxItemsPerCell\": 3", "\"maxItemsPerCell\": 0");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("maxItemsPerCell"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Validate_WithMissingRequiredCategory_ReturnsMissingError()
    {
        var json = TestDataFactory.ValidJson.Replace(
            "{ \"category\": \"blockers\",   \"cells\": [[], [], [], []] }",
            "{ \"category\": \"inProgress\", \"cells\": [[], [], [], []] }");
        var data = TestDataFactory.Deserialize(json);

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().Contain(e => e.Contains("missing required category"));
    }
}