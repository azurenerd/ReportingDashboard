using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Focused tests on DashboardData header-specific fields (title, subtitle, backlogLink, currentMonth)
/// to ensure data binding for the Header component works correctly.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataHeaderFieldTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region Property Assignment

    [Fact]
    public void Title_CanBeSetAndRetrieved()
    {
        var data = new DashboardData { Title = "Project Alpha" };
        Assert.Equal("Project Alpha", data.Title);
    }

    [Fact]
    public void Subtitle_CanBeSetAndRetrieved()
    {
        var data = new DashboardData { Subtitle = "Team X - April 2026" };
        Assert.Equal("Team X - April 2026", data.Subtitle);
    }

    [Fact]
    public void BacklogLink_CanBeSetAndRetrieved()
    {
        var data = new DashboardData { BacklogLink = "https://dev.azure.com/org/project" };
        Assert.Equal("https://dev.azure.com/org/project", data.BacklogLink);
    }

    [Fact]
    public void CurrentMonth_CanBeSetAndRetrieved()
    {
        var data = new DashboardData { CurrentMonth = "April" };
        Assert.Equal("April", data.CurrentMonth);
    }

    #endregion

    #region JSON Deserialization for Header Fields

    [Fact]
    public void Deserialize_HeaderFields_FromJsonPropertyNames()
    {
        var json = """{"title":"T","subtitle":"S","backlogLink":"https://link","currentMonth":"Apr","months":[],"timeline":{"startDate":"","endDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.NotNull(data);
        Assert.Equal("T", data!.Title);
        Assert.Equal("S", data.Subtitle);
        Assert.Equal("https://link", data.BacklogLink);
        Assert.Equal("Apr", data.CurrentMonth);
    }

    [Fact]
    public void Deserialize_MissingHeaderFields_DefaultToEmpty()
    {
        var json = """{"months":[],"timeline":{"startDate":"","endDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";

        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.NotNull(data);
        Assert.Equal(string.Empty, data!.Title);
        Assert.Equal(string.Empty, data.Subtitle);
        Assert.Equal(string.Empty, data.BacklogLink);
        Assert.Equal(string.Empty, data.CurrentMonth);
    }

    #endregion

    #region Special Characters in Header Fields

    [Fact]
    public void Title_HandlesSpecialCharacters()
    {
        var data = new DashboardData { Title = "Project <Alpha> & \"Beta\"" };
        var json = JsonSerializer.Serialize(data, JsonOpts);
        var round = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.Equal("Project <Alpha> & \"Beta\"", round!.Title);
    }

    [Fact]
    public void BacklogLink_HandlesQueryParameters()
    {
        var url = "https://dev.azure.com/org/project/_backlogs?q=search&filter=active";
        var data = new DashboardData { BacklogLink = url };
        var json = JsonSerializer.Serialize(data, JsonOpts);
        var round = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.Equal(url, round!.BacklogLink);
    }

    [Fact]
    public void Subtitle_HandlesLongStrings()
    {
        var longSubtitle = string.Join(" - ", Enumerable.Range(1, 50).Select(i => $"Word{i}"));
        var data = new DashboardData { Subtitle = longSubtitle };
        var json = JsonSerializer.Serialize(data, JsonOpts);
        var round = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.Equal(longSubtitle, round!.Subtitle);
    }

    [Fact]
    public void CurrentMonth_HandlesVariousFormats()
    {
        var months = new[] { "January", "Feb", "3", "April 2026", "Mai" };
        foreach (var month in months)
        {
            var data = new DashboardData { CurrentMonth = month };
            Assert.Equal(month, data.CurrentMonth);
        }
    }

    #endregion

    #region Serialization Output

    [Fact]
    public void Serialize_HeaderFields_UsesCamelCase()
    {
        var data = new DashboardData
        {
            Title = "Test",
            BacklogLink = "https://link",
            CurrentMonth = "April"
        };

        var json = JsonSerializer.Serialize(data, JsonOpts);

        Assert.Contains("\"title\"", json);
        Assert.Contains("\"backlogLink\"", json);
        Assert.Contains("\"currentMonth\"", json);
        Assert.DoesNotContain("\"Title\"", json);
        Assert.DoesNotContain("\"BacklogLink\"", json);
        Assert.DoesNotContain("\"CurrentMonth\"", json);
    }

    #endregion
}