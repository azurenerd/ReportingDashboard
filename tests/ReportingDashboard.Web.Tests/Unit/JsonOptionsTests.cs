using System.Text.Json;
using System.Text.Json.Serialization;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Unit;

public class JsonOptionsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Default_HasCaseInsensitiveAndCamelCaseAndCommentAndTrailingCommaSettings()
    {
        var o = JsonOptions.Default;
        o.PropertyNameCaseInsensitive.Should().BeTrue();
        o.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
        o.ReadCommentHandling.Should().Be(JsonCommentHandling.Skip);
        o.AllowTrailingCommas.Should().BeTrue();
        o.Converters.Should().ContainSingle(c => c is JsonStringEnumConverter);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Default_IsSingletonInstance()
    {
        ReferenceEquals(JsonOptions.Default, JsonOptions.Default).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deserialize_MilestoneWithCamelCaseEnumAndIsoDate_Succeeds()
    {
        const string json = """
        { "date": "2026-03-26", "type": "poc", "label": "Mar 26 PoC" }
        """;

        var m = JsonSerializer.Deserialize<Milestone>(json, JsonOptions.Default);

        m.Should().NotBeNull();
        m!.Date.Should().Be(new DateOnly(2026, 3, 26));
        m.Type.Should().Be(MilestoneType.Poc);
        m.Label.Should().Be("Mar 26 PoC");
        m.CaptionPosition.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deserialize_HeatmapWithoutMaxItemsPerCell_DefaultsTo4()
    {
        const string json = """
        {
          "months": ["Jan","Feb","Mar","Apr"],
          "currentMonthIndex": null,
          "rows": [
            { "category": "shipped", "cells": [[],[],[],[]] },
            { "category": "inProgress", "cells": [[],[],[],[]] },
            { "category": "carryover", "cells": [[],[],[],[]] },
            { "category": "blockers", "cells": [[],[],[],[]] }
          ]
        }
        """;

        var h = JsonSerializer.Deserialize<Heatmap>(json, JsonOptions.Default);

        h.Should().NotBeNull();
        h!.MaxItemsPerCell.Should().Be(4);
        h.CurrentMonthIndex.Should().BeNull();
        h.Months.Should().HaveCount(4);
        h.Rows.Should().HaveCount(4);
        h.Rows[0].Category.Should().Be(HeatmapCategory.Shipped);
        h.Rows[1].Category.Should().Be(HeatmapCategory.InProgress);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Deserialize_InvalidMilestoneTypeEnum_ThrowsJsonException()
    {
        const string json = """
        { "date": "2026-03-26", "type": "bogus", "label": "x" }
        """;

        var act = () => JsonSerializer.Deserialize<Milestone>(json, JsonOptions.Default);

        act.Should().Throw<JsonException>();
    }
}