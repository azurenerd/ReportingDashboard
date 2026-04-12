using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests;

public class VisualizationServiceTests
{
    private readonly VisualizationService _service;

    public VisualizationServiceTests()
    {
        _service = new VisualizationService();
    }

    #region CSS Class Name Tests

    [Theory]
    [InlineData("shipped", false, "ship-cell")]
    [InlineData("shipped", true, "ship-cell apr")]
    [InlineData("inprogress", false, "prog-cell")]
    [InlineData("inprogress", true, "prog-cell apr")]
    [InlineData("in-progress", false, "prog-cell")]
    [InlineData("in progress", false, "prog-cell")]
    [InlineData("carryover", false, "carry-cell")]
    [InlineData("carryover", true, "carry-cell apr")]
    [InlineData("blockers", false, "block-cell")]
    [InlineData("blockers", true, "block-cell apr")]
    public void GetCellClassName_ReturnsCorrectClassName(string status, bool isCurrentMonth, string expected)
    {
        // Act
        var className = _service.GetCellClassName(status, isCurrentMonth);

        // Assert
        Assert.Equal(expected, className);
    }

    [Theory]
    [InlineData("shipped")]
    [InlineData("inprogress")]
    [InlineData("carryover")]
    [InlineData("blockers")]
    public void GetCellClassName_CaseInsensitive(string status)
    {
        // Act
        var classNameLower = _service.GetCellClassName(status.ToLower(), false);
        var classNameUpper = _service.GetCellClassName(status.ToUpper(), false);

        // Assert
        Assert.Equal(classNameLower, classNameUpper);
    }

    [Fact]
    public void GetCellClassName_WithInvalidStatus_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetCellClassName("invalid", false));
    }

    #endregion

    #region Background Color Tests

    [Theory]
    [InlineData("shipped", false, "#F0FBF0")]
    [InlineData("shipped", true, "#D8F2DA")]
    [InlineData("inprogress", false, "#EEF4FE")]
    [InlineData("inprogress", true, "#DAE8FB")]
    [InlineData("carryover", false, "#FFFDE7")]
    [InlineData("carryover", true, "#FFF0B0")]
    [InlineData("blockers", false, "#FFF5F5")]
    [InlineData("blockers", true, "#FFE4E4")]
    public void GetCellBackgroundColor_ReturnsCorrectHexColor(string status, bool isCurrentMonth, string expected)
    {
        // Act
        var color = _service.GetCellBackgroundColor(status, isCurrentMonth);

        // Assert
        Assert.Equal(expected, color);
    }

    #endregion

    #region Dot Color Tests

    [Theory]
    [InlineData("shipped", "#34A853")]
    [InlineData("inprogress", "#0078D4")]
    [InlineData("in-progress", "#0078D4")]
    [InlineData("in progress", "#0078D4")]
    [InlineData("carryover", "#F4B400")]
    [InlineData("blockers", "#EA4335")]
    public void GetDotColor_ReturnsCorrectHexColor(string status, string expected)
    {
        // Act
        var color = _service.GetDotColor(status);

        // Assert
        Assert.Equal(expected, color);
    }

    [Fact]
    public void GetDotColor_WithInvalidStatus_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetDotColor("invalid"));
    }

    #endregion

    #region Status Header Tests

    [Theory]
    [InlineData("shipped", "ship-hdr")]
    [InlineData("inprogress", "prog-hdr")]
    [InlineData("carryover", "carry-hdr")]
    [InlineData("blockers", "block-hdr")]
    public void GetStatusHeaderClassName_ReturnsCorrectClassName(string status, string expected)
    {
        // Act
        var className = _service.GetStatusHeaderClassName(status);

        // Assert
        Assert.Equal(expected, className);
    }

    [Theory]
    [InlineData("shipped", "#1B7A28")]
    [InlineData("inprogress", "#1565C0")]
    [InlineData("carryover", "#B45309")]
    [InlineData("blockers", "#991B1B")]
    public void GetStatusHeaderColor_ReturnsCorrectHexColor(string status, string expected)
    {
        // Act
        var color = _service.GetStatusHeaderColor(status);

        // Assert
        Assert.Equal(expected, color);
    }

    [Theory]
    [InlineData("shipped", "#E8F5E9")]
    [InlineData("inprogress", "#E3F2FD")]
    [InlineData("carryover", "#FFF8E1")]
    [InlineData("blockers", "#FEF2F2")]
    public void GetStatusHeaderBackgroundColor_ReturnsCorrectHexColor(string status, string expected)
    {
        // Act
        var color = _service.GetStatusHeaderBackgroundColor(status);

        // Assert
        Assert.Equal(expected, color);
    }

    #endregion

    #region SVG Generation Tests

    [Fact]
    public void GenerateSvgDiamond_WithValidParams_GeneratesValidSvgPolygon()
    {
        // Act
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", true);

        // Assert
        Assert.Contains("<polygon", svg);
        Assert.Contains("points=", svg);
        Assert.Contains("fill=\"#F4B400\"", svg);
        Assert.Contains("filter=", svg);
    }

    [Fact]
    public void GenerateSvgDiamond_WithoutFilter_DoesNotIncludeFilterAttribute()
    {
        // Act
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);

        // Assert
        Assert.DoesNotContain("filter=", svg);
    }

    [Fact]
    public void GenerateSvgCircle_WithValidParams_GeneratesValidSvgCircle()
    {
        // Act
        var svg = _service.GenerateSvgCircle(100, 50, 5, "#999", "#666", 2);

        // Assert
        Assert.Contains("<circle", svg);
        Assert.Contains("cx=\"100\"", svg);
        Assert.Contains("cy=\"50\"", svg);
        Assert.Contains("r=\"5\"", svg);
        Assert.Contains("fill=\"#999\"", svg);
        Assert.Contains("stroke=\"#666\"", svg);
        Assert.Contains("stroke-width=\"2\"", svg);
    }

    [Fact]
    public void GenerateSvgLine_WithValidParams_GeneratesValidSvgLine()
    {
        // Act
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#0078D4", 3);

        // Assert
        Assert.Contains("<line", svg);
        Assert.Contains("x1=\"0\"", svg);
        Assert.Contains("y1=\"100\"", svg);
        Assert.Contains("x2=\"1560\"", svg);
        Assert.Contains("y2=\"100\"", svg);
        Assert.Contains("stroke=\"#0078D4\"", svg);
        Assert.Contains("stroke-width=\"3\"", svg);
    }

    [Fact]
    public void GenerateSvgLine_WithDasharray_IncludesDasharrayAttribute()
    {
        // Act
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2, "5,3");

        // Assert
        Assert.Contains("stroke-dasharray=\"5,3\"", svg);
    }

    [Fact]
    public void GenerateSvgLine_WithoutDasharray_DoesNotIncludeDasharrayAttribute()
    {
        // Act
        var svg = _service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2);

        // Assert
        Assert.DoesNotContain("stroke-dasharray=", svg);
    }

    [Fact]
    public void GenerateSvgText_WithValidParams_GeneratesValidSvgText()
    {
        // Act
        var svg = _service.GenerateSvgText(100, 50, "March", "12", "#666", "middle");

        // Assert
        Assert.Contains("<text", svg);
        Assert.Contains("x=\"100\"", svg);
        Assert.Contains("y=\"50\"", svg);
        Assert.Contains("font-size=\"12\"", svg);
        Assert.Contains("fill=\"#666\"", svg);
        Assert.Contains("text-anchor=\"middle\"", svg);
        Assert.Contains(">March</text>", svg);
    }

    [Fact]
    public void GenerateSvgText_EscapesHtmlSpecialCharacters()
    {
        // Act
        var svg = _service.GenerateSvgText(100, 50, "Test & <tag>", "12");

        // Assert
        Assert.Contains("&amp;", svg);
        Assert.Contains("&lt;", svg);
        Assert.Contains("&gt;", svg);
    }

    [Fact]
    public void GenerateSvgRect_WithValidParams_GeneratesValidSvgRect()
    {
        // Act
        var svg = _service.GenerateSvgRect(0, 0, 100, 50, "#FFF", "#000", 1);

        // Assert
        Assert.Contains("<rect", svg);
        Assert.Contains("x=\"0\"", svg);
        Assert.Contains("y=\"0\"", svg);
        Assert.Contains("width=\"100\"", svg);
        Assert.Contains("height=\"50\"", svg);
        Assert.Contains("fill=\"#FFF\"", svg);
        Assert.Contains("stroke=\"#000\"", svg);
        Assert.Contains("stroke-width=\"1\"", svg);
    }

    #endregion

    #region Milestone Shape Tests

    [Fact]
    public void GetMilestoneShapes_ReturnsAllThreeTypes()
    {
        // Act
        var shapes = _service.GetMilestoneShapes();

        // Assert
        Assert.Equal(3, shapes.Count);
        Assert.ContainsKey("poc", shapes);
        Assert.ContainsKey("release", shapes);
        Assert.ContainsKey("checkpoint", shapes);
    }

    [Fact]
    public void GetMilestoneShape_Poc_ReturnsYellowDiamond()
    {
        // Act
        var shape = _service.GetMilestoneShape("poc");

        // Assert
        Assert.Equal("poc", shape.Type);
        Assert.Equal("diamond", shape.Shape);
        Assert.Equal("#F4B400", shape.Color);
        Assert.Equal(12, shape.Size);
    }

    [Fact]
    public void GetMilestoneShape_Release_ReturnsGreenDiamond()
    {
        // Act
        var shape = _service.GetMilestoneShape("release");

        // Assert
        Assert.Equal("release", shape.Type);
        Assert.Equal("diamond", shape.Shape);
        Assert.Equal("#34A853", shape.Color);
        Assert.Equal(12, shape.Size);
    }

    [Fact]
    public void GetMilestoneShape_Checkpoint_ReturnsGrayCircle()
    {
        // Act
        var shape = _service.GetMilestoneShape("checkpoint");

        // Assert
        Assert.Equal("checkpoint", shape.Type);
        Assert.Equal("circle", shape.Shape);
        Assert.Equal("#999", shape.Color);
        Assert.Equal(5, shape.Size);
    }

    [Fact]
    public void GetMilestoneShape_WithInvalidType_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetMilestoneShape("invalid"));
    }

    #endregion

    #region Timeline Tests

    [Theory]
    [InlineData(0, "#0078D4")]
    [InlineData(1, "#00897B")]
    [InlineData(2, "#546E7A")]
    [InlineData(3, "#999")]
    public void GetTimelineColor_ReturnsCorrectColorByIndex(int index, string expected)
    {
        // Act
        var color = _service.GetTimelineColor(index);

        // Assert
        Assert.Equal(expected, color);
    }

    [Theory]
    [InlineData(0, 42)]
    [InlineData(1, 98)]
    [InlineData(2, 154)]
    [InlineData(3, 42)]
    public void GetTimelineYPosition_ReturnsCorrectPositionByIndex(int index, int expected)
    {
        // Act
        var position = _service.GetTimelineYPosition(index);

        // Assert
        Assert.Equal(expected, position);
    }

    #endregion
}