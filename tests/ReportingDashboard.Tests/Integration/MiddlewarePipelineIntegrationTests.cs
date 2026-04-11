using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the middleware pipeline: antiforgery, static files,
/// exception handling, and response characteristics.
/// </summary>
[Trait("Category", "Integration")]
public class MiddlewarePipelineIntegrationTests : IDisposable
{
    private readonly string _tempWebRoot;

    public MiddlewarePipelineIntegrationTests()
    {
        _tempWebRoot = Path.Combine(Path.GetTempPath(), $"Middleware_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempWebRoot);
        Directory.CreateDirectory(Path.Combine(_tempWebRoot, "css"));
        File.WriteAllText(Path.Combine(_tempWebRoot, "data.json"),
            TestDataHelper.CreateValidDataJsonString());
        File.WriteAllText(Path.Combine(_tempWebRoot, "css", "dashboard.css"),
            "* { margin: 0; } body { width: 1920px; height: 1080px; }");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWebRoot))
            Directory.Delete(_tempWebRoot, recursive: true);
    }

    private WebApplicationFactory<ReportingDashboard.Components.App> CreateFactory()
    {
        return new WebApplicationFactory<ReportingDashboard.Components.App>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("WebRootPath", _tempWebRoot);
            });
    }

    [Fact]
    public async Task StaticFiles_AreServedBeforeRouting()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        // Static file should be served directly without Blazor routing
        var response = await client.GetAsync("/data.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task StaticFiles_CssHasCorrectContentType()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/css/dashboard.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task HtmlResponse_HasUtf8Charset()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("charset", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HtmlResponse_HasLangAttribute()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("lang=\"en\"", html);
    }

    [Fact]
    public async Task HtmlResponse_HasBaseHref()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("base href", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MultipleRequestsToSameEndpoint_AllSucceed()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        for (int i = 0; i < 5; i++)
        {
            var response = await client.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task ConcurrentRequests_AllSucceed()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.GetAsync("/"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
    }

    [Fact]
    public async Task HeadRequest_ReturnsOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Head, "/css/dashboard.css");
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}