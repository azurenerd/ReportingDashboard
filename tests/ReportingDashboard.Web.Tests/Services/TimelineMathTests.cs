using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Services;

public class TimelineMathTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 6, 30);
    private const double Width = 1560d;

    // ----- DateToX --------------------------------------------------------

    [Fact]
    public void DateToX_At_Start_Is_Zero()
    {
        TimelineMath.DateToX(Start, Start, End, Width).Should().Be(0);
    }

    [Fact]
    public void DateToX_At_End_Is_Width()
    {
        TimelineMath.DateToX(End, Start, End, Width).Should().Be(Width);
    }

    [Fact]
    public void DateToX_At_Midpoint_Is_Half_Width()
    {
        var mid = Start.AddDays((End.DayNumber - Start.DayNumber) / 2);
        TimelineMath.DateToX(mid, Start, End, Width)
            .Should().BeApproximately(Width / 2d, 0.001);
    }

    [Fact]
    public void DateToX_Apr_19_2026_Matches_Reference_Design()
    {
        // Apr 19 = day 108 of the 180-day window -> ~936 on a 1560-wide canvas,
        // within the PM-spec "~974" approximate tolerance.
        var x = TimelineMath.DateToX(new DateOnly(2026, 4, 19), Start, End, Width);
        x.Should().BeApproximately(974, 100);
        x.Should().BeApproximately(108d / 180d * Width, 0.001);
    }

    [Fact]
    public void DateToX_May_19_2026_Matches_Reference_Design()
    {
        // May 19 is 30 days after Apr 19, so x advances by one month unit (260).
        var apr = TimelineMath.DateToX(new DateOnly(2026, 4, 19), Start, End, Width);
        var may = TimelineMath.DateToX(new DateOnly(2026, 5, 19), Start, End, Width);
        may.Should().BeApproximately(1234, 100);
        (may - apr).Should().BeApproximately(260, 0.001);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void DateToX_Rejects_Non_Positive_Width(double badWidth)
    {
        Action act = () => TimelineMath.DateToX(Start, Start, End, badWidth);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DateToX_Rejects_End_Not_After_Start()
    {
        Action act = () => TimelineMath.DateToX(Start, End, Start, Width);
        act.Should().Throw<ArgumentException>();
    }

    // ----- MonthGridlines -------------------------------------------------

    [Fact]
    public void MonthGridlines_Produces_Six_Entries_Jan_Through_Jun()
    {
        var lines = TimelineMath.MonthGridlines(Start, End, Width);

        lines.Should().HaveCount(6);
        lines.Select(g => g.Label).Should().ContainInOrder(
            "Jan", "Feb", "Mar", "Apr", "May", "Jun");

        lines[0].X.Should().Be(0);
        lines.Select(g => g.X).Should().BeInAscendingOrder();
        lines.Should().OnlyContain(g => g.X >= 0 && g.X <= Width);
    }

    [Fact]
    public void MonthGridlines_Rejects_Invalid_Range()
    {
        Action act = () => TimelineMath.MonthGridlines(End, Start, Width);
        act.Should().Throw<ArgumentException>();
    }

    // ----- NowX -----------------------------------------------------------

    [Fact]
    public void NowX_In_Range_Returns_Proportional_Position()
    {
        var now = TimelineMath.NowX(new DateOnly(2026, 4, 19), Start, End, Width);
        now.InRange.Should().BeTrue();
        now.X.Should().BeApproximately(974, 100);
    }

    [Fact]
    public void NowX_Before_Range_Clamps_To_Zero_And_Flags_OutOfRange()
    {
        var now = TimelineMath.NowX(new DateOnly(2025, 12, 15), Start, End, Width);
        now.InRange.Should().BeFalse();
        now.X.Should().Be(0);
    }

    [Fact]
    public void NowX_After_Range_Clamps_To_Width_And_Flags_OutOfRange()
    {
        var now = TimelineMath.NowX(new DateOnly(2026, 7, 15), Start, End, Width);
        now.InRange.Should().BeFalse();
        now.X.Should().Be(Width);
    }

    // ----- CurrentMonthIndex ---------------------------------------------

    [Fact]
    public void CurrentMonthIndex_Returns_Index_When_Match_Found()
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr" };
        TimelineMath.CurrentMonthIndex(new DateOnly(2026, 4, 19), months).Should().Be(3);
    }

    [Fact]
    public void CurrentMonthIndex_Matches_Case_Insensitively_And_Long_Names()
    {
        var months = new[] { "January", "FEBRUARY", "march", "April" };
        TimelineMath.CurrentMonthIndex(new DateOnly(2026, 2, 1), months).Should().Be(1);
    }

    [Fact]
    public void CurrentMonthIndex_Returns_Minus_One_When_No_Match()
    {
        var months = new[] { "Mar", "Apr", "May", "Jun" };
        TimelineMath.CurrentMonthIndex(new DateOnly(2026, 1, 15), months).Should().Be(-1);
    }

    // ----- TruncateItems --------------------------------------------------

    [Fact]
    public void TruncateItems_With_Zero_Items_Returns_Empty_And_No_Overflow()
    {
        var (kept, overflow) = TimelineMath.TruncateItems(Array.Empty<string>(), 4);
        kept.Should().BeEmpty();
        overflow.Should().Be(0);
    }

    [Fact]
    public void TruncateItems_At_Max_Keeps_All_Items_With_No_Overflow()
    {
        var items = new[] { "a", "b", "c", "d" };
        var (kept, overflow) = TimelineMath.TruncateItems(items, 4);
        kept.Should().Equal("a", "b", "c", "d");
        overflow.Should().Be(0);
    }

    [Fact]
    public void TruncateItems_Above_Max_Keeps_First_Max_Minus_One_And_Reports_Overflow()
    {
        // 7 items, max=4 -> keep first 3, overflow=4 (renders as "+4 more").
        var items = new[] { "a", "b", "c", "d", "e", "f", "g" };
        var (kept, overflow) = TimelineMath.TruncateItems(items, 4);
        kept.Should().Equal("a", "b", "c");
        overflow.Should().Be(4);
    }

    [Fact]
    public void TruncateItems_Rejects_Invalid_Max()
    {
        Action act = () => TimelineMath.TruncateItems(new[] { "a" }, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
