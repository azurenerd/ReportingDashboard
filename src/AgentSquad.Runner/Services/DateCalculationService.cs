#nullable enable

using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for date and timeline calculations.
/// Handles month boundary calculations, milestone positioning on the SVG timeline,
/// and determination of the current month for highlighting.
/// </summary>
public class DateCalculationService : IDateCalculationService
{
    private const int PixelsPerMonth = 260;
    private readonly ILogger<DateCalculationService> logger;

    public DateCalculationService(ILogger<DateCalculationService> logger)
    {
        this.logger = logger;
    }

    public List<MonthInfo> GetDisplayMonths(DateTime currentDate)
    {
        try
        {
            var months = new List<MonthInfo>();
            var today = currentDate.ToUniversalTime();
            var currentYear = today.Year;
            var currentMonth = today.Month;

            for (int i = -1; i < 3; i++)
            {
                var targetMonth = currentMonth + i;
                var targetYear = currentYear;

                if (targetMonth < 1)
                {
                    targetMonth += 12;
                    targetYear--;
                }
                else if (targetMonth > 12)
                {
                    targetMonth -= 12;
                    targetYear++;
                }

                var monthName = new DateTime(targetYear, targetMonth, 1).ToString("MMMM");
                var isCurrentMonth = targetYear == currentYear && targetMonth == currentMonth;

                var startDate = new DateTime(targetYear, targetMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                months.Add(new MonthInfo
                {
                    Name = monthName,
                    Year = targetYear,
                    StartDate = startDate,
                    EndDate = endDate,
                    GridColumnIndex = i + 1,
                    IsCurrentMonth = isCurrentMonth
                });
            }

            logger.LogDebug("Calculated display months: {Count} months", months.Count);
            return months;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating display months");
            throw new InvalidOperationException($"Failed to calculate display months for {currentDate:yyyy-MM-dd}", ex);
        }
    }

    public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
    {
        try
        {
            var baseline = baselineDate.ToUniversalTime();
            var milestone = milestoneDate.ToUniversalTime();

            var monthDiff = (milestone.Year - baseline.Year) * 12 + (milestone.Month - baseline.Month);
            monthDiff += milestone.Day >= 15 ? 1 : 0;

            int xPosition = monthDiff * PixelsPerMonth;

            if (xPosition < -100 || xPosition > 1560 + 100)
            {
                logger.LogWarning("Milestone date {Date} results in x-position {Position} outside typical timeline range",
                    milestoneDate.ToString("yyyy-MM-dd"), xPosition);
            }

            return xPosition;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating x-position for milestone date {Date}", milestoneDate.ToString("yyyy-MM-dd"));
            throw new InvalidOperationException($"Failed to calculate x-position for {milestoneDate:yyyy-MM-dd}", ex);
        }
    }

    public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
    {
        try
        {
            var baseline = baselineDate.ToUniversalTime();
            var today = currentDate.ToUniversalTime();

            var monthDiff = (today.Year - baseline.Year) * 12 + (today.Month - baseline.Month);
            int dayOfMonth = today.Day;
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            double monthFraction = (double)dayOfMonth / daysInMonth;
            double xPosition = (monthDiff * PixelsPerMonth) + (PixelsPerMonth * monthFraction);

            return (int)Math.Round(xPosition);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating x-position for 'Now' marker");
            throw new InvalidOperationException("Failed to calculate 'Now' marker position", ex);
        }
    }

    public bool IsCurrentMonth(string monthName, int year, DateTime currentDate)
    {
        try
        {
            var today = currentDate.ToUniversalTime();
            var currentMonthName = today.ToString("MMMM");

            bool isMatch = monthName.Equals(currentMonthName, StringComparison.OrdinalIgnoreCase) && year == today.Year;
            return isMatch;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error determining if month {Month} {Year} is current month", monthName, year);
            return false;
        }
    }
}