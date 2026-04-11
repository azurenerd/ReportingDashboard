using System.Net;
using FluentAssertions;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class HeatmapRenderingIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public HeatmapRenderingIntegrationTests()
    {
        _factory = new WebAppFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static string BuildJsonWithHeatmap(
        string currentMonth = "Apr",
        string[]? heatmapMonths = null,
        string? categoriesJson = null)
    {
        var months = heatmapMonths ?? new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        var monthsStr = string.Join(",", months.Select(m => $"\"{m}\""));

        var categories = categoriesJson ?? """
        [
            { "name": "Shipped", "cssClass": "ship", "emoji": "✅", "items": { "Jan": ["Feature A"], "Apr": ["Feature B", "Feature C"] } },
            { "name": "In Progress", "cssClass": "prog", "emoji": "🔄", "items": { "Apr": ["Work Item X"] } },
            { "name": "Carryover", "cssClass": "carry", "emoji": "📦", "items": {} },
            { "name": "Blockers", "cssClass": "block", "emoji": "🚫", "items": { "Feb": ["Blocker 1"] } }
        ]
        """;

        return $$"""
        {
            "project": { "title": "Heatmap Test", "subtitle": "Sub", "backlogUrl": "", "currentMonth": "{{currentMonth}}" },
            "timeline": { "months": [{{monthsStr}}], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": [{{monthsStr}}], "categories": {{categories}} }
        }
        """;
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_RendersHeatmapGrid()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("class=\"hm-wrap\"");
        html.Should().Contain("class=\"hm-grid\"");
        html.Should().Contain("MONTHLY EXECUTION HEATMAP");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_RendersColumnHeaders()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("hm-col-hdr");
        html.Should().Contain("Jan");
        html.Should().Contain("Feb");
        html.Should().Contain("Mar");
        html.Should().Contain("Apr");
        html.Should().Contain("May");
        html.Should().Contain("Jun");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_RendersCornerCell()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("hm-corner");
        html.Should().Contain("STATUS");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_RendersRowHeaders()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("ship-hdr");
        html.Should().Contain("SHIPPED");
        html.Should().Contain("prog-hdr");
        html.Should().Contain("IN PROGRESS");
        html.Should().Contain("carry-hdr");
        html.Should().Contain("CARRYOVER");
        html.Should().Contain("block-hdr");
        html.Should().Contain("BLOCKERS");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_RendersItemsInCells()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Feature A");
        html.Should().Contain("Feature B");
        html.Should().Contain("Feature C");
        html.Should().Contain("Work Item X");
        html.Should().Contain("Blocker 1");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_RendersEmptyCellDash()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        // Carryover has no items for any month, so all its cells should have dashes
        html.Should().Contain("empty-cell");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_HighlightsCurrentMonth()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap(currentMonth: "Apr"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("now-hdr");
        html.Should().Contain("now-badge");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_CurrentMonthCellsHaveCurrentClass()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap(currentMonth: "Apr"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        // Current month cells should have "current" class
        html.Should().Contain("current");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_GridTemplateColumnsMatchMonthCount()
    {
        var months = new[] { "Jan", "Feb", "Mar" };
        _factory.WriteDataJson(BuildJsonWithHeatmap(heatmapMonths: months, currentMonth: "Jan"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("grid-template-columns:160px repeat(3, 1fr)");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_GridTemplateRowsMatchCategoryCount()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        // 4 categories
        html.Should().Contain("grid-template-rows:36px repeat(4, 1fr)");
    }

    [Fact]
    public async Task GetRoot_WithSingleCategory_RendersMinimalGrid()
    {
        var categories = """
        [
            { "name": "Shipped", "cssClass": "ship", "emoji": "✅", "items": { "Jan": ["A"] } }
        ]
        """;
        _factory.WriteDataJson(BuildJsonWithHeatmap(
            heatmapMonths: new[] { "Jan" },
            categoriesJson: categories,
            currentMonth: "Jan"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("grid-template-columns:160px repeat(1, 1fr)");
        html.Should().Contain("grid-template-rows:36px repeat(1, 1fr)");
        html.Should().Contain("SHIPPED");
        html.Should().Contain(">A<");
    }

    [Fact]
    public async Task GetRoot_WithEmptyCategories_RendersGridWithHeaderOnly()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap(
            heatmapMonths: new[] { "Jan", "Feb" },
            categoriesJson: "[]",
            currentMonth: "Jan"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("hm-grid");
        html.Should().Contain("hm-corner");
    }

    [Fact]
    public async Task GetRoot_WithHeatmapData_EmojiRenderedInRowHeaders()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("✅");
        html.Should().Contain("🔄");
        html.Should().Contain("📦");
        html.Should().Contain("🚫");
    }

    [Fact]
    public async Task GetRoot_WithCurrentMonthNotMatching_NoNowHighlight()
    {
        _factory.WriteDataJson(BuildJsonWithHeatmap(
            currentMonth: "Dec",
            heatmapMonths: new[] { "Jan", "Feb" }));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("now-hdr");
        html.Should().NotContain("now-badge");
    }
}