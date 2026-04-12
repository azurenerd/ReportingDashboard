using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class MilestoneShapeInfoTests
{
    [Fact]
    public void MilestoneShapeInfo_HasDefaultEmptyType()
    {
        var shapeInfo = new MilestoneShapeInfo();
        shapeInfo.Type.Should().Be(string.Empty);
    }

    [Fact]
    public void MilestoneShapeInfo_HasDefaultEmptyShape()
    {
        var shapeInfo = new MilestoneShapeInfo();
        shapeInfo.Shape.Should().Be(string.Empty);
    }

    [Fact]
    public void MilestoneShapeInfo_HasDefaultEmptyColor()
    {
        var shapeInfo = new MilestoneShapeInfo();
        shapeInfo.Color.Should().Be(string.Empty);
    }

    [Fact]
    public void MilestoneShapeInfo_HasDefaultZeroSize()
    {
        var shapeInfo = new MilestoneShapeInfo();
        shapeInfo.Size.Should().Be(0);
    }

    [Fact]
    public void MilestoneShapeInfo_CanSetType()
    {
        var shapeInfo = new MilestoneShapeInfo { Type = "poc" };
        shapeInfo.Type.Should().Be("poc");
    }

    [Fact]
    public void MilestoneShapeInfo_CanSetShape()
    {
        var shapeInfo = new MilestoneShapeInfo { Shape = "diamond" };
        shapeInfo.Shape.Should().Be("diamond");
    }

    [Fact]
    public void MilestoneShapeInfo_CanSetColor()
    {
        var shapeInfo = new MilestoneShapeInfo { Color = "#F4B400" };
        shapeInfo.Color.Should().Be("#F4B400");
    }

    [Fact]
    public void MilestoneShapeInfo_CanSetSize()
    {
        var shapeInfo = new MilestoneShapeInfo { Size = 12 };
        shapeInfo.Size.Should().Be(12);
    }

    [Fact]
    public void MilestoneShapeInfo_CanBeInitializedWithCompleteData()
    {
        var shapeInfo = new MilestoneShapeInfo
        {
            Type = "release",
            Shape = "diamond",
            Color = "#34A853",
            Size = 12
        };

        shapeInfo.Type.Should().Be("release");
        shapeInfo.Shape.Should().Be("diamond");
        shapeInfo.Color.Should().Be("#34A853");
        shapeInfo.Size.Should().Be(12);
    }

    [Fact]
    public void MilestoneShapeInfo_SupportsValidShapeTypes()
    {
        var validShapes = new[] { "diamond", "circle", "line" };

        foreach (var shape in validShapes)
        {
            var shapeInfo = new MilestoneShapeInfo { Shape = shape };
            shapeInfo.Shape.Should().Be(shape);
        }
    }

    [Fact]
    public void MilestoneShapeInfo_SupportsHexColorCodes()
    {
        var hexColors = new[] { "#F4B400", "#34A853", "#999", "#EA4335", "#0078D4" };

        foreach (var color in hexColors)
        {
            var shapeInfo = new MilestoneShapeInfo { Color = color };
            shapeInfo.Color.Should().Be(color);
        }
    }

    [Fact]
    public void MilestoneShapeInfo_SizeCanVary()
    {
        var sizes = new[] { 5, 7, 8, 12, 16 };

        foreach (var size in sizes)
        {
            var shapeInfo = new MilestoneShapeInfo { Size = size };
            shapeInfo.Size.Should().Be(size);
        }
    }
}