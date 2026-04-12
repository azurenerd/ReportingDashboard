using AgentSquad.Runner.Services;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class VisualizationServiceExtendedTests
{
    private readonly VisualizationService _service = new();

    #region GetCellClassName Comprehensive Tests

    [Theory]
    [InlineData("shipped", false, "ship-cell")]
    [InlineData("shipped", true, "ship-cell apr")]
    [InlineData("inprogress", false, "prog-cell")]
    [InlineData("inprogress", true, "prog-cell apr")]
    [InlineData("in-progress", false, "prog-cell")]
    [InlineData("in-progress", true, "prog-cell apr")]
    [InlineData("in progress", false, "prog-cell")]
    [InlineData("in progress", true, "prog-cell apr")]
    [InlineData("carryover", false, "carry-cell")]
    [InlineData("carryover", true, "carry-cell apr")]
    [InlineData("blockers", false, "block-cell")]
    [InlineData("blockers", true, "block-cell apr")]
    public void GetCellClassName_ReturnsExactClassNames(string status, bool isCurrentMonth, string expected)
    {
        var className = _service.GetCellClassName(status, isCurrentMonth);

        className.Should().Be(expected);
    }

    [Fact]
    public void GetCellClassName_WithInvalidStatus_ThrowsArgumentException()
    {
        var action = () => _service.GetCellClassName("invalid", false);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetCellClassName_WithEmptyStatus_ThrowsArgumentException()
    {
        var action = () => _service.GetCellClassName("", false);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetCellClassName_CurrentMonthAlwaysAppends_Apr()
    {
        var statuses = new[] { "shipped", "inprogress", "carryover", "blockers" };

        foreach (var status in statuses)
        {
            var className = _service.GetCellClassName(status, true);
            className.Should().EndWith(" apr");
        }
    }

    #endregion

    #region GetDotColor Comprehensive Tests

    [Theory]
    [InlineData("shipped", "#34A853")]
    [InlineData("inprogress", "#0078D4")]
    [InlineData("in-progress", "#0078D4")]
    [InlineData("in progress", "#0078D4")]
    [InlineData("carryover", "#F4B400")]
    [InlineData("blockers", "#EA4335")]
    public void GetDotColor_ReturnsCorrectHexCodes(string status, string expected)
    {
        var color = _service.GetDotColor(status);

        color.Should().Be(expected);
    }

    [Fact]
    public void GetDotColor_AllStatusTypesReturnValidHexCodes()
    {
        var statuses = new[] { "shipped", "inprogress", "carryover", "blockers" };

        foreach (var status in statuses)
        {
            var color = _service.GetDotColor(status);

            color.Should().StartWith("#");
            color.Length.Should().Be(7);
            color.Should().MatchRegex("^#[0-9A-Fa-f]{6}$");
        }
    }

    [Fact]
    public void GetDotColor_WithInvalidStatus_ThrowsArgumentException()
    {
        var action = () => _service.GetDotColor("unknown");

        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region GetStatusHeaderClassName Comprehensive Tests

    [Theory]
    [InlineData("shipped", "ship-hdr")]
    [InlineData("inprogress", "prog-hdr")]
    [InlineData("in-progress", "prog-hdr")]
    [InlineData("carryover", "carry-hdr")]
    [InlineData("blockers", "block-hdr")]
    public void GetStatusHeaderClassName_ReturnsCorrectClasses(string status, string expected)
    {
        var className = _service.GetStatusHeaderClassName(status);

        className.Should().Be(expected);
    }

    [Fact]
    public void GetStatusHeaderClassName_AllStatusTypesReturnValidClasses()
    {
        var statuses = new[] { "shipped", "inprogress", "carryover", "blockers" };

        foreach (var status in statuses)
        {
            var className = _service.GetStatusHeaderClassName(status);

            className.Should().NotBeNullOrEmpty();
            className.Should().Contain("-hdr");
        }
    }

    #endregion

    #region GenerateSvgDiamond Comprehensive Tests

    [Fact]
    public void GenerateSvgDiamond_WithFilter_ContainsFilterAttribute()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", true);

        svg.Should().Contain("filter=");
    }

    [Fact]
    public void GenerateSvgDiamond_WithoutFilter_DoesNotContainFilterAttribute()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);

        svg.Should().NotContain("filter=");
    }

    [Fact]
    public void GenerateSvgDiamond_ContainsPolygonTag()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);

        svg.Should().Contain("<polygon");
    }

    [Fact]
    public void GenerateSvgDiamond_ContainsPoints()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);

        svg.Should().Contain("points=");
    }

    [Fact]
    public void GenerateSvgDiamond_ContainsFillColor()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);

        svg.Should().Contain("fill=\"#F4B400\"");
    }

    [Fact]
    public void GenerateSvgDiamond_DifferentColorsProduceDifferentOutput()
    {
        var svg1 = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);
        var svg2 = _service.GenerateSvgDiamond(100, 50, "#34A853", false);

        svg1.Should().NotBe(svg2);
    }

    [Fact]
    public void GenerateSvgDiamond_DifferentPositionsProduceDifferentOutput()
    {
        var svg1 = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);
        var svg2 = _service.GenerateSvgDiamond(200, 100, "#F4B400", false);

        svg1.Should().NotBe(svg2);
    }

    #endregion

    #region GenerateSvgCircle Comprehensive Tests

    [Fact]
    public void GenerateSvgCircle_ContainsCircleTag()
    {
        var svg = _service.GenerateSvgCircle(100, 50, 5, "#999", "#666", 2);

        svg.Should().Contain("<circle");
    }

    [Theory]
    [InlineData("cx")]
    [InlineData("cy")]
    [InlineData("r")]
    [InlineData("fill")]
    [InlineData("stroke")]
    [InlineData("stroke-width")]
    public void GenerateSvgCircle_ContainsRequiredAttributes(string attribute)
    {
        var svg = _service.GenerateSvgCircle(100, 50, 5, "#999", "#666", 2);

        svg.Should().Contain(attribute);
    }

    [Fact]
    public void GenerateSvgCircle_WithDifferentRadii_ProducesDifferentOutput()
    {
        var svg1 = _service.GenerateSvgCircle(100, 50, 5, "#999", "#666", 2);
        var svg2 = _service.GenerateSvgCircle(100, 50, 10, "#999", "#666", 2);

        svg1.Should().NotBe(svg2);
    }

    [Fact]
    public void GenerateSvgCircle_ContainsCorrectCxValue()
    {
        var svg = _service.GenerateSvgCircle(123, 50, 5, "#999", "#666", 2);

        svg.Should().Contain("cx=\"123\"");
    }

    [Fact]
    public void GenerateSvgCircle_ContainsCorrectCyValue()
    {
        var svg = _service.GenerateSvgCircle(100, 456, 5, "#999", "#666", 2);

        svg.Should().Contain("cy=\"456\"");
    }

    [Fact]
    public void GenerateSvgCircle_ContainsCorrectRadiusValue()
    {
        var svg = _service.GenerateSvgCircle(100, 50, 7, "#999", "#666", 2);

        svg.Should().Contain("r=\"7\"");
    }

    #endregion

    #region GenerateSvgLine Comprehensive Tests

    [Fact]
    public void GenerateSvgLine_ContainsLineTag()
    {
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2);

        svg.Should().Contain("<line");
    }

    [Theory]
    [InlineData("x1")]
    [InlineData("y1")]
    [InlineData("x2")]
    [InlineData("y2")]
    [InlineData("stroke")]
    [InlineData("stroke-width")]
    public void GenerateSvgLine_ContainsRequiredAttributes(string attribute)
    {
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2);

        svg.Should().Contain(attribute);
    }

    [Fact]
    public void GenerateSvgLine_WithDasharray_ContainsDasharrayAttribute()
    {
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2, "5,3");

        svg.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void GenerateSvgLine_WithoutDasharray_DoesNotContainDasharrayAttribute()
    {
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2);

        svg.Should().NotContain("stroke-dasharray");
    }

    [Fact]
    public void GenerateSvgLine_WithDifferentDasharray_ProducesDifferentOutput()
    {
        var svg1 = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2, "5,3");
        var svg2 = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2, "10,5");

        svg1.Should().NotBe(svg2);
    }

    [Fact]
    public void GenerateSvgLine_ContainsCorrectCoordinates()
    {
        var svg = _service.GenerateSvgLine(10, 20, 30, 40, "#EA4335", 2);

        svg.Should().Contain("x1=\"10\"");
        svg.Should().Contain("y1=\"20\"");
        svg.Should().Contain("x2=\"30\"");
        svg.Should().Contain("y2=\"40\"");
    }

    [Fact]
    public void GenerateSvgLine_ContainsCorrectStrokeWidth()
    {
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 5);

        svg.Should().Contain("stroke-width=\"5\"");
    }

    #endregion

    #region GetMilestoneShapes Comprehensive Tests

    [Fact]
    public void GetMilestoneShapes_ReturnsNonEmptyDictionary()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes.Should().NotBeEmpty();
    }

    [Fact]
    public void GetMilestoneShapes_ContainsAllThreeKeys()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes.Keys.Should().Contain(new[] { "poc", "release", "checkpoint" });
    }

    [Fact]
    public void GetMilestoneShapes_PocHasDiamondShape()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["poc"].Shape.Should().Be("diamond");
    }

    [Fact]
    public void GetMilestoneShapes_PocHasYellowColor()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["poc"].Color.Should().Be("#F4B400");
    }

    [Fact]
    public void GetMilestoneShapes_ReleaseHasDiamondShape()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["release"].Shape.Should().Be("diamond");
    }

    [Fact]
    public void GetMilestoneShapes_ReleaseHasGreenColor()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["release"].Color.Should().Be("#34A853");
    }

    [Fact]
    public void GetMilestoneShapes_CheckpointHasCircleShape()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["checkpoint"].Shape.Should().Be("circle");
    }

    [Fact]
    public void GetMilestoneShapes_CheckpointHasGrayColor()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["checkpoint"].Color.Should().Be("#999");
    }

    [Fact]
    public void GetMilestoneShapes_AllShapesHaveSize()
    {
        var shapes = _service.GetMilestoneShapes();

        foreach (var shape in shapes.Values)
        {
            shape.Size.Should().BeGreaterThan(0);
        }
    }

    #endregion
}