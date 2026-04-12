using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class MonthInfoTests
{
    [Fact]
    public void MonthInfo_CanSetProperties()
    {
        var startDate = new DateTime(2026, 3, 1);
        var endDate = new DateTime(2026, 3, 31);

        var monthInfo = new MonthInfo
        {
            Name = "March",
            Year = 2026,
            StartDate = startDate,
            EndDate = endDate,
            GridColumnIndex = 0,
            IsCurrentMonth = false
        };

        monthInfo.Name.Should().Be("March");
        monthInfo.Year.Should().Be(2026);
        monthInfo.GridColumnIndex.Should().Be(0);
    }

    [Fact]
    public void MonthInfo_CanMarkAsCurrentMonth()
    {
        var monthInfo = new MonthInfo { IsCurrentMonth = true };

        monthInfo.IsCurrentMonth.Should().BeTrue();
    }

    [Fact]
    public void MonthInfo_StartDateIsBeforeEndDate()
    {
        var startDate = new DateTime(2026, 3, 1);
        var endDate = new DateTime(2026, 3, 31);

        var monthInfo = new MonthInfo { StartDate = startDate, EndDate = endDate };

        monthInfo.StartDate.Should().BeBefore(monthInfo.EndDate);
    }

    [Fact]
    public void MonthInfo_GridColumnIndexCanBeMultipleValues()
    {
        var month0 = new MonthInfo { GridColumnIndex = 0 };
        var month1 = new MonthInfo { GridColumnIndex = 1 };
        var month3 = new MonthInfo { GridColumnIndex = 3 };

        month0.GridColumnIndex.Should().Be(0);
        month1.GridColumnIndex.Should().Be(1);
        month3.GridColumnIndex.Should().Be(3);
    }
}