using FluentAssertions;
using ReportingDashboard.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Unit tests for TimelineHelper static methods covering CalculateX, CalculateXClamped,
/// GetMonthGridlines, GetYLane, GetLabelAbove, DiamondPoints, F, and RenderSvgText.
/// </summary>
[Trait("Category", "Unit")]
public class TimelineHelperTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 7, 1);

    // --- CalculateX ---

    [Fact]
    public void CalculateX_StartDate_ReturnsZero()
    {
        var result = TimelineHelper.CalculateX(Start, Start, End);
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateX_EndDate_ReturnsSvgWidth()
    {
        var result = TimelineHelper.CalculateX(End, Start, End);
        result.Should().Be(1560.0);
    }

    [Fact]
    public void CalculateX_MidRange_ReturnsProportionalX()
    {
        // Apr 2 is roughly halfway in a Jan 1 - Jul 1 range (181 days total, ~91 elapsed)
        var mid = new DateOnly(2026, 4, 2);
        var result = TimelineHelper.CalculateX(mid, Start, End);
        result.Should().BeApproximately(780.0, 15.0);
    }

    [Fact]
    public void CalculateX_StringOverload_ParsesDateCorrectly()
    {
        var result = TimelineHelper.CalculateX("2026-07-01", Start, End);
        result.Should().Be(1560.0);
    }

    [Fact]
    public void CalculateX_EqualStartAndEnd_ReturnsZero()
    {
        var same = new DateOnly(2026, 3, 1);
        var result = TimelineHelper.CalculateX(same, same, same);
        result.Should().Be(0.0);
    }

    // --- CalculateXClamped ---

    [Fact]
    public void CalculateXClamped_DateBeforeStart_ClampsToZero()
    {
        var before = new DateOnly(2025, 6, 1);
        var result = TimelineHelper.CalculateXClamped(before, Start, End);
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateXClamped_DateAfterEnd_ClampsToSvgWidth()
    {
        var after = new DateOnly(2027, 1, 1);
        var result = TimelineHelper.CalculateXClamped(after, Start, End);
        result.Should().Be(1560.0);
    }

    [Fact]
    public void CalculateXClamped_DateInRange_ReturnsUnclamped()
    {
        var mid = new DateOnly(2026, 4, 1);
        var unclamped = TimelineHelper.CalculateX(mid, Start, End);
        var clamped = TimelineHelper.CalculateXClamped(mid, Start, End);
        clamped.Should().Be(unclamped);
    }

    // --- GetMonthGridlines ---

    [Fact]
    public void GetMonthGridlines_SixMonthRange_ReturnsCorrectCount()
    {
        // Jan 1 to Jul 1: gridlines at Feb 1, Mar 1, Apr 1, May 1, Jun 1, Jul 1 = 6
        var results = TimelineHelper.GetMonthGridlines(Start, End);
        results.Should().HaveCount(6);
    }

    [Fact]
    public void GetMonthGridlines_LabelsAreThreeLetterAbbreviations()
    {
        var results = TimelineHelper.GetMonthGridlines(Start, End);
        results.Select(r => r.Label).Should().BeEquivalentTo(
            new[] { "Feb", "Mar", "Apr", "May", "Jun", "Jul" });
    }

    // --- GetYLane ---

    [Fact]
    public void GetYLane_ThreeWorkstreams_Returns42_98_154()
    {
        TimelineHelper.GetYLane(0, 3).Should().Be(42);
        TimelineHelper.GetYLane(1, 3).Should().Be(98);
        TimelineHelper.GetYLane(2, 3).Should().Be(154);
    }

    [Fact]
    public void GetYLane_OneWorkstream_ReturnsCentered()
    {
        TimelineHelper.GetYLane(0, 1).Should().Be(98);
    }

    [Fact]
    public void GetYLane_TwoWorkstreams_Returns70And126()
    {
        TimelineHelper.GetYLane(0, 2).Should().Be(70);
        TimelineHelper.GetYLane(1, 2).Should().Be(126);
    }

    [Fact]
    public void GetYLane_ZeroOrNegativeTotal_ReturnsFallback98()
    {
        TimelineHelper.GetYLane(0, 0).Should().Be(98);
        TimelineHelper.GetYLane(0, -1).Should().Be(98);
    }

    // --- GetLabelAbove ---

    [Fact]
    public void GetLabelAbove_ExplicitAbove_ReturnsTrue()
    {
        TimelineHelper.GetLabelAbove("above", 1).Should().BeTrue();
    }

    [Fact]
    public void GetLabelAbove_ExplicitBelow_ReturnsFalse()
    {
        TimelineHelper.GetLabelAbove("below", 0).Should().BeFalse();
    }

    [Fact]
    public void GetLabelAbove_NullPosition_AlternatesByIndex()
    {
        TimelineHelper.GetLabelAbove(null, 0).Should().BeTrue();   // even → above
        TimelineHelper.GetLabelAbove(null, 1).Should().BeFalse();  // odd → below
        TimelineHelper.GetLabelAbove(null, 2).Should().BeTrue();
    }

    // --- DiamondPoints ---

    [Fact]
    public void DiamondPoints_ProducesCorrectFormat()
    {
        var result = TimelineHelper.DiamondPoints(100.0, 50.0, 11.0);
        // cx,cy-11 cx+11,cy cx,cy+11 cx-11,cy
        result.Should().Be("100.0,39.0 111.0,50.0 100.0,61.0 89.0,50.0");
    }

    // --- F (formatting) ---

    [Fact]
    public void F_FormatsWithOneDecimal_InvariantCulture()
    {
        TimelineHelper.F(123.456).Should().Be("123.5");
        TimelineHelper.F(0.0).Should().Be("0.0");
    }

    // --- RenderSvgText ---

    [Fact]
    public void RenderSvgText_ProducesValidSvgTextElement()
    {
        var result = TimelineHelper.RenderSvgText(100.0, 50.0, "#666", "11", "600", "Feb");
        result.Should().Contain("<text ");
        result.Should().Contain("x=\"100.0\"");
        result.Should().Contain("y=\"50.0\"");
        result.Should().Contain("fill=\"#666\"");
        result.Should().Contain("font-size=\"11\"");
        result.Should().Contain(">Feb</text>");
    }

    [Fact]
    public void RenderSvgText_WithTextAnchor_IncludesAnchorAttribute()
    {
        var result = TimelineHelper.RenderSvgText(10.0, 20.0, "#000", "10", "400", "NOW", "middle");
        result.Should().Contain("text-anchor=\"middle\"");
    }

    [Fact]
    public void RenderSvgText_HtmlEncodesContent()
    {
        var result = TimelineHelper.RenderSvgText(0, 0, "#000", "10", "400", "<script>");
        result.Should().Contain("&lt;script&gt;");
        result.Should().NotContain("<script>");
    }
}