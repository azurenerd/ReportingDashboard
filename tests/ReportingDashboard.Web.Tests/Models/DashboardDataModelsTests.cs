using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Models;

[Trait("Category", "Unit")]
public class DashboardDataModelsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    [Fact]
    public void Project_Placeholder_IsSingleton_WithErrorTitle_AndNoBacklogUrl()
    {
        var a = Project.Placeholder;
        var b = Project.Placeholder;

        ReferenceEquals(a, b).Should().BeTrue();
        a.Title.Should().Be("(data.json error)");
        a.Subtitle.Should().Be("see error banner above");
        a.BacklogUrl.Should().BeNull();
        a.BacklogLinkText.Should().Be("\u2192 ADO Backlog");
    }

    [Fact]
    public void Project_BacklogLinkText_DefaultsToArrowAdoBacklog_WhenOmitted()
    {
        const string json = """{ "title": "T", "subtitle": "S" }""";

        var p = JsonSerializer.Deserialize<Project>(json, JsonOptions);

        p.Should().NotBeNull();
        p!.Title.Should().Be("T");
        p.Subtitle.Should().Be("S");
        p.BacklogUrl.Should().BeNull();
        p.BacklogLinkText.Should().Be("\u2192 ADO Backlog");
    }

    [Fact]
    public void Heatmap_MaxItemsPerCell_DefaultsTo4_WhenOmitted_AndCurrentMonthIndexStaysNull()
    {
        const string json = """
        {
          "months": ["Jan","Feb","Mar","Apr"],
          "rows": []
        }
        """;

        var h = JsonSerializer.Deserialize<Heatmap>(json, JsonOptions);

        h.Should().NotBeNull();
        h!.MaxItemsPerCell.Should().Be(4);
        h.CurrentMonthIndex.Should().BeNull();
        h.Months.Should().HaveCount(4);
        h.Rows.Should().BeEmpty();
    }

    [Fact]
    public void DashboardData_FullSample_Deserializes_WithDatesLanesMilestonesAndHeatmap()
    {
        const string json = """
        {
          "project": {
            "title": "Privacy Automation Release Roadmap",
            "subtitle": "Org - Privacy Automation Workstream - Apr 2026",
            "backlogUrl": "https://example.com/backlog"
          },
          "timeline": {
            "start": "2026-01-01",
            "end": "2026-06-30",
            "lanes": [
              {
                "id": "M1",
                "label": "Core",
                "color": "#0078D4",
                "milestones": [
                  { "date": "2026-01-12", "type": "checkpoint", "label": "Kickoff" },
                  { "date": "2026-03-26", "type": "poc",        "label": "PoC" },
                  { "date": "2026-05-20", "type": "prod",       "label": "GA", "captionPosition": "below" }
                ]
              },
              { "id": "M2", "label": "Ingest", "color": "#00897B", "milestones": [] },
              { "id": "M3", "label": "Ops",    "color": "#546E7A", "milestones": [] },
            ]
          },
          "heatmap": {
            "months": ["Jan","Feb","Mar","Apr"],
            "currentMonthIndex": 3,
            "rows": [
              { "category": "shipped",    "cells": [[],[],[],["Auth"]] },
              { "category": "inProgress", "cells": [[],[],[],["Ingest"]] },
              { "category": "carryover",  "cells": [[],[],[],[]] },
              { "category": "blockers",   "cells": [[],[],[],["Vendor SLA"]] }
            ]
          }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("Privacy Automation Release Roadmap");
        data.Project.Subtitle.Should().Contain("Privacy Automation Workstream");
        data.Project.BacklogUrl.Should().Be("https://example.com/backlog");

        data.Timeline.Start.Should().Be(new DateOnly(2026, 1, 1));
        data.Timeline.End.Should().Be(new DateOnly(2026, 6, 30));
        data.Timeline.Lanes.Should().HaveCount(3);

        var lane0 = data.Timeline.Lanes[0];
        lane0.Id.Should().Be("M1");
        lane0.Color.Should().Be("#0078D4");
        lane0.Milestones.Should().HaveCount(3);
        lane0.Milestones[0].Date.Should().Be(new DateOnly(2026, 1, 12));
        lane0.Milestones.Select(m => m.Type).Should().BeEquivalentTo(new[]
        {
            MilestoneType.Checkpoint, MilestoneType.Poc, MilestoneType.Prod
        });
        lane0.Milestones[2].CaptionPosition.Should().Be(CaptionPosition.Below);

        data.Heatmap.Months.Should().HaveCount(4);
        data.Heatmap.CurrentMonthIndex.Should().Be(3);
        data.Heatmap.MaxItemsPerCell.Should().Be(4);
        data.Heatmap.Rows.Should().HaveCount(4);
        data.Heatmap.Rows.Select(r => r.Category).Should().BeEquivalentTo(new[]
        {
            HeatmapCategory.Shipped, HeatmapCategory.InProgress,
            HeatmapCategory.Carryover, HeatmapCategory.Blockers
        });
        data.Heatmap.Rows.Single(r => r.Category == HeatmapCategory.Blockers)
            .Cells[3].Single().Should().Be("Vendor SLA");
    }

    [Fact]
    public void Enums_DeserializeCaseInsensitively_AndInProgressCamelCaseBinds()
    {
        JsonSerializer.Deserialize<MilestoneType>("\"poc\"", JsonOptions).Should().Be(MilestoneType.Poc);
        JsonSerializer.Deserialize<MilestoneType>("\"PoC\"", JsonOptions).Should().Be(MilestoneType.Poc);
        JsonSerializer.Deserialize<MilestoneType>("\"PROD\"", JsonOptions).Should().Be(MilestoneType.Prod);
        JsonSerializer.Deserialize<MilestoneType>("\"checkpoint\"", JsonOptions).Should().Be(MilestoneType.Checkpoint);

        JsonSerializer.Deserialize<HeatmapCategory>("\"inProgress\"", JsonOptions).Should().Be(HeatmapCategory.InProgress);
        JsonSerializer.Deserialize<HeatmapCategory>("\"shipped\"", JsonOptions).Should().Be(HeatmapCategory.Shipped);
        JsonSerializer.Deserialize<HeatmapCategory>("\"BLOCKERS\"", JsonOptions).Should().Be(HeatmapCategory.Blockers);

        JsonSerializer.Deserialize<CaptionPosition>("\"above\"", JsonOptions).Should().Be(CaptionPosition.Above);
        JsonSerializer.Deserialize<CaptionPosition>("\"BELOW\"", JsonOptions).Should().Be(CaptionPosition.Below);
    }
}