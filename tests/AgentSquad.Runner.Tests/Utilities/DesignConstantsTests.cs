using AgentSquad.Runner.Utilities;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Utilities;

public class DesignConstantsTests
{
    [Fact]
    public void ColorConstants_AreValidHexCodes()
    {
        // Assert
        DesignConstants.ColorShipped.Should().Be("#1B7A28");
        DesignConstants.ColorProgress.Should().Be("#1565C0");
        DesignConstants.ColorCarryover.Should().Be("#B45309");
        DesignConstants.ColorBlocker.Should().Be("#991B1B");
        DesignConstants.ColorPoc.Should().Be("#F4B400");
        DesignConstants.ColorProduction.Should().Be("#34A853");
        DesignConstants.ColorNow.Should().Be("#EA4335");
    }

    [Theory]
    [InlineData("shipped", "#1B7A28")]
    [InlineData("Shipped", "#1B7A28")]
    [InlineData("SHIPPED", "#1B7A28")]
    [InlineData("in progress", "#1565C0")]
    [InlineData("In Progress", "#1565C0")]
    [InlineData("carryover", "#B45309")]
    [InlineData("Carryover", "#B45309")]
    [InlineData("blockers", "#991B1B")]
    [InlineData("Blockers", "#991B1B")]
    public void GetColorByStatus_ReturnsCorrectColor(string status, string expectedColor)
    {
        // Act
        var result = DesignConstants.GetColorByStatus(status);

        // Assert
        result.Should().Be(expectedColor);
    }

    [Theory]
    [InlineData("shipped", "#E8F5E9")]
    [InlineData("in progress", "#E3F2FD")]
    [InlineData("carryover", "#FFF8E1")]
    [InlineData("blockers", "#FEF2F2")]
    public void GetBgColorByStatus_ReturnsCorrectBackground(string status, string expectedBgColor)
    {
        // Act
        var result = DesignConstants.GetBgColorByStatus(status);

        // Assert
        result.Should().Be(expectedBgColor);
    }

    [Theory]
    [InlineData("shipped", "#F0FBF0")]
    [InlineData("in progress", "#EEF4FE")]
    [InlineData("carryover", "#FFFDE7")]
    [InlineData("blockers", "#FFF5F5")]
    public void GetCellBgColorByStatus_ReturnsCorrectCellBackground(string status, string expectedBgColor)
    {
        // Act
        var result = DesignConstants.GetCellBgColorByStatus(status);

        // Assert
        result.Should().Be(expectedBgColor);
    }

    [Theory]
    [InlineData("shipped", "ship-hdr")]
    [InlineData("Shipped", "ship-hdr")]
    [InlineData("SHIPPED", "ship-hdr")]
    [InlineData("in progress", "prog-hdr")]
    [InlineData("In Progress", "prog-hdr")]
    [InlineData("carryover", "carry-hdr")]
    [InlineData("Carryover", "carry-hdr")]
    [InlineData("blockers", "block-hdr")]
    [InlineData("Blockers", "block-hdr")]
    public void GetCssClassByStatus_ReturnsCorrectCssClass(string status, string expectedCssClass)
    {
        // Act
        var result = DesignConstants.GetCssClassByStatus(status);

        // Assert
        result.Should().Be(expectedCssClass);
    }

    [Theory]
    [InlineData("shipped", "ship-hdr")]
    [InlineData("Shipped", "ship-hdr")]
    [InlineData("in progress", "prog-hdr")]
    [InlineData("In Progress", "prog-hdr")]
    [InlineData("carryover", "carry-hdr")]
    [InlineData("Carryover", "carry-hdr")]
    [InlineData("blockers", "block-hdr")]
    [InlineData("Blockers", "block-hdr")]
    public void GetHeaderCssClassByStatus_ReturnsCorrectCssClass(string status, string expectedCssClass)
    {
        // Act
        var result = DesignConstants.GetHeaderCssClassByStatus(status);

        // Assert
        result.Should().Be(expectedCssClass);
    }

    [Theory]
    [InlineData("shipped", "ship-cell")]
    [InlineData("Shipped", "ship-cell")]
    [InlineData("in progress", "prog-cell")]
    [InlineData("In Progress", "prog-cell")]
    [InlineData("carryover", "carry-cell")]
    [InlineData("Carryover", "carry-cell")]
    [InlineData("blockers", "block-cell")]
    [InlineData("Blockers", "block-cell")]
    public void GetCellCssClassByStatus_ReturnsCorrectCssClass(string status, string expectedCssClass)
    {
        // Act
        var result = DesignConstants.GetCellCssClassByStatus(status);

        // Assert
        result.Should().Be(expectedCssClass);
    }

    [Fact]
    public void CssClassConstants_AreNonEmpty()
    {
        // Assert
        DesignConstants.CssShipHdr.Should().NotBeEmpty();
        DesignConstants.CssProgHdr.Should().NotBeEmpty();
        DesignConstants.CssCarryHdr.Should().NotBeEmpty();
        DesignConstants.CssBlockHdr.Should().NotBeEmpty();
        DesignConstants.CssShipCell.Should().NotBeEmpty();
        DesignConstants.CssProgCell.Should().NotBeEmpty();
        DesignConstants.CssCarryCell.Should().NotBeEmpty();
        DesignConstants.CssBlockCell.Should().NotBeEmpty();
    }

    [Fact]
    public void LayoutConstants_HaveValidDimensions()
    {
        // Assert
        DesignConstants.HeatmapRowHeaderWidth.Should().Be(160);
        DesignConstants.HeatmapHeaderRowHeight.Should().Be(36);
        DesignConstants.TimelineMonthWidth.Should().Be(260);
        DesignConstants.TimelineSvgWidth.Should().Be(1560);
        DesignConstants.ViewportWidth.Should().Be(1920);
        DesignConstants.ViewportHeight.Should().Be(1080);
    }
}