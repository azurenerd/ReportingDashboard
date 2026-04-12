namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Calculated month information used by timeline and heatmap rendering.
    /// Computed by DateCalculationService.GetDisplayMonths().
    /// </summary>
    public class MonthInfo
    {
        public string Name { get; set; } = string.Empty;
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int GridColumnIndex { get; set; }
        public bool IsCurrentMonth { get; set; }
    }
}