using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

/// <summary>
/// JSON round-trip and binding tests for <see cref="DashboardData"/>. Verifies
/// that a canonical sample JSON blob deserializes, reserializes, and
/// re-deserializes with no data loss, and that enum + <see cref="DateOnly"/>
/// parsing behave as the architecture specifies.
/// </summary>
public class DashboardDataJsonTests
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
          { "id":"M2", "label":"PDS & Data Inventory", "color":"#00897B", "milestones":[] },
          { "id":"M3", "label":"Auto Review DFD",     "color":"#546E7A", "milestones":[] }
        ]
      },
      "heatmap": {
        "months": ["Jan","Feb","Mar","Apr"],
        "currentMonthIndex": null,
        "maxItemsPerCell": 4,
        "rows": [
          {"category":"shipped",    "cells":[["Item A"],["Item B"],[],["Item C"]]},
          {"category":"inProgress", "cells":[[],[],["X"],["Y","Z"]]},
          {"category":"carryover",  "cells":[[],[],[],["Legacy API"]]},
          {"category":"blockers",   "cells":[[],[],[],["Vendor SLA"]]}
        ]
      }
    }
    """;

    [Fact]
    public void Deserialize_BindsAllTopLevelSections()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(SampleJson, JsonOptions.Default);

        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("Privacy Automation Release Roadmap");
        data.Project.Subtitle.Should().Contain("Privacy Automation Workstream");
        data.Project.BacklogUrl.Should().Be("https://dev.azure.com/contoso/privacy/_backlogs/backlog/");
        data.Timeline.Lanes.Should().HaveCount(3);
        data.Heatmap.Rows.Should().HaveCount(4);
    }

    [Fact]
    public void RoundTrip_PreservesAllFieldsWithNoDataLoss()
    {
        var first = JsonSerializer.Deserialize<DashboardData>(SampleJson, JsonOptions.Default)!;
        var reserialized = JsonSerializer.Serialize(first, JsonOptions.Default);
        var second = JsonSerializer.Deserialize<DashboardData>(reserialized, JsonOptions.Default)!;

        second.Project.Title.Should().Be(first.Project.Title);
        second.Project.Subtitle.Should().Be(first.Project.Subtitle);
        second.Project.BacklogUrl.Should().Be(first.Project.BacklogUrl);

        second.Timeline.Start.Should().Be(first.Timeline.Start);
        second.Timeline.End.Should().Be(first.Timeline.End);
        second.Timeline.Lanes.Should().HaveCount(first.Timeline.Lanes.Count);
        for (var i = 0; i < first.Timeline.Lanes.Count; i++)
        {
            var a = first.Timeline.Lanes[i];
            var b = second.Timeline.Lanes[i];
            b.Id.Should().Be(a.Id);
            b.Label.Should().Be(a.Label);
            b.Color.Should().Be(a.Color);
            b.Milestones.Should().HaveCount(a.Milestones.Count);
            for (var j = 0; j < a.Milestones.Count; j++)
            {
                b.Milestones[j].Date.Should().Be(a.Milestones[j].Date);
                b.Milestones[j].Type.Should().Be(a.Milestones[j].Type);
                b.Milestones[j].Label.Should().Be(a.Milestones[j].Label);
            }
        }

        second.Heatmap.Months.Should().Equal(first.Heatmap.Months);
        second.Heatmap.MaxItemsPerCell.Should().Be(first.Heatmap.MaxItemsPerCell);
        second.Heatmap.CurrentMonthIndex.Should().Be(first.Heatmap.CurrentMonthIndex);
        second.Heatmap.Rows.Should().HaveCount(first.Heatmap.Rows.Count);
        for (var i = 0; i < first.Heatmap.Rows.Count; i++)
        {
            second.Heatmap.Rows[i].Category.Should().Be(first.Heatmap.Rows[i].Category);
            second.Heatmap.Rows[i].Cells.Should().HaveCount(first.Heatmap.Rows[i].Cells.Count);
            for (var c = 0; c < first.Heatmap.Rows[i].Cells.Count; c++)
            {
                second.Heatmap.Rows[i].Cells[c].Should()
                    .Equal(first.Heatmap.Rows[i].Cells[c]);
            }
        }
    }

    [Theory]
    [InlineData("poc", MilestoneType.Poc)]
    [InlineData("prod", MilestoneType.Prod)]
    [InlineData("checkpoint", MilestoneType.Checkpoint)]
    public void MilestoneType_ParsesFromCamelCaseJson(string jsonValue, MilestoneType expected)
    {
        var json = $"\"{jsonValue}\"";
        var actual = JsonSerializer.Deserialize<MilestoneType>(json, JsonOptions.Default);
        actual.Should().Be(expected);
    }

    [Fact]
    public void MilestoneType_IsCaseInsensitiveOnRead()
    {
        JsonSerializer.Deserialize<MilestoneType>("\"POC\"", JsonOptions.Default)
            .Should().Be(MilestoneType.Poc);
        JsonSerializer.Deserialize<MilestoneType>("\"Checkpoint\"", JsonOptions.Default)
            .Should().Be(MilestoneType.Checkpoint);
    }

    [Theory]
    [InlineData("shipped", HeatmapCategory.Shipped)]
    [InlineData("inProgress", HeatmapCategory.InProgress)]
    [InlineData("carryover", HeatmapCategory.Carryover)]
    [InlineData("blockers", HeatmapCategory.Blockers)]
    public void HeatmapCategory_ParsesFromCamelCaseJson(string jsonValue, HeatmapCategory expected)
    {
        var json = $"\"{jsonValue}\"";
        var actual = JsonSerializer.Deserialize<HeatmapCategory>(json, JsonOptions.Default);
        actual.Should().Be(expected);
    }

    [Fact]
    public void Enums_SerializeAsCamelCaseStrings()
    {
        JsonSerializer.Serialize(MilestoneType.Poc, JsonOptions.Default).Should().Be("\"poc\"");
        JsonSerializer.Serialize(MilestoneType.Checkpoint, JsonOptions.Default).Should().Be("\"checkpoint\"");
        JsonSerializer.Serialize(HeatmapCategory.InProgress, JsonOptions.Default).Should().Be("\"inProgress\"");
        JsonSerializer.Serialize(HeatmapCategory.Blockers, JsonOptions.Default).Should().Be("\"blockers\"");
    }

    [Fact]
    public void MaxItemsPerCell_DefaultsToFour_WhenOmitted()
    {
        const string json = """
        {
          "project":  { "title":"t", "subtitle":"s" },
          "timeline": { "start":"2026-01-01", "end":"2026-06-30", "lanes":[] },
          "heatmap":  {
            "months": ["Jan","Feb","Mar","Apr"],
            "rows": [
              {"category":"shipped",    "cells":[[],[],[],[]]},
              {"category":"inProgress", "cells":[[],[],[],[]]},
              {"category":"carryover",  "cells":[[],[],[],[]]},
              {"category":"blockers",   "cells":[[],[],[],[]]}
            ]
          }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions.Default)!;

        data.Heatmap.MaxItemsPerCell.Should().Be(4);
        data.Heatmap.CurrentMonthIndex.Should().BeNull();
    }

    [Fact]
    public void DateOnly_ParsesIsoYearMonthDayStrings()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(SampleJson, JsonOptions.Default)!;

        data.Timeline.Start.Should().Be(new DateOnly(2026, 1, 1));
        data.Timeline.End.Should().Be(new DateOnly(2026, 6, 30));

        var m1 = data.Timeline.Lanes[0].Milestones;
        m1[0].Date.Should().Be(new DateOnly(2026, 1, 12));
        m1[1].Date.Should().Be(new DateOnly(2026, 3, 26));
        m1[2].Date.Should().Be(new DateOnly(2026, 4, 30));
    }

    [Fact]
    public void DateOnly_SerializesAsIsoYearMonthDayString()
    {
        var data = JsonSerializer.Deserialize<DashboardData>(SampleJson, JsonOptions.Default)!;

        var json = JsonSerializer.Serialize(data, JsonOptions.Default);

        json.Should().Contain("\"start\":\"2026-01-01\"");
        json.Should().Contain("\"end\":\"2026-06-30\"");
        json.Should().Contain("\"date\":\"2026-03-26\"");
    }

    [Fact]
    public void Deserialize_AcceptsCommentsAndTrailingCommas()
    {
        const string json = """
        {
          // PM hand-edited comment
          "project":  { "title":"t", "subtitle":"s" },
          "timeline": { "start":"2026-01-01", "end":"2026-06-30", "lanes":[], },
          "heatmap":  {
            "months": ["Jan","Feb","Mar","Apr"],
            "rows": [
              {"category":"shipped",    "cells":[[],[],[],[]]},
              {"category":"inProgress", "cells":[[],[],[],[]]},
              {"category":"carryover",  "cells":[[],[],[],[]]},
              {"category":"blockers",   "cells":[[],[],[],[]]},
            ],
          },
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions.Default);

        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("t");
    }

    [Fact]
    public void Deserialize_IsPropertyCaseInsensitive()
    {
        const string json = """
        {
          "Project":  { "Title":"t", "Subtitle":"s", "BacklogUrl":null },
          "TIMELINE": { "Start":"2026-01-01", "END":"2026-06-30", "lanes":[] },
          "heatmap":  {
            "Months": ["Jan","Feb","Mar","Apr"],
            "MaxItemsPerCell": 7,
            "Rows": [
              {"Category":"shipped",    "Cells":[[],[],[],[]]},
              {"Category":"inProgress", "Cells":[[],[],[],[]]},
              {"Category":"carryover",  "Cells":[[],[],[],[]]},
              {"Category":"blockers",   "Cells":[[],[],[],[]]}
            ]
          }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions.Default)!;

        data.Project.Title.Should().Be("t");
        data.Timeline.Start.Should().Be(new DateOnly(2026, 1, 1));
        data.Heatmap.MaxItemsPerCell.Should().Be(7);
    }

    [Fact]
    public void ProjectPlaceholder_IsStableStaticInstance()
    {
        Project.Placeholder.Should().BeSameAs(Project.Placeholder);
        Project.Placeholder.Title.Should().Be("(data.json error)");
        Project.Placeholder.Subtitle.Should().Be("see error banner above");
    }
}
