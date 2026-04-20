using System.Text.Json;
using System.Text.Json.Serialization;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Unit;

internal static class TestDataFactory
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public const string ValidJson = """
    {
      "project": {
        "title": "My Project",
        "subtitle": "Org - Workstream - Apr 2026",
        "backlogUrl": "https://dev.azure.com/org/project"
      },
      "timeline": {
        "start": "2026-01-01",
        "end": "2026-12-31",
        "lanes": [
          {
            "id": "M1",
            "label": "Workstream 1",
            "color": "#0078D4",
            "milestones": [
              { "date": "2026-03-15", "label": "PoC", "type": "poc" },
              { "date": "2026-06-01", "label": "GA",  "type": "prod" }
            ]
          }
        ]
      },
      "heatmap": {
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "currentMonthIndex": 3,
        "maxItemsPerCell": 3,
        "rows": [
          { "category": "shipped",    "cells": [[], [], [], []] },
          { "category": "inProgress", "cells": [[], [], [], []] },
          { "category": "carryover",  "cells": [[], [], [], []] },
          { "category": "blockers",   "cells": [[], [], [], []] }
        ]
      }
    }
    """;

    public static DashboardData Deserialize(string json)
        => JsonSerializer.Deserialize<DashboardData>(json, JsonOptions)!;

    public static DashboardData ValidData() => Deserialize(ValidJson);
}