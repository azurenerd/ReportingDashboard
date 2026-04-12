using AgentSquad.Runner.Models;
using NodaTime;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Implementation of IDateCalculationService for date calculations and timeline positioning
/// </summary>
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
        
        var year = currentDate.Year;
        var month = currentDate.Month;
        
        // Start from previous month
        var startMonth = month - 1;
        var startYear = year;
        
        if (startMonth < 1)
        {
            startMonth = 12;
            startYear = year - 1;
        }
        
        for (int i = 0; i < 4; i++)
        {
            var displayMonth = startMonth + i;
            var displayYear = startYear;
            
            if (displayMonth > 12)
            {
                displayMonth = displayMonth - 12;
                displayYear = displayYear + 1;
            }
            
            var monthName = GetMonthName(displayMonth);
            var startDate = new DateTime(displayYear, displayMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var isCurrentMonth = displayMonth == month && displayYear == year;
            
            var monthInfo = new MonthInfo
            {
                Name = monthName,
                Year = displayYear,
                StartDate = startDate,
                EndDate = endDate,
                GridColumnIndex = i,
                IsCurrentMonth = isCurrentMonth
            };
            
            result.Add(monthInfo);
        }
        
        return result;
    }

    public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
    {
        var baselineYear = baselineDate.Year;
        var jan1 = new DateTime(baselineYear, 1, 1);
        
        var daysDiff = (milestoneDate - jan1).TotalDays;
        var dayOfYear = (int)daysDiff;
        
        // Assuming 365 days in year, calculate month position
        // Jan: 0-260, Feb: 260-520, Mar: 520-780, Apr: 780-1040, May: 1040-1300, Jun: 1300-1560
        var monthIndex = dayOfYear / 30; // Approximate
        var dayInMonth = dayOfYear % 30;
        
        var xPosition = (monthIndex * PixelsPerMonth) + (dayInMonth * (PixelsPerMonth / 30));
        
        return Math.Clamp(xPosition, 0, SvgWidth);
    }

    public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
    {
        return GetMilestoneXPosition(currentDate, baselineDate);
    }

    public bool IsCurrentMonth(string monthName, int year, DateTime currentDate)
    {
        var currentMonthName = GetMonthName(currentDate.Month);
        
        return monthName.Equals(currentMonthName, StringComparison.OrdinalIgnoreCase) 
            && year == currentDate.Year;
    }

    public (int startX, int endX) GetMonthBounds(int monthIndex)
    {
        var startX = monthIndex * PixelsPerMonth;
        var endX = startX + PixelsPerMonth;
        
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
            _ => string.Empty
        };
    }
}