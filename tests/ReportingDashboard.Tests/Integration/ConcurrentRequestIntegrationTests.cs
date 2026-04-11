using System.Net;
using FluentAssertions;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class ConcurrentRequestIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public ConcurrentRequestIntegrationTests()
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
    public async Task GetRoot_ConcurrentRequests_AllSucceed()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _client.GetAsync("/"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("Test Project");
        }
    }

    [Fact]
    public async Task GetRoot_ConcurrentRequests_WithError_AllShowError()
    {
        _factory.DeleteDataJson();

        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.GetAsync("/"))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        foreach (var response in responses)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var html = await response.Content.ReadAsStringAsync();
            html.Should().Contain("error-container");
        }
    }

    [Fact]
    public async Task GetRoot_MultipleEndpoints_AllRespond()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var rootTask = _client.GetAsync("/");
        var notFoundTask = _client.GetAsync("/does-not-exist");

        await Task.WhenAll(rootTask, notFoundTask);

        rootTask.Result.StatusCode.Should().Be(HttpStatusCode.OK);
        notFoundTask.Result.StatusCode.Should().Be(HttpStatusCode.OK);

        var rootHtml = await rootTask.Result.Content.ReadAsStringAsync();
        var notFoundHtml = await notFoundTask.Result.Content.ReadAsStringAsync();

        rootHtml.Should().Contain("Test Project");
        notFoundHtml.Should().Contain("Page not found");
    }
}