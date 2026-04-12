using NodaTime;

namespace AgentSquad.Runner.Services;

public class DateCalculationService
{
    private const int PixelsPerMonth = 260;
    private const int SvgWidth = 1560;
    private readonly ILogger<DateCalculationService> _logger;

    public DateCalculationService(ILogger<DateCalculationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates 4-month display window starting from the month containing the current date.
    /// Returns months in chronological order.
    /// </summary>
    public List<MonthInfo> GetDisplayMonths(DateTime currentDate)
    {
        var currentYearMonth = YearMonth.FromDateTime(currentDate);
        var months = new List<MonthInfo>();

        for (int i = 0; i < 4; i++)
        {
            var displayYearMonth = currentYearMonth.PlusMonths(i);
            var startOfMonth = displayYearMonth.AtDay(1).ToDateTimeUnspecified();
            var endOfMonth = displayYearMonth.AtEndOfMonth().ToDateTimeUnspecified();

            months.Add(new MonthInfo
            {
                Name = GetMonthName(displayYearMonth.Month),
                Year = displayYearMonth.Year,
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                GridColumnIndex = i,
                IsCurrentMonth = displayYearMonth == currentYearMonth
            });
        }

        return months;
    }

    /// <summary>
    /// Converts a milestone date to SVG x-coordinate based on Jan 1 baseline.
    /// Assumes timeline spans Jan 1 - Jun 30 (6 months, 1560px total).
    /// Returns pixel position from left edge (0-1560).
    /// </summary>
    public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
    {
        var baseline = LocalDate.FromDateTime(baselineDate);
        var milestone = LocalDate.FromDateTime(milestoneDate);

        // Get January 1 of the baseline year as reference
        var january1 = new LocalDate(baseline.Year, 1, 1);

        // Calculate days elapsed from January 1
        var daysFromJanuary1 = Period.Between(january1, milestone, PeriodUnits.Days).Days;

        // Convert days to pixels (260 pixels per 30-day month approximately)
        // More precisely: 1560px / 184 days (Jan 1 - Jun 30) = 8.478 px/day
        var pixelsPerDay = (double)SvgWidth / 184.0; // 184 days from Jan 1 to Jun 30 inclusive
        var xPosition = (int)(daysFromJanuary1 * pixelsPerDay);

        if (xPosition < 0 || xPosition > SvgWidth)
        {
            _logger.LogWarning(
                "Milestone date {MilestoneDate:yyyy-MM-dd} outside timeline range (Jan 1 - Jun 30). Position: {Position}px",
                milestoneDate, xPosition);
        }

        return Math.Max(0, Math.Min(xPosition, SvgWidth));
    }

    /// <summary>
    /// Calculates x-coordinate of red "Now" dashed line at current date.
    /// Uses same baseline (Jan 1) as milestone positioning.
    /// </summary>
    public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
    {
        return GetMilestoneXPosition(currentDate, baselineDate);
    }

    /// <summary>
    /// Determines if a given year/month matches the current month.
    /// Used for highlighting current month column in heatmap.
    /// </summary>
    public bool IsCurrentMonth(string monthName, int year, DateTime currentDate)
    {
        var currentYearMonth = YearMonth.FromDateTime(currentDate);
        var month = ParseMonthName(monthName);

        return currentYearMonth.Year == year && currentYearMonth.Month == month;
    }

    /// <summary>
    /// Returns the start and end x-positions for a given month (0-5 for Jan-Jun).
    /// Used for drawing month gridlines in timeline SVG.
    /// </summary>
    public (int StartX, int EndX) GetMonthBounds(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(monthIndex), "Month index must be 0-5 (Jan-Jun)");
        }

        int startX = monthIndex * PixelsPerMonth;
        int endX = startX + PixelsPerMonth;

        return (startX, endX);
    }

    /// <summary>
    /// Gets the x-position for the start of a given month (0-5 for Jan-Jun).
    /// Used for drawing month gridlines.
    /// </summary>
    public int GetMonthGridLinePosition(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(monthIndex), "Month index must be 0-5 (Jan-Jun)");
        }

        return monthIndex * PixelsPerMonth;
    }

    /// <summary>
    /// Converts month name (e.g., "March") to ISO month number (3).
    /// </summary>
    private int ParseMonthName(string monthName)
    {
        return monthName.ToLowerInvariant() switch
        {
            "january" or "jan" => 1,
            "february" or "feb" => 2,
            "march" or "mar" => 3,
            "april" or "apr" => 4,
            "may" => 5,
            "june" or "jun" => 6,
            "july" or "jul" => 7,
            "august" or "aug" => 8,
            "september" or "sep" => 9,
            "october" or "oct" => 10,
            "november" or "nov" => 11,
            "december" or "dec" => 12,
            _ => throw new ArgumentException($"Invalid month name: {monthName}")
        };
    }

    /// <summary>
    /// Converts ISO month number to full month name.
    /// </summary>
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
            _ => throw new ArgumentException($"Invalid month number: {monthNumber}")
        };
    }
}

public class MonthInfo
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GridColumnIndex { get; set; }
    public bool IsCurrentMonth { get; set; }
}