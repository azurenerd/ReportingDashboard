using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Models;

public class DashboardDataDeserializationTests
{
    private const string SampleJson = """
    {
      "project": {
        "title": "Privacy Automation Release Roadmap",
        "subtitle": "Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026",
        "backlogUrl": "https://dev.azure.com/contoso/privacy/_backlogs/backlog/"
      },
      "timeline": {
        "start": "2026-01-01",
        "end":   "2026-06-30",
        "lanes": [
          { "id":"M1", "label":"Chatbot & MS Role", "color":"#0078D4",
            "milestones":[
              {"date":"2026-01-12","type":"checkpoint","label":"Jan 12"},
              {"date":"2026-03-26","type":"poc","label":"Mar 26 PoC"},
              {"date":"2026-04-30","type":"prod","label":"Apr Prod (TBD)"}
            ]},
          { "id":"M2", "label":"PDS & Data Inventory", "color":"#00897B", "milestones":[] }
        ]
      },
      "heatmap": {
        "months": ["Jan","Feb","Mar","Apr"],
        "currentMonthIndex": null,
        "rows": [
          {"category":"shipped",    "cells":[["Item A"],["Item B"],[],["Item C"]]},
          {"category":"inProgress", "cells":[[],[],["X"],["Y","Z"]]},
          {"category":"carryover",  "cells":[[],[],[],["Legacy API"]]},
          {"category":"blockers",   "cells":[[],[],[],["Vendor SLA"]]}
        ]
      }
    }
    """;

    private static JsonSerializerOptions Options => new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    [Fact]
    public void Deserialize_ValidJson_PopulatesAllSections()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(SampleJson, Options);

        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("Privacy Automation Release Roadmap");
        data.Project.Subtitle.Should().Contain("April 2026");
        data.Project.BacklogUrl.Should().Be("https://dev.azure.com/contoso/privacy/_backlogs/backlog/");
    }

    [Fact]
    public void Deserialize_Timeline_ParsesDateOnlyAndLanes()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(SampleJson, Options)!;

        data.Timeline.Start.Should().Be(new DateOnly(2026, 1, 1));
        data.Timeline.End.Should().Be(new DateOnly(2026, 6, 30));
        data.Timeline.Lanes.Should().HaveCount(2);

        var m1 = data.Timeline.Lanes[0];
        m1.Id.Should().Be("M1");
        m1.Color.Should().Be("#0078D4");
        m1.Milestones.Should().HaveCount(3);
        m1.Milestones[0].Type.Should().Be(MilestoneType.Checkpoint);
        m1.Milestones[0].Date.Should().Be(new DateOnly(2026, 1, 12));
        m1.Milestones[1].Type.Should().Be(MilestoneType.Poc);
        m1.Milestones[2].Type.Should().Be(MilestoneType.Prod);
        m1.Milestones[2].Label.Should().Be("Apr Prod (TBD)");
    }

    [Fact]
    public void Deserialize_Heatmap_ParsesCategoriesAndCells()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(SampleJson, Options)!;

        data.Heatmap.Months.Should().Equal("Jan", "Feb", "Mar", "Apr");
        data.Heatmap.CurrentMonthIndex.Should().BeNull();
        data.Heatmap.MaxItemsPerCell.Should().Be(4); // default applied
        data.Heatmap.Rows.Should().HaveCount(4);

        data.Heatmap.Rows[0].Category.Should().Be(HeatmapCategory.Shipped);
        data.Heatmap.Rows[1].Category.Should().Be(HeatmapCategory.InProgress);
        data.Heatmap.Rows[2].Category.Should().Be(HeatmapCategory.Carryover);
        data.Heatmap.Rows[3].Category.Should().Be(HeatmapCategory.Blockers);

        data.Heatmap.Rows[0].Cells.Should().HaveCount(4);
        data.Heatmap.Rows[0].Cells[0].Should().ContainSingle().Which.Should().Be("Item A");
        data.Heatmap.Rows[0].Cells[2].Should().BeEmpty();
        data.Heatmap.Rows[1].Cells[3].Should().Equal("Y", "Z");
    }

    [Fact]
    public void RoundTrip_SerializeThenDeserialize_PreservesKeyFields()
    {
        var original = JsonSerializer.Deserialize<DashboardData>(SampleJson, Options)!;
        var json = JsonSerializer.Serialize(original, Options);
        var roundTripped = JsonSerializer.Deserialize<DashboardData>(json, Options)!;

        roundTripped.Project.Title.Should().Be(original.Project.Title);
        roundTripped.Project.BacklogUrl.Should().Be(original.Project.BacklogUrl);
        roundTripped.Timeline.Start.Should().Be(original.Timeline.Start);
        roundTripped.Timeline.End.Should().Be(original.Timeline.End);
        roundTripped.Timeline.Lanes[0].Milestones[1].Type.Should().Be(MilestoneType.Poc);
        roundTripped.Heatmap.Rows[3].Category.Should().Be(HeatmapCategory.Blockers);
        roundTripped.Heatmap.MaxItemsPerCell.Should().Be(4);
    }

    [Fact]
    public void Project_Placeholder_HasExpectedDefaults()
    {
        Project.Placeholder.Title.Should().Be("(data.json error)");
        Project.Placeholder.Subtitle.Should().Be("see error banner above");
        Project.Placeholder.BacklogLinkText.Should().Be("\u2192 ADO Backlog");
    }

    [Fact]
    public void Heatmap_MaxItemsPerCell_RespectsExplicitValue()
    {
        const string json = """
        {
          "project": { "title":"t", "subtitle":"s" },
          "timeline": { "start":"2026-01-01", "end":"2026-06-30", "lanes": [] },
          "heatmap": {
            "months": ["Jan","Feb","Mar","Apr"],
            "maxItemsPerCell": 6,
            "rows": [
              {"category":"shipped","cells":[[],[],[],[]]},
              {"category":"inProgress","cells":[[],[],[],[]]},
              {"category":"carryover","cells":[[],[],[],[]]},
              {"category":"blockers","cells":[[],[],[],[]]}
            ]
          }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options)!;
        data.Heatmap.MaxItemsPerCell.Should().Be(6);
    }
}