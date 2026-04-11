using System.Net;
using FluentAssertions;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DataFileReloadIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public DataFileReloadIntegrationTests()
    {
        _factory = new WebAppFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetRoot_AfterDataFileChange_ReflectsNewTitle()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Version 1"));
        var html1 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html1.Should().Contain("Version 1");

        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Version 2"));
        var html2 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html2.Should().Contain("Version 2");
        html2.Should().NotContain("Version 1");
    }

    [Fact]
    public async Task GetRoot_AfterDataFileDeleted_ShowsError()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());
        var response1 = await _client.GetAsync("/");
        var html1 = await response1.Content.ReadAsStringAsync();
        html1.Should().Contain("class=\"hdr\"");

        _factory.DeleteDataJson();
        var response2 = await _client.GetAsync("/");
        var html2 = await response2.Content.ReadAsStringAsync();
        html2.Should().Contain("error-container");
        html2.Should().Contain("data.json not found");
    }

    [Fact]
    public async Task GetRoot_AfterDataFileRecreated_ShowsDataAgain()
    {
        _factory.DeleteDataJson();
        var html1 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html1.Should().Contain("error-container");

        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Recovered"));
        var html2 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html2.Should().Contain("Recovered");
        html2.Should().NotContain("error-container");
    }

    [Fact]
    public async Task GetRoot_AfterValidToInvalid_ShowsValidationError()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());
        var html1 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html1.Should().Contain("class=\"hdr\"");

        // Replace with data that fails validation
        var invalidJson = """
        {
            "project": { "title": "", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(invalidJson);
        var html2 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html2.Should().Contain("error-container");
        html2.Should().Contain("project.title");
    }

    [Fact]
    public async Task GetRoot_AfterCorruptedToValid_Recovers()
    {
        _factory.WriteDataJson("{ not valid json !!!");
        var html1 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html1.Should().Contain("error-container");

        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Back Online"));
        var html2 = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
        html2.Should().Contain("Back Online");
        html2.Should().NotContain("error-container");
    }

    [Fact]
    public async Task GetRoot_RapidFileChanges_AlwaysReflectsLatest()
    {
        for (int i = 1; i <= 5; i++)
        {
            _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: $"Iteration {i}"));
            var html = await (await _client.GetAsync("/")).Content.ReadAsStringAsync();
            html.Should().Contain($"Iteration {i}");
        }
    }
}