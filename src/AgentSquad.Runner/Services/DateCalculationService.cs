using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DateCalculationService : IDateCalculationService
{
    private const int PixelsPerMonth = 260;
    private const int SvgWidth = 1560;
    private readonly ILogger<DateCalculationService> _logger;

    public DateCalculationService(ILogger<DateCalculationService> logger)
    {
        _logger = logger;
    }

    public List<MonthInfo> GetDisplayMonths(DateTime currentDate)
    {
        var result = new List<MonthInfo>();
        var today = currentDate.Date;
        var currentMonth = new DateTime(today.Year, today.Month, 1);
        
        // Display: previous month, current month, and next 2 months
        for (int i = -1; i < 3; i++)
        {
            var monthStart = currentMonth.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var isCurrentMonth = monthStart.Year == today.Year && monthStart.Month == today.Month;

            result.Add(new MonthInfo
            {
                Name = GetMonthName(monthStart.Month),
                Year = monthStart.Year,
                StartDate = monthStart,
                EndDate = monthEnd,
                GridColumnIndex = i + 1,
                IsCurrentMonth = isCurrentMonth
            });
        }

        return result;
    }

    public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
    {
        try
        {
            var daysDifference = (milestoneDate.Date - baselineDate.Date).TotalDays;
            
            // 6 months = ~181 days = 1560 pixels
            double monthProgress = daysDifference / 181.0;
            int xPosition = (int)(monthProgress * SvgWidth);

            return Math.Clamp(xPosition, 0, SvgWidth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating milestone x position for {Date}", milestoneDate);
            return 0;
        }
    }

    public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
    {
        return GetMilestoneXPosition(currentDate, baselineDate);
    }

    public bool IsCurrentMonth(string month, int year, DateTime currentDate)
    {
        return GetMonthName(currentDate.Month).Equals(month, StringComparison.OrdinalIgnoreCase) && 
               currentDate.Year == year;
    }

    public (int startX, int endX) GetMonthBounds(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex > 5)
            throw new ArgumentOutOfRangeException(nameof(monthIndex), "Month index must be 0-5 (Jan-Jun)");

        int startX = monthIndex * PixelsPerMonth;
        int endX = startX + PixelsPerMonth;

        return (startX, endX);
    }

    private string GetMonthName(int monthNumber)
    {
        return monthNumber switch
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
}