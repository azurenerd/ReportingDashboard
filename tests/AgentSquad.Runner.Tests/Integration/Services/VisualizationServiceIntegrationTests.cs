using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class VisualizationServiceIntegrationTests
{
    private readonly VisualizationService _service;

    public VisualizationServiceIntegrationTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<VisualizationService>();
        _service = new VisualizationService(logger);
    }

    [Fact]
    public void GetCellClassName_ReturnsConsistentValues()
    {
        var className1 = _service.GetCellClassName("shipped", false);
        var className2 = _service.GetCellClassName("shipped", false);
        
        className1.Should().Be(className2);
    }

    [Fact]
    public void GetDotColor_ReturnConsistentHexColors()
    {
        var color1 = _service.GetDotColor("shipped");
        var color2 = _service.GetDotColor("shipped");
        
        color1.Should().Be(color2);
        color1.Should().Match("#*");
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsValidCssClasses()
    {
        var statuses = new[] { "shipped", "inProgress", "carryover", "blockers" };
        
        foreach (var status in statuses)
        {
            var className = _service.GetStatusHeaderClassName(status);
            className.Should().NotBeNullOrEmpty();
            className.Should().Contain("hdr");
        }
    }

    [Fact]
    public void GenerateSvgDiamond_ProducesValidMarkup()
    {
        var svg = _service.GenerateSvgDiamond(100, 50, "#F4B400");
        
        svg.Should().Contain("<polygon");
        svg.Should().Contain("points=");
        svg.Should().Contain("fill=\"#F4B400\"");
        svg.Should().NotContain("polygon>");
    }

    [Fact]
    public void GenerateSvgCircle_ProducesValidMarkup()
    {
        var svg = _service.GenerateSvgCircle(100, 50, 8, "#999", "#888", 1);
        
        svg.Should().Contain("<circle");
        svg.Should().Contain("cx=\"100\"");
        svg.Should().Contain("cy=\"50\"");
        svg.Should().Contain("r=\"8\"");
    }

    [Fact]
    public void GenerateSvgLine_ProducesValidMarkup()
    {
        var svg = _service.GenerateSvgLine(0, 50, 100, 50, "#EA4335", 2);
        
        svg.Should().Contain("<line");
        svg.Should().Contain("x1=\"0\"");
        svg.Should().Contain("x2=\"100\"");
        svg.Should().Contain("y1=\"50\"");
        svg.Should().Contain("y2=\"50\"");
        svg.Should().Contain("stroke=\"#EA4335\"");
    }

    [Fact]
    public void GenerateSvgLine_WithDasharray_ProducesValidDashedMarkup()
    {
        var svg = _service.GenerateSvgLine(0, 100, 200, 100, "#EA4335", 2, "5,3");
        
        svg.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void GetMilestoneShapes_ContainsAllShapeTypes()
    {
        var shapes = _service.GetMilestoneShapes();
        
        shapes.Should().HaveCount(3);
        shapes.Should().ContainKey("poc");
        shapes.Should().ContainKey("release");
        shapes.Should().ContainKey("checkpoint");
    }

    [Fact]
    public void GetMilestoneShapes_PocHasCorrectMetadata()
    {
        var shapes = _service.GetMilestoneShapes();
        
        shapes["poc"].Type.Should().Be("poc");
        shapes["poc"].Shape.Should().Be("diamond");
        shapes["poc"].Color.Should().StartWith("#");
        shapes["poc"].Size.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetMilestoneShapes_ReleaseHasCorrectMetadata()
    {
        var shapes = _service.GetMilestoneShapes();
        
        shapes["release"].Type.Should().Be("release");
        shapes["release"].Shape.Should().Be("diamond");
        shapes["release"].Color.Should().StartWith("#");
        shapes["release"].Size.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetMilestoneShapes_CheckpointHasCorrectMetadata()
    {
        var shapes = _service.GetMilestoneShapes();
        
        shapes["checkpoint"].Type.Should().Be("checkpoint");
        shapes["checkpoint"].Shape.Should().Be("circle");
        shapes["checkpoint"].Color.Should().StartWith("#");
        shapes["checkpoint"].Size.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetStatusHeaderText_ReturnsAllValidStatuses()
    {
        var statuses = new[] { "shipped", "inProgress", "carryover", "blockers" };
        
        foreach (var status in statuses)
        {
            var text = _service.GetStatusHeaderText(status);
            text.Should().NotBeNullOrEmpty();
            text.Should().NotBe(status);
        }
    }

    [Fact]
    public void AllColorsAreValidHexCodes()
    {
        var shapes = _service.GetMilestoneShapes();
        
        foreach (var shape in shapes.Values)
        {
            shape.Color.Should().Match("#[A-Fa-f0-9]{6}");
        }
    }

    [Fact]
    public void SvgGenerationCoordinatesAreValid()
    {
        var svgLine = _service.GenerateSvgLine(0, 0, 1560, 150, "#ccc", 1);
        
        svgLine.Should().Contain("1560");
        svgLine.Should().Contain("150");
    }
}