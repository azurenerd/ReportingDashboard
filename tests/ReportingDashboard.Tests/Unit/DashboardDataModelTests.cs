using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    [Fact]
    public void LabelPosition_DeserializesFromString()
    {
        var json = """{"date":"2026-03-01","type":"poc","label":"Test","labelPosition":"Below"}""";
        var milestone = JsonSerializer.Deserialize<TrackMilestone>(json, JsonOptions);

        milestone.Should().NotBeNull();
        milestone!.LabelPosition.Should().Be(LabelPosition.Below);
    }

    [Fact]
    public void LabelPosition_DefaultsToAbove()
    {
        var json = """{"date":"2026-03-01","type":"poc","label":"Test"}""";
        var milestone = JsonSerializer.Deserialize<TrackMilestone>(json, JsonOptions);

        milestone!.LabelPosition.Should().Be(LabelPosition.Above);
    }

    [Fact]
    public void HeatmapRow_DeserializesItemsDictionary()
    {
        var json = """{"category":"Shipped","items":{"Jan":["Item A","Item B"],"Feb":[]}}""";
        var row = JsonSerializer.Deserialize<HeatmapRow>(json, JsonOptions);

        row.Should().NotBeNull();
        row!.Category.Should().Be("Shipped");
        row.Items["Jan"].Should().HaveCount(2);
        row.Items["Feb"].Should().BeEmpty();
    }

    [Fact]
    public void ProjectInfo_BacklogLinkTextIsOptional()
    {
        var json = """{"title":"T","subtitle":"S","backlogUrl":"http://x.com"}""";
        var info = JsonSerializer.Deserialize<ProjectInfo>(json, JsonOptions);

        info!.BacklogLinkText.Should().BeNull();
    }

    [Fact]
    public void TimelineConfig_NowDateIsOptional()
    {
        var json = """{"startDate":"2026-01-01","endDate":"2026-06-30","tracks":[]}""";
        var config = JsonSerializer.Deserialize<TimelineConfig>(json, JsonOptions);

        config!.NowDate.Should().BeNull();
    }
}