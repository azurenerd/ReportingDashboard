using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void DashboardData_DeserializesFromJson()
    {
        var json = """
        {
            "title": "Test Project",
            "subtitle": "Org - Team",
            "backlogUrl": "https://ado.example.com",
            "reportDate": "2026-04-15",
            "timelineRange": {
                "start": "2026-01-01",
                "end": "2026-06-30",
                "monthGridlines": [
                    { "label": "Jan", "date": "2026-01-01" }
                ]
            },
            "workstreams": [
                {
                    "id": "M1",
                    "label": "M1 - Feature",
                    "description": "Feature work",
                    "color": "#4285F4",
                    "milestones": [
                        { "date": "2026-03-01", "label": "PoC", "type": "poc", "labelPosition": "above" }
                    ]
                }
            ],
            "heatmap": {
                "title": "Heatmap",
                "months": ["January", "February"],
                "currentMonth": "February",
                "rows": [
                    {
                        "category": "shipped",
                        "displayLabel": "Shipped",
                        "cells": [
                            { "month": "January", "items": ["Item A", "Item B"] }
                        ]
                    }
                ]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<DashboardData>(json);

        data.Should().NotBeNull();
        data!.Title.Should().Be("Test Project");
        data.ReportDate.Should().Be("2026-04-15");
        data.TimelineRange.Start.Should().Be("2026-01-01");
        data.Workstreams.Should().HaveCount(1);
        data.Workstreams[0].Milestones[0].LabelPosition.Should().Be("above");
        data.Heatmap.Rows[0].Cells[0].Items.Should().ContainInOrder("Item A", "Item B");
    }

    [Fact]
    public void Milestone_LabelPosition_DefaultsToNull()
    {
        var json = """{ "date": "2026-01-15", "label": "Check", "type": "checkpoint" }""";

        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        milestone.Should().NotBeNull();
        milestone!.LabelPosition.Should().BeNull();
        milestone.Type.Should().Be("checkpoint");
    }
}