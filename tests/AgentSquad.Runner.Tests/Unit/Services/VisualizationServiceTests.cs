using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Unit.Services;

public class VisualizationServiceTests
{
    [Fact]
    public void GetCellClassName_ReturnsShipCell_WhenStatusIsShipped()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetCellClassName("shipped", false);

        // Assert
        Assert.Equal("ship-cell", result);
    }

    [Fact]
    public void GetCellClassName_ReturnsProgCell_WhenStatusIsInProgress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetCellClassName("inprogress", false);

        // Assert
        Assert.Equal("prog-cell", result);
    }

    [Fact]
    public void GetCellClassName_ReturnsCarryCell_WhenStatusIsCarryover()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetCellClassName("carryover", false);

        // Assert
        Assert.Equal("carry-cell", result);
    }

    [Fact]
    public void GetCellClassName_ReturnsBlockCell_WhenStatusIsBlockers()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetCellClassName("blockers", false);

        // Assert
        Assert.Equal("block-cell", result);
    }

    [Fact]
    public void GetCellClassName_AppendsAprClass_WhenIsCurrentMonthTrue()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetCellClassName("shipped", true);

        // Assert
        Assert.Equal("ship-cell apr", result);
    }

    [Fact]
    public void GetDotColor_ReturnsGreen_WhenStatusIsShipped()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetDotColor("shipped");

        // Assert
        Assert.Equal("#34A853", result);
    }

    [Fact]
    public void GetDotColor_ReturnsBlue_WhenStatusIsInProgress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetDotColor("inprogress");

        // Assert
        Assert.Equal("#0078D4", result);
    }

    [Fact]
    public void GetDotColor_ReturnsYellow_WhenStatusIsCarryover()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetDotColor("carryover");

        // Assert
        Assert.Equal("#F4B400", result);
    }

    [Fact]
    public void GetDotColor_ReturnsRed_WhenStatusIsBlockers()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetDotColor("blockers");

        // Assert
        Assert.Equal("#EA4335", result);
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsShipHdr_WhenStatusIsShipped()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetStatusHeaderClassName("shipped");

        // Assert
        Assert.Equal("ship-hdr", result);
    }

    [Fact]
    public void GetStatusHeaderClassName_ReturnsProgHdr_WhenStatusIsInProgress()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetStatusHeaderClassName("inprogress");

        // Assert
        Assert.Equal("prog-hdr", result);
    }

    [Fact]
    public void GenerateSvgDiamond_ReturnsSvgMarkup_WithValidCoordinates()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GenerateSvgDiamond(100, 50, "#F4B400");

        // Assert
        Assert.Contains("polygon", result);
        Assert.Contains("100,42", result);
        Assert.Contains("108,50", result);
        Assert.Contains("100,58", result);
        Assert.Contains("92,50", result);
    }

    [Fact]
    public void GenerateSvgCircle_ReturnsSvgMarkup_WithValidAttributes()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GenerateSvgCircle(100, 50, 8, "#999", "#666", 1);

        // Assert
        Assert.Contains("circle", result);
        Assert.Contains("cx=\"100\"", result);
        Assert.Contains("cy=\"50\"", result);
        Assert.Contains("r=\"8\"", result);
    }

    [Fact]
    public void GenerateSvgLine_ReturnsSvgMarkup_WithValidCoordinates()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2);

        // Assert
        Assert.Contains("line", result);
        Assert.Contains("x1=\"0\"", result);
        Assert.Contains("y1=\"100\"", result);
        Assert.Contains("x2=\"1560\"", result);
        Assert.Contains("y2=\"100\"", result);
    }

    [Fact]
    public void GenerateSvgLine_IncludesDasharray_WhenProvided()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GenerateSvgLine(0, 100, 1560, 100, "#EA4335", 2, "5,5");

        // Assert
        Assert.Contains("stroke-dasharray=\"5,5\"", result);
    }

    [Fact]
    public void GetMilestoneShapes_ReturnsThreeShapes()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<VisualizationService>>();
        var service = new VisualizationService(mockLogger.Object);

        // Act
        var result = service.GetMilestoneShapes();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("poc", result.Keys);
        Assert.Contains("release", result.Keys);
        Assert.Contains("checkpoint", result.Keys);
    }
}