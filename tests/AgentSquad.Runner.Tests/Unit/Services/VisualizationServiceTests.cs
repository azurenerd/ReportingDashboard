using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class VisualizationServiceTests
{
    private readonly VisualizationService _service = new();

    [Fact]
    public void GetCellClassName_ReturnsCorrectClassForShipped()
    {
        var className = _service.GetCellClassName("shipped", false);

        className.Should().Contain("ship-cell");
    }

    [Fact]
    public void GetCellClassName_ReturnsCurrentMonthVariant()
    {
        var className = _service.GetCellClassName("shipped", true);

        className.Should().Contain("ship-cell");
        className.Should().Contain("apr");
    }

    [Fact]
    public void GetCellClassName_HandlesInProgress()
    {
        var className = _service.GetCellClassName("inProgress", false);

        className.Should().Contain("prog-cell");
    }

    [Fact]
    public void GetCellClassName_HandlesCarryover()
    {
        var className = _service.GetCellClassName("carryover", false);

        className.Should().Contain("carry-cell");
    }

    [Fact]
    public void GetCellClassName_HandlesBlockers()
    {
        var className = _service.GetCellClassName("blockers", false);

        className.Should().Contain("block-cell");
    }

    [Fact]
    public void GetDotColor_ReturnsGreenForShipped()
    {
        var color = _service.GetDotColor("shipped");

        color.Should().Be("#34A853");
    }

    [Fact]
    public void GetDotColor_ReturnsBlueForInProgress()
    {
        var color = _service.GetDotColor("inProgress");

        color.Should().Be("#0078D4");
    }

    [Fact]
    public void GetDotColor_ReturnsYellowForCarryover()
    {
        var color = _service.GetDotColor("carryover");

        color.Should().Be("#F4B400");
    }

    [Fact]
    public void GetDotColor_ReturnsRedForBlockers()
    {
        var color = _service.GetDotColor("blockers");

        color.Should().Be("#EA4335");
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsCorrectClassForShipped()
    {
        var className = _service.GetStatusHeaderClassName("shipped");

        className.Should().Be("ship-hdr");
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsCorrectClassForInProgress()
    {
        var className = _service.GetStatusHeaderClassName("inProgress");

        className.Should().Be("prog-hdr");
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsCorrectClassForCarryover()
    {
        var className = _service.GetStatusHeaderClassName("carryover");

        className.Should().Be("carry-hdr");
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsCorrectClassForBlockers()
    {
        var className = _service.GetStatusHeaderClassName("blockers");

        className.Should().Be("block-hdr");
    }

    [Fact]
    public void GenerateSvgDiamond_ProducesValidSvgElement()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400");

        svg.Should().Contain("<path");
        svg.Should().Contain("#F4B400");
    }

    [Fact]
    public void GenerateSvgDiamond_WithFilter_IncludesFilter()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", true);

        svg.Should().Contain("filter");
    }

    [Fact]
    public void GenerateSvgDiamond_WithoutFilter_OmitsFilter()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400", false);

        svg.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateSvgCircle_ProducesValidCircle()
    {
        var svg = _service.GenerateSvgCircle(100, 50, 6, "#999", "#888", 1);

        svg.Should().Contain("<circle");
        svg.Should().Contain("#999");
    }

    [Fact]
    public void GenerateSvgCircle_SetsCorrectRadius()
    {
        var svg = _service.GenerateSvgCircle(100, 50, 8, "#999", "#888", 1);

        svg.Should().Contain("r=\"8\"");
    }

    [Fact]
    public void GenerateSvgLine_ProducesValidLine()
    {
        var svg = _service.GenerateSvgLine(0, 100, 200, 100, "#EA4335", 2);

        svg.Should().Contain("<line");
        svg.Should().Contain("#EA4335");
    }

    [Fact]
    public void GenerateSvgLine_WithDasharray_IncludesDasharray()
    {
        var svg = _service.GenerateSvgLine(0, 100, 200, 100, "#EA4335", 2, "5,5");

        svg.Should().Contain("stroke-dasharray");
    }

    [Fact]
    public void GetMilestoneShapes_ReturnsAllThreeTypes()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes.Should().ContainKey("poc");
        shapes.Should().ContainKey("release");
        shapes.Should().ContainKey("checkpoint");
    }

    [Fact]
    public void GetMilestoneShapes_PocIsYellowDiamond()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["poc"].Shape.Should().Be("diamond");
        shapes["poc"].Color.Should().Be("#F4B400");
    }

    [Fact]
    public void GetMilestoneShapes_ReleaseIsGreenDiamond()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["release"].Shape.Should().Be("diamond");
        shapes["release"].Color.Should().Be("#34A853");
    }

    [Fact]
    public void GetMilestoneShapes_CheckpointIsGrayCircle()
    {
        var shapes = _service.GetMilestoneShapes();

        shapes["checkpoint"].Shape.Should().Be("circle");
        shapes["checkpoint"].Color.Should().Be("#999");
    }
}