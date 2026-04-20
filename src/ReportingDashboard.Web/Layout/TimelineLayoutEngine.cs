using System;
using System.Collections.Generic;
using System.Globalization;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Layout;

public static class TimelineLayoutEngine
{
    public const int SvgWidth = 1560;
    public const int SvgHeight = 185;
    public const int TopPad = 20;
    public const int BottomPad = 20;
    private const double CaptionProximityPx = 50.0;

    public static TimelineViewModel Build(
        Timeline timeline,
        DateOnly today,
        int svgWidth = SvgWidth,
        int svgHeight = SvgHeight)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        if (timeline.End <= timeline.Start || timeline.Lanes is null || timeline.Lanes.Count == 0)
        {
            return TimelineViewModel.Empty with { SvgWidth = svgWidth, SvgHeight = svgHeight };
        }

        double totalDays = (timeline.End.ToDateTime(TimeOnly.MinValue) - timeline.Start.ToDateTime(TimeOnly.MinValue)).TotalDays;
        if (totalDays <= 0)
        {
            return TimelineViewModel.Empty with { SvgWidth = svgWidth, SvgHeight = svgHeight };
        }

        double XOf(DateOnly date)
        {
            double days = (date.ToDateTime(TimeOnly.MinValue) - timeline.Start.ToDateTime(TimeOnly.MinValue)).TotalDays;
            return days / totalDays * svgWidth;
        }

        var gridlines = new List<MonthGridline>();
        var cursor = new DateOnly(timeline.Start.Year, timeline.Start.Month, 1);
        if (cursor < timeline.Start)
        {
            cursor = cursor.AddMonths(1);
        }
        while (cursor <= timeline.End)
        {
            double x = XOf(cursor);
            string label = cursor.ToDateTime(TimeOnly.MinValue).ToString("MMM", CultureInfo.InvariantCulture);
            gridlines.Add(new MonthGridline(x, label));
            cursor = cursor.AddMonths(1);
        }

        int laneCount = timeline.Lanes.Count;
        double usableHeight = svgHeight - TopPad - BottomPad;
        var lanes = new List<LaneGeometry>(laneCount);

        for (int i = 0; i < laneCount; i++)
        {
            var lane = timeline.Lanes[i];
            double y = TopPad + (i + 0.5) * usableHeight / laneCount;

            var milestoneList = new List<MilestoneGeometry>();
            if (lane.Milestones is not null)
            {
                var sorted = new List<Milestone>(lane.Milestones);
                sorted.Sort((a, b) => a.Date.CompareTo(b.Date));

                double? prevX = null;
                CaptionPlacement prevPos = CaptionPlacement.Above;

                foreach (var m in sorted)
                {
                    if (m.Date < timeline.Start || m.Date > timeline.End)
                    {
                        continue;
                    }

                    double x = XOf(m.Date);
                    CaptionPlacement pos;
                    if (m.CaptionPosition is CaptionPosition cp)
                    {
                        pos = cp == CaptionPosition.Below ? CaptionPlacement.Below : CaptionPlacement.Above;
                    }
                    else if (prevX.HasValue && Math.Abs(x - prevX.Value) < CaptionProximityPx)
                    {
                        pos = prevPos == CaptionPlacement.Above ? CaptionPlacement.Below : CaptionPlacement.Above;
                    }
                    else
                    {
                        pos = CaptionPlacement.Above;
                    }

                    milestoneList.Add(new MilestoneGeometry(x, y, MapType(m.Type), m.Label ?? string.Empty, pos));
                    prevX = x;
                    prevPos = pos;
                }
            }

            lanes.Add(new LaneGeometry(lane.Id, lane.Label, lane.Color, y, milestoneList));
        }

        bool nowInRange = today >= timeline.Start && today <= timeline.End;
        double nowX = nowInRange ? XOf(today) : 0;
        var now = new NowMarker(nowX, nowInRange);

        return new TimelineViewModel(gridlines, lanes, now, svgWidth, svgHeight);
    }

    private static MilestoneGeometryType MapType(MilestoneType t) => t switch
    {
        MilestoneType.Poc => MilestoneGeometryType.Poc,
        MilestoneType.Prod => MilestoneGeometryType.Prod,
        _ => MilestoneGeometryType.Checkpoint
    };
}