namespace AgentSquad.Runner.Utils;

public static class DateFormatter
{
    public static string ToMilestoneFormat(DateTime due)
    {
        int month = due.Month;
        int year = due.Year;

        int fiscalYear = month >= 10 ? year + 1 : year;

        int quarter = month switch
        {
            10 or 11 or 12 => 1,
            1 or 2 or 3 => 2,
            4 or 5 or 6 => 3,
            7 or 8 or 9 => 4,
            _ => 1
        };

        string dateStr = due.ToString("MMM d");
        return $"Q{quarter} FY{fiscalYear} ({dateStr})";
    }
}