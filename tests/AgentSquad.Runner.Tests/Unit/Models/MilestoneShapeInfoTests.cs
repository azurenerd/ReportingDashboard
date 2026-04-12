using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class MilestoneShapeInfoTests
{
    [Fact]
    public void MilestoneShapeInfo_CanSetProperties()
    {
        var shapeInfo = new MilestoneShapeInfo
        {
            Type = "poc",
            Shape = "diamond",
            Color = "#F4B400",
            Size = 12
        };

        shapeInfo.Type.Should().Be("poc");
        shapeInfo.Shape.Should().Be("diamond");
        shapeInfo.Color.Should().Be("#F4B400");
        shapeInfo.Size.Should().Be(12);
    }

    [Fact]
    public void MilestoneShapeInfo_CanRepresentDiamond()
    {
        var shapeInfo = new MilestoneShapeInfo { Shape = "diamond" };

        shapeInfo.Shape.Should().Be("diamond");
    }

    [Fact]
    public void MilestoneShapeInfo_CanRepresentCircle()
    {
        var shapeInfo = new MilestoneShapeInfo { Shape = "circle" };

        shapeInfo.Shape.Should().Be("circle");
    }

    [Fact]
    public void MilestoneShapeInfo_ColorIsValidHexFormat()
    {
        var shapeInfo = new MilestoneShapeInfo { Color = "#F4B400" };

        shapeInfo.Color.Should().MatchRegex(@"^#[0-9A-Fa-f]{6}$");
    }

    [Fact]
    public void MilestoneShapeInfo_SizeIsPositive()
    {
        var shapeInfo = new MilestoneShapeInfo { Size = 12 };

        shapeInfo.Size.Should().BeGreaterThan(0);
    }
}