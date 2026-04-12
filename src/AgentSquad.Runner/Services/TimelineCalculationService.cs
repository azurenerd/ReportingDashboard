namespace AgentSquad.Runner.Services;

public class TimelineCalculationService
{
    public const int MonthWidthPx = 260;
    public const int SvgWidthPx = 1560;
    public const int SvgHeightPx = 185;
    
    private static readonly DateTime TimelineStartDate = new(2026, 1, 1);
    private static readonly DateTime TimelineEndDate = new(2026, 6, 30);
    private static readonly string[] MonthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };

    public int DateToPixel(DateTime date)
    {
        var totalDays = (TimelineEndDate - TimelineStartDate).TotalDays;
        var daysFromStart = (date - TimelineStartDate).TotalDays;
        
        if (daysFromStart < 0)
            daysFromStart = 0;
        if (daysFromStart > totalDays)
            daysFromStart = totalDays;
        
        var position = (int)(daysFromStart / totalDays * SvgWidthPx);
        return Math.Max(0, Math.Min(position, SvgWidthPx));
    }

    public int GetMonthPosition(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex > 5)
            return 0;
        return monthIndex * MonthWidthPx;
    }

    public int GetNowLinePosition()
    {
        var nowDate = new DateTime(2026, 4, 12);
        return DateToPixel(nowDate);
    }

    public int CalculateMilestoneYPosition(int milestoneIndex, int totalMilestones)
    {
        if (totalMilestones <= 0)
            return SvgHeightPx / 2;
        
        var spacing = SvgHeightPx / (totalMilestones + 1);
        return spacing * (milestoneIndex + 1);
    }

    public string GetMonthName(int monthIndex)
    {
        if (monthIndex < 0 || monthIndex >= MonthNames.Length)
            return string.Empty;
        return MonthNames[monthIndex];
    }
}