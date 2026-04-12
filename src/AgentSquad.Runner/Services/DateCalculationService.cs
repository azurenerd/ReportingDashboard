using NodaTime;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DateCalculationService : IDateCalculationService
{
    private const int PixelsPerMonth = 260;
    private const int SvgWidth = 1560;
    private const int MonthsInTimeline = 6;
    private readonly ILogger<DateCalculationService> _logger;

    public DateCalculationService(ILogger<DateCalculationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public List<MonthInfo> GetDisplayMonths(DateTime currentDate)
    {
        var months = new List<MonthInfo>();
        var currentYear = currentDate.Year;
        var currentMonth = currentDate.Month;

        var startMonth = currentMonth - 1;
        var startYear = currentYear;
        if (startMonth < 1)
        {
            startMonth = 12;
            startYear--;
        }

        for (int i = 0; i < 4; i++)
        {
            var month = startMonth + i;
            var year = startYear;

            if (month > 12)
            {
                month -= 12;
                year++;
            }

            var startDate = new DateTime(year, month, 1);
            var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            months.Add(new MonthInfo
            {
                Name = GetMonthName(month),
                Year = year,
                StartDate = startDate,
                EndDate = endDate,
                GridColumnIndex = i,
                IsCurrentMonth = year == currentYear && month == currentMonth
            });
        }

        _logger.LogDebug("Display months calculated for {CurrentDate}: {Months}",
            currentDate, string.Join(", ", months.Select(m => $"{m.Name} {m.Year}")));

        return months;
    }

    public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
    {
        var baselineYear = baselineDate.Year;
        var milestoneYear = milestoneDate.Year;

        if (milestoneYear != baselineYear)
        {
            throw new ArgumentOutOfRangeException(nameof(milestoneDate),
                $"Milestone date {milestoneDate:yyyy-MM-dd} is outside timeline range " +
                $"(year {baselineYear})");
        }

        var baselineYearStart = new DateTime(baselineYear, 1, 1);
        var daysFromStart = (milestoneDate.Date - baselineYearStart).TotalDays;
        var monthsFromStart = daysFromStart / 30.4375;

        var xPosition = (int)(monthsFromStart * PixelsPerMonth);

        if (xPosition < 0 || xPosition > SvgWidth)
        {
            throw new ArgumentOutOfRangeException(nameof(milestoneDate),
                $"Milestone date {milestoneDate:yyyy-MM-dd} calculates to x-position {xPosition}, " +
                $"which is outside SVG width (0-{SvgWidth})");
        }

        return xPosition;
    }

    public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
    {
        return GetMilestoneXPosition(currentDate, baselineDate);
    }

    public bool IsCurrentMonth(YearMonth month, DateTime currentDate)
    {
        var current = new YearMonth(currentDate.Year, currentDate.Month);
        return month == current;
    }

    public (int startX, int endX) GetMonthBounds(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex >= MonthsInTimeline)
        {
            throw new ArgumentOutOfRangeException(nameof(monthIndex),
                $"Month index must be between 0 and {MonthsInTimeline - 1}");
        }

        var startX = monthIndex * PixelsPerMonth;
        var endX = startX + PixelsPerMonth;

        return (startX, endX);
    }

    private string GetMonthName(int month) => month switch
    {
        1 => "January",
        2 => "February",
        3 => "March",
        4 => "April",
        5 => "May",
        6 => "June",
        7 => "July",
        8 => "August",
        9 => "September",
        10 => "October",
        11 => "November",
        12 => "December",
        _ => "Unknown"
    };
}