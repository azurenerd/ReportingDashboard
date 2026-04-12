using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service for date calculations, month boundary calculations, and timeline positioning.
    /// Stub implementation: complete implementation deferred to subsequent PR.
    /// </summary>
    public class DateCalculationService : IDateCalculationService
    {
        private readonly ILogger<DateCalculationService> _logger;

        public DateCalculationService(ILogger<DateCalculationService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get 4-month display window (current month + 3 future months) based on current date.
        /// Stub: implementation deferred.
        /// </summary>
        public List<MonthInfo> GetDisplayMonths(DateTime currentDate)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Convert milestone date to SVG x-coordinate (pixels).
        /// Stub: implementation deferred.
        /// </summary>
        public int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Get x-coordinate for "Now" marker (red dashed line).
        /// Stub: implementation deferred.
        /// </summary>
        public int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Determine if a given month/year matches current month.
        /// Stub: implementation deferred.
        /// </summary>
        public bool IsCurrentMonth(string month, int year, DateTime currentDate)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Get the start and end x-positions for a month column in SVG timeline.
        /// Stub: implementation deferred.
        /// </summary>
        public (int startX, int endX) GetMonthBounds(int monthIndex)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }
    }
}