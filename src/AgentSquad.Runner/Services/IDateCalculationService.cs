using NodaTime;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IDateCalculationService
{
    List<MonthInfo> GetDisplayMonths(DateTime currentDate);

    int GetMilestoneXPosition(DateTime milestoneDate, DateTime baselineDate);

    int GetNowMarkerXPosition(DateTime currentDate, DateTime baselineDate);

    bool IsCurrentMonth(YearMonth month, DateTime currentDate);

    (int startX, int endX) GetMonthBounds(int monthIndex);
}