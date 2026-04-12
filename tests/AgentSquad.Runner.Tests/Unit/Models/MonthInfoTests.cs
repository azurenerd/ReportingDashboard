using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class MonthInfoTests
{
    [Fact]
    public void MonthInfo_HasDefaultEmptyName()
    {
        var monthInfo = new MonthInfo();
        monthInfo.Name.Should().Be(string.Empty);
    }

    [Fact]
    public void MonthInfo_HasDefaultZeroYear()
    {
        var monthInfo = new MonthInfo();
        monthInfo.Year.Should().Be(0);
    }

    [Fact]
    public void MonthInfo_HasDefaultZeroGridColumnIndex()
    {
        var monthInfo = new MonthInfo();
        monthInfo.GridColumnIndex.Should().Be(0);
    }

    [Fact]
    public void MonthInfo_HasDefaultFalseIsCurrentMonth()
    {
        var monthInfo = new MonthInfo();
        monthInfo.IsCurrentMonth.Should().BeFalse();
    }

    [Fact]
    public void MonthInfo_CanSetName()
    {
        var monthInfo = new MonthInfo { Name = "March" };
        monthInfo.Name.Should().Be("March");
    }

    [Fact]
    public void MonthInfo_CanSetYear()
    {
        var monthInfo = new MonthInfo { Year = 2026 };
        monthInfo.Year.Should().Be(2026);
    }

    [Fact]
    public void MonthInfo_CanSetStartDate()
    {
        var date = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthInfo = new MonthInfo { StartDate = date };
        monthInfo.StartDate.Should().Be(date);
    }

    [Fact]
    public void MonthInfo_CanSetEndDate()
    {
        var date = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc);
        var monthInfo = new MonthInfo { EndDate = date };
        monthInfo.EndDate.Should().Be(date);
    }

    [Fact]
    public void MonthInfo_CanSetIsCurrentMonth()
    {
        var monthInfo = new MonthInfo { IsCurrentMonth = true };
        monthInfo.IsCurrentMonth.Should().BeTrue();
    }

    [Fact]
    public void MonthInfo_CanSetGridColumnIndex()
    {
        var monthInfo = new MonthInfo { GridColumnIndex = 2 };
        monthInfo.GridColumnIndex.Should().Be(2);
    }

    [Fact]
    public void MonthInfo_SupportsAllMonthNames()
    {
        var monthNames = new[] { "January", "February", "March", "April", "May", "June",
                                 "July", "August", "September", "October", "November", "December" };

        foreach (var month in monthNames)
        {
            var monthInfo = new MonthInfo { Name = month };
            monthInfo.Name.Should().Be(month);
        }
    }

    [Fact]
    public void MonthInfo_CanBeInitializedWithCompleteData()
    {
        var startDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 4, 30, 23, 59, 59, DateTimeKind.Utc);

        var monthInfo = new MonthInfo
        {
            Name = "April",
            Year = 2026,
            StartDate = startDate,
            EndDate = endDate,
            GridColumnIndex = 1,
            IsCurrentMonth = true
        };

        monthInfo.Name.Should().Be("April");
        monthInfo.Year.Should().Be(2026);
        monthInfo.StartDate.Should().Be(startDate);
        monthInfo.EndDate.Should().Be(endDate);
        monthInfo.GridColumnIndex.Should().Be(1);
        monthInfo.IsCurrentMonth.Should().BeTrue();
    }

    [Fact]
    public void MonthInfo_EndDateIsGreaterThanStartDate()
    {
        var startDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 4, 30, 23, 59, 59, DateTimeKind.Utc);

        var monthInfo = new MonthInfo
        {
            StartDate = startDate,
            EndDate = endDate
        };

        monthInfo.EndDate.Should().BeGreaterThan(monthInfo.StartDate);
    }

    [Fact]
    public void MonthInfo_GridColumnIndexesAreSequential()
    {
        var months = new List<MonthInfo>
        {
            new MonthInfo { Name = "March", GridColumnIndex = 0 },
            new MonthInfo { Name = "April", GridColumnIndex = 1 },
            new MonthInfo { Name = "May", GridColumnIndex = 2 },
            new MonthInfo { Name = "June", GridColumnIndex = 3 }
        };

        for (int i = 0; i < months.Count; i++)
        {
            months[i].GridColumnIndex.Should().Be(i);
        }
    }
}