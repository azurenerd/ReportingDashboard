using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed record MonthGridline(double X, string Label);

public sealed record LaneGeometry(
    string Id, string Label, string Color, double Y,
    IReadOnlyList<MilestoneGeometry> Milestones);

public sealed record MilestoneGeometry(
    double X, double Y, MilestoneType Type, string Caption,
    CaptionPosition CaptionPosition);

public sealed record NowMarker(double X, bool InRange);

public sealed record TimelineViewModel(
    IReadOnlyList<MonthGridline> Gridlines,
    IReadOnlyList<LaneGeometry> Lanes,
    NowMarker Now)
{
    public static TimelineViewModel Empty { get; } = new(
        Array.Empty<MonthGridline>(),
        Array.Empty<LaneGeometry>(),
        new NowMarker(0, false));
}

public static class TimelineLayoutEngine
{
    public const int SvgWidth = 1560;
    public const int SvgHeight = 185;
    public const int TopPad = 20;
    public const int BottomPad = 20;

    public static TimelineViewModel Build(
        Timeline timeline, DateOnly today,
        int svgWidth = SvgWidth, int svgHeight = SvgHeight)
    {
        var totalDays = timeline.End.DayNumber - timeline.Start.DayNumber;
        if (totalDays <= 0)
            return TimelineViewModel.Empty;

        double XOf(DateOnly date) =>
            (double)(date.DayNumber - timeline.Start.DayNumber) / totalDays * svgWidth;

        // Month gridlines
        var gridlines = new List<MonthGridline>();
        var current = new DateOnly(timeline.Start.Year, timeline.Start.Month, 1);
        while (current <= timeline.End)
        {
            gridlines.Add(new MonthGridline(XOf(current), current.ToString("MMM")));
            current = current.AddMonths(1);
        }

        // Lanes
        var laneCount = timeline.Lanes.Count;
        var availableHeight = svgHeight - TopPad - BottomPad;
        var lanes = new List<LaneGeometry>();

        for (int i = 0; i < laneCount; i++)
        {
            var lane = timeline.Lanes[i];
            var y = TopPad + (i + 0.5) * availableHeight / laneCount;

            var milestones = new List<MilestoneGeometry>();
            var prevX = -100.0;
            var alternateBelow = false;

            foreach (var m in lane.Milestones.OrderBy(m => m.Date))
            {
                var x = XOf(m.Date);

                CaptionPosition captionPos;
                if (m.CaptionPosition.HasValue)
                {
                    captionPos = m.CaptionPosition.Value;
                }
                else if (Math.Abs(x - prevX) < 50)
                {
                    captionPos = alternateBelow ? CaptionPosition.Below : CaptionPosition.Above;
                    alternateBelow = !alternateBelow;
                }
                else
                {
                    captionPos = CaptionPosition.Above;
                    alternateBelow = true;
                }

                milestones.Add(new MilestoneGeometry(x, y, m.Type, m.Label, captionPos));
                prevX = x;
            }

            lanes.Add(new LaneGeometry(lane.Id, lane.Label, lane.Color, y, milestones));
        }

        // NOW marker
        var nowX = XOf(today);
        var inRange = today >= timeline.Start && today <= timeline.End;
        var now = new NowMarker(nowX, inRange);

        return new TimelineViewModel(gridlines, lanes, now);
    }
}