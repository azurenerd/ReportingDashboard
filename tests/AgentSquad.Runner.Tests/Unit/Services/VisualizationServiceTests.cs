using AgentSquad.Runner.Services;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class VisualizationServiceTests
{
    private readonly VisualizationService _service = new();

    [Fact]
    public void GetCellClassName_ReturnsShipCellForShipped()
    {
        var result = _service.GetCellClassName("shipped", false);

        result.Should().Be("ship-cell");
    }

    [Fact]
    public void GetCellClassName_ReturnsProgCellForInProgress()
    {
        var result = _service.GetCellClassName("inprogress", false);

        result.Should().Be("prog-cell");
    }

    [Fact]
    public void GetCellClassName_ReturnsCarryCellForCarryover()
    {
        var result = _service.GetCellClassName("carryover", false);

        result.Should().Be("carry-cell");
    }

    [Fact]
    public void GetCellClassName_ReturnsBlockCellForBlockers()
    {
        var result = _service.GetCellClassName("blockers", false);

        result.Should().Be("block-cell");
    }

    [Fact]
    public void GetCellClassName_AddsAprClassForCurrentMonth()
    {
        var result = _service.GetCellClassName("shipped", true);

        result.Should().Contain("apr");
    }

    [Fact]
    public void GetDotColor_ReturnsGreenForShipped()
    {
        var result = _service.GetDotColor("shipped");

        result.Should().Be("#34A853");
    }

    [Fact]
    public void GetDotColor_ReturnsBlueForInProgress()
    {
        var result = _service.GetDotColor("inprogress");

        result.Should().Be("#0078D4");
    }

    [Fact]
    public void GetDotColor_ReturnsYellowForCarryover()
    {
        var result = _service.GetDotColor("carryover");

        result.Should().Be("#F4B400");
    }

    [Fact]
    public void GetDotColor_ReturnsRedForBlockers()
    {
        var result = _service.GetDotColor("blockers");

        result.Should().Be("#EA4335");
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsShipHdrForShipped()
    {
        var result = _service.GetStatusHeaderClassName("shipped");

        result.Should().Be("ship-hdr");
    }

    [Fact]
    public void GetStatusHeaderClassName_IsCaseInsensitive()
    {
        var result = _service.GetStatusHeaderClassName("SHIPPED");

        result.Should().Be("ship-hdr");
    }

    [Fact]
    public void GenerateSvgDiamond_ContainsPolygonElement()
    {
        var result = _service.GenerateSvgDiamond(100, 100, "#F4B400");

        result.Should().Contain("polygon");
    }

    [Fact]
    public void GenerateSvgDiamond_ContainsFillAttribute()
    {
        var result = _service.GenerateSvgDiamond(100, 100, "#F4B400");

        result.Should().Contain("fill=\"#F4B400\"");
    }

    [Fact]
    public void GenerateSvgDiamond_WithFilterIncludesFilterReference()
    {
        var result = _service.GenerateSvgDiamond(100, 100, "#F4B400", true);

        result.Should().Contain("filter");
    }

    [Fact]
    public void GenerateSvgDiamond_WithoutFilterOmitsFilterReference()
    {
        var result = _service.GenerateSvgDiamond(100, 100, "#F4B400", false);

        result.Should().NotContain("filter");
    }

    [Fact]
    public void GenerateSvgCircle_ContainsCircleElement()
    {
        var result = _service.GenerateSvgCircle(100, 100, 5, "#999", "#666", 1);

        result.Should().Contain("circle");
    }

    [Fact]
    public void GenerateSvgCircle_ContainsCxCyAttributes()
    {
        var result = _service.GenerateSvgCircle(150, 200, 5, "#999", "#666", 1);

        result.Should().Contain("cx=\"150\"");
        result.Should().Contain("cy=\"200\"");
    }

    [Fact]
    public void GenerateSvgCircle_ContainsRadiusAttribute()
    {
        var result = _service.GenerateSvgCircle(100, 100, 8, "#999", "#666", 1);

        result.Should().Contain("r=\"8\"");
    }

    [Fact]
    public void GenerateSvgLine_ContainsLineElement()
    {
        var result = _service.GenerateSvgLine(0, 0, 100, 100, "#EA4335", 2);

        result.Should().Contain("line");
    }

    [Fact]
    public void GenerateSvgLine_ContainsCoordinates()
    {
        var result = _service.GenerateSvgLine(10, 20, 100, 200, "#EA4335", 2);

        result.Should().Contain("x1=\"10\"");
        result.Should().Contain("y1=\"20\"");
        result.Should().Contain("x2=\"100\"");
        result.Should().Contain("y2=\"200\"");
    }

    [Fact]
    public void GenerateSvgLine_WithDasharray_IncludesDashAttribute()
    {
        var result = _service.GenerateSvgLine(0, 0, 100, 100, "#EA4335", 2, "5,3");

        result.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void GenerateSvgLine_WithoutDasharray_OmitsDashAttribute()
    {
        var result = _service.GenerateSvgLine(0, 0, 100, 100, "#EA4335", 2);

        result.Should().NotContain("stroke-dasharray");
    }

    [Fact]
    public void GetMilestoneShapes_ReturnsDictionary()
    {
        var result = _service.GetMilestoneShapes();

        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<string, AgentSquad.Runner.Models.MilestoneShapeInfo>>();
    }

    [Fact]
    public void GetMilestoneShapes_ContainsPocKey()
    {
        var result = _service.GetMilestoneShapes();

        result.Should().ContainKey("poc");
    }

    [Fact]
    public void GetMilestoneShapes_ContainsReleaseKey()
    {
        var result = _service.GetMilestoneShapes();

        result.Should().ContainKey("release");
    }

    [Fact]
    public void GetMilestoneShapes_ContainsCheckpointKey()
    {
        var result = _service.GetMilestoneShapes();

        result.Should().ContainKey("checkpoint");
    }

    [Fact]
    public void GetMilestoneShapes_PocShapeIsDiamond()
    {
        var result = _service.GetMilestoneShapes();

        result["poc"].Shape.Should().Be("diamond");
    }

    [Fact]
    public void GetMilestoneShapes_ReleaseShapeIsDiamond()
    {
        var result = _service.GetMilestoneShapes();

        result["release"].Shape.Should().Be("diamond");
    }

    [Fact]
    public void GetMilestoneShapes_CheckpointShapeIsCircle()
    {
        var result = _service.GetMilestoneShapes();

        result["checkpoint"].Shape.Should().Be("circle");
    }

    [Fact]
    public void GetMilestoneShapes_PocColorIsYellow()
    {
        var result = _service.GetMilestoneShapes();

        result["poc"].Color.Should().Be("#F4B400");
    }

    [Fact]
    public void GetMilestoneShapes_ReleaseColorIsGreen()
    {
        var result = _service.GetMilestoneShapes();

        result["release"].Color.Should().Be("#34A853");
    }

    [Fact]
    public void GetCellClassName_DefaultsToShipCellForUnknownStatus()
    {
        var result = _service.GetCellClassName("unknown", false);

        result.Should().Be("ship-cell");
    }
}