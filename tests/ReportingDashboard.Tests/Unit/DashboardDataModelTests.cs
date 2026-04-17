using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void DashboardData_DeserializesFromProjectPhoenixJson()
    {
        var json = """
        {
            "schemaVersion": 1,
            "header": {
                "title": "Project Phoenix",
                "subtitle": "Engineering · Core Platform · April 2026",
                "backlogLink": "https://dev.azure.com/org/project",
                "currentDate": "2026-04-15"
            },
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "tracks": [
                    {
                        "id": "M1",
                        "label": "M1",
                        "description": "API Gateway",
                        "color": "#0078D4",
                        "milestones": [
                            { "label": "Kickoff", "date": "2026-01-15", "type": "checkpoint" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "columns": ["January", "February", "March", "April"],
                "highlightColumn": "April",
                "rows": [
                    {
                        "category": "Shipped",
                        "categoryStyle": "ship",
                        "cells": [["Item A"], ["Item B"], [], ["Item C"]]
                    }
                ]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);

        data.Should().NotBeNull();
        data!.SchemaVersion.Should().Be(1);
        data.Header.Title.Should().Be("Project Phoenix");
        data.Timeline.Tracks.Should().HaveCount(1);
        data.Timeline.Tracks[0].Milestones.Should().HaveCount(1);
        data.Heatmap.Columns.Should().HaveCount(4);
        data.Heatmap.HighlightColumn.Should().Be("April");
        data.Heatmap.Rows.Should().HaveCount(1);
        data.Heatmap.Rows[0].Category.Should().Be("Shipped");
    }

    [Fact]
    public void HeatmapData_HighlightColumn_IsNullable()
    {
        var json = """
        {
            "columns": ["Jan"],
            "rows": []
        }
        """;

        var data = JsonSerializer.Deserialize<HeatmapData>(json, Options);

        data.Should().NotBeNull();
        data!.HighlightColumn.Should().BeNull();
    }
}