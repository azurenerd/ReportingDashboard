using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AgentSquad.Tests.Integration;

public class StaticFileServingTests : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        
        // Verify application started successfully
        var maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await _client.GetAsync("/");
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    break;
                }
            }
            catch (HttpRequestException) when (i < maxRetries - 1)
            {
                await Task.Delay(500);
            }
        }
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _factory?.Dispose();
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetCssFile_ReturnsOkWithCorrectContentType()
    {
        var response = await _client.GetAsync("/css/base.css");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/css", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetJsFile_ReturnsOkWithCorrectContentType()
    {
        var response = await _client.GetAsync("/js/dashboard.js");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/javascript", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task StaticFile_IncludesCacheHeaders()
    {
        var response = await _client.GetAsync("/css/base.css");
        
        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(response.Headers.CacheControl.MaxAge.HasValue);
        Assert.True(response.Headers.CacheControl.MaxAge.Value.TotalSeconds > 0);
    }

    [Fact]
    public async Task StaticFile_VerifiesStaticFileMiddlewareActive()
    {
        // Verify static middleware is configured by checking for typical headers
        var response = await _client.GetAsync("/css/base.css");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentLength);
        Assert.True(response.Content.Headers.ContentLength > 0);
    }

    [Fact]
    public async Task BlazorFrameworkFile_ReturnsOk()
    {
        var response = await _client.GetAsync("/_framework/blazor.web.js");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/javascript", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task NonExistentFile_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/nonexistent/file.css");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}