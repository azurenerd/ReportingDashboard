#nullable enable

using AgentSquad.Runner.Config;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

public class VisualizationServiceTests
{
    private readonly ILogger<VisualizationService> mockLogger;
    private readonly VisualizationService service;

    public VisualizationServiceTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        mockLogger = loggerFactory.CreateLogger<VisualizationService>();
        service = new VisualizationService(mockLogger);
    }

    [Theory]
    [InlineData("shipped", false, "ship-cell")]
    [InlineData("shipped", true, "ship-cell apr")]
    [InlineData("inProgress", false, "prog-cell")]
    [InlineData("inProgress", true, "prog-cell apr")]
    [InlineData("carryover", false, "carry-cell")]
    [InlineData("carryover", true, "carry-cell apr")]
    [InlineData("blockers", false, "block-cell")]
    [InlineData("blockers", true, "block-cell apr")]
    public void GetCellClassName_ReturnsCorrectCssClass(string status, bool isCurrentMonth, string expected)
    {
        var result = service.GetCellClassName(status, isCurrentMonth);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("shipped", "#34A853")]
    [InlineData("inProgress", "#0078D4")]
    [InlineData("carryover", "#F4B400")]
    [InlineData("blockers", "#EA4335")]
    public void GetDotColor_ReturnsCorrectColorForStatus(string status, string expected)
    {
        var result = service.GetDotColor(status);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("shipped", "ship-hdr")]
    [InlineData("inProgress", "prog-hdr")]
    [InlineData("carryover", "carry-hdr")]
    [InlineData("blockers", "block-hdr")]
    public void GetStatusHeaderClassName_ReturnsCorrectHeaderClass(string status, string expected)
    {
        var result = service.GetStatusHeaderClassName(status);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateSvgDiamond_ReturnsValidSvgPolygon()
    {
        var result = service.GenerateSvgDiamond(100, 90, "#F4B400", true);

        Assert.Contains("polygon", result);
        Assert.Contains("100,84", result); // cy - half
        Assert.Contains("106,90", result); // cx + half, cy
        Assert.Contains("100,96", result); // cx, cy + half
        Assert.Contains("94,90", result); // cx - half, cy
        Assert.Contains("fill=\"#F4B400\"", result);
        Assert.Contains("filter=\"url(#drop-shadow)\"", result);
    }

    [Fact]
    public void GenerateSvgDiamond_WithoutFilter_OmitsFilterAttribute()
    {
        var result = service.GenerateSvgDiamond(100, 90, "#34A853", false);

        Assert.Contains("polygon", result);
        Assert.DoesNotContain("filter", result);
    }

    [Fact]
    public void GenerateSvgCircle_ReturnsValidSvgCircle()
    {
        var result = service.GenerateSvgCircle(100, 90, 5, "#999", "#666", 1);

        Assert.Contains("circle", result);
        Assert.Contains("cx=\"100\"", result);
        Assert.Contains("cy=\"90\"", result);
        Assert.Contains("r=\"5\"", result);
        Assert.Contains("fill=\"#999\"", result);
        Assert.Contains("stroke=\"#666\"", result);
        Assert.Contains("stroke-width=\"1\"", result);
    }

    [Fact]
    public void GenerateSvgLine_ReturnsValidSvgLine()
    {
        var result = service.GenerateSvgLine(0, 90, 1560, 90, "#CCC", 1);

        Assert.Contains("line", result);
        Assert.Contains("x1=\"0\"", result);
        Assert.Contains("y1=\"90\"", result);
        Assert.Contains("x2=\"1560\"", result);
        Assert.Contains("y2=\"90\"", result);
        Assert.Contains("stroke=\"#CCC\"", result);
        Assert.Contains("stroke-width=\"1\"", result);
    }

    [Fact]
    public void GenerateSvgLine_WithDasharray_IncludesDasharrayAttribute()
    {
        var result = service.GenerateSvgLine(100, 0, 100, 185, "#EA4335", 2, "5,5");

        Assert.Contains("stroke-dasharray=\"5,5\"", result);
    }

    [Fact]
    public void GenerateSvgLine_WithoutDasharray_OmitsDasharrayAttribute()
    {
        var result = service.GenerateSvgLine(100, 0, 100, 185, "#EA4335", 2);

        Assert.DoesNotContain("stroke-dasharray", result);
    }

    [Fact]
    public void GetMilestoneShapes_WithNullMilestones_ReturnsEmptyList()
    {
        var result = service.GetMilestoneShapes(null!, new DateTime(2026, 1, 1), "#EA4335");

        Assert.Empty(result);
    }

    [Fact]
    public void GetMilestoneShapes_WithEmptyMilestones_ReturnsEmptyList()
    {
        var result = service.GetMilestoneShapes(new List<Milestone>(), new DateTime(2026, 1, 1), "#EA4335");

        Assert.Empty(result);
    }

    [Fact]
    public void GetMilestoneShapes_WithValidMilestones_ReturnsShapeStrings()
    {
        var milestones = new List<Milestone>
        {
            new() { Id = "m1", Label = "PoC", Date = "2026-03-15", Type = "poc" },
            new() { Id = "m2", Label = "Checkpoint", Date = "2026-04-01", Type = "checkpoint" },
            new() { Id = "m3", Label = "Release", Date = "2026-06-30", Type = "release" }
        };

        var result = service.GetMilestoneShapes(milestones, new DateTime(2026, 1, 1), "#EA4335");

        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.NotNull(r));
        Assert.All(result, r => Assert.NotEmpty(r));
    }

    [Fact]
    public void GetMilestoneShapes_CheckpointReturnsCircle()
    {
        var milestones = new List<Milestone>
        {
            new() { Id = "cp1", Label = "Checkpoint", Date = "2026-04-01", Type = "checkpoint" }
        };

        var result = service.GetMilestoneShapes(milestones, new DateTime(2026, 1, 1), "#EA4335");

        Assert.Single(result);
        Assert.Contains("circle", result[0]);
    }

    [Fact]
    public void GetMilestoneShapes_PoCReturnsPolygon()
    {
        var milestones = new List<Milestone>
        {
            new() { Id = "poc1", Label = "PoC", Date = "2026-03-15", Type = "poc" }
        };

        var result = service.GetMilestoneShapes(milestones, new DateTime(2026, 1, 1), "#EA4335");

        Assert.Single(result);
        Assert.Contains("polygon", result[0]);
    }

    [Fact]
    public void GetMilestoneShapes_ReleaseReturnsPolygon()
    {
        var milestones = new List<Milestone>
        {
            new() { Id = "rel1", Label = "Release", Date = "2026-06-30", Type = "release" }
        };

        var result = service.GetMilestoneShapes(milestones, new DateTime(2026, 1, 1), "#EA4335");

        Assert.Single(result);
        Assert.Contains("polygon", result[0]);
    }

    [Fact]
    public void GetMilestoneShapes_WithInvalidDate_SkipsMilestone()
    {
        var milestones = new List<Milestone>
        {
            new() { Id = "m1", Label = "PoC", Date = "invalid-date", Type = "poc" },
            new() { Id = "m2", Label = "Release", Date = "2026-06-30", Type = "release" }
        };

        var result = service.GetMilestoneShapes(milestones, new DateTime(2026, 1, 1), "#EA4335");

        Assert.Single(result);
    }
}