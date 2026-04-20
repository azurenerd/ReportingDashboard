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
}