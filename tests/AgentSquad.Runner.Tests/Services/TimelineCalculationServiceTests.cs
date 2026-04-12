using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class TimelineCalculationServiceTests
{
    private readonly TimelineCalculationService _service = new();

    [Fact]
    public void DateToPixel_WithJanuary1_ReturnsZero()
    {
        var date = new DateTime(2026, 1, 1);
        int result = _service.DateToPixel(date);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToPixel_WithJune30_ReturnsMaxWidth()
    {
        var date = new DateTime(2026, 6, 30);
        int result = _service.DateToPixel(date);
        Assert.Equal(1560, result);
    }

    [Fact]
    public void DateToPixel_WithApril12_ReturnsValidPosition()
    {
        var date = new DateTime(2026, 4, 12);
        int result = _service.DateToPixel(date);
        Assert.True(result > 700 && result < 950);
    }

    [Fact]
    public void DateToPixel_WithPastDate_ReturnsZero()
    {
        var date = new DateTime(2025, 12, 31);
        int result = _service.DateToPixel(date);
        Assert.Equal(0, result);
    }

    [Fact]
    public void DateToPixel_WithFutureDate_ReturnsMaxWidth()
    {
        var date = new DateTime(2026, 12, 31);
        int result = _service.DateToPixel(date);
        Assert.Equal(1560, result);
    }

    [Fact]
    public void GetMonthPosition_WithMonthIndex0_ReturnsZero()
    {
        int result = _service.GetMonthPosition(0);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetMonthPosition_WithMonthIndex3_Returns780()
    {
        int result = _service.GetMonthPosition(3);
        Assert.Equal(780, result);
    }

    [Fact]
    public void GetMonthPosition_WithMonthIndex5_Returns1300()
    {
        int result = _service.GetMonthPosition(5);
        Assert.Equal(1300, result);
    }

    [Fact]
    public void GetMonthPosition_WithInvalidIndex_ReturnsZero()
    {
        int result = _service.GetMonthPosition(10);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetNowLinePosition_ReturnsValidPosition()
    {
        int result = _service.GetNowLinePosition();
        Assert.True(result > 700 && result < 950);
    }

    [Fact]
    public void CalculateMilestoneYPosition_WithThreeMilestones_ReturnsEvenSpacing()
    {
        int y0 = _service.CalculateMilestoneYPosition(0, 3);
        int y1 = _service.CalculateMilestoneYPosition(1, 3);
        int y2 = _service.CalculateMilestoneYPosition(2, 3);

        Assert.Equal(46, y0);
        Assert.Equal(92, y1);
        Assert.Equal(138, y2);
    }

    [Fact]
    public void CalculateMilestoneYPosition_WithSingleMilestone_ReturnsCentered()
    {
        int result = _service.CalculateMilestoneYPosition(0, 1);
        Assert.Equal(92, result);
    }

    [Fact]
    public void GetMonthName_WithValidIndex_ReturnsCorrectName()
    {
        Assert.Equal("Jan", _service.GetMonthName(0));
        Assert.Equal("Feb", _service.GetMonthName(1));
        Assert.Equal("Mar", _service.GetMonthName(2));
        Assert.Equal("Apr", _service.GetMonthName(3));
        Assert.Equal("May", _service.GetMonthName(4));
        Assert.Equal("Jun", _service.GetMonthName(5));
    }

    [Fact]
    public void GetMonthName_WithInvalidIndex_ReturnsEmpty()
    {
        string result = _service.GetMonthName(10);
        Assert.Equal(string.Empty, result);
    }
}