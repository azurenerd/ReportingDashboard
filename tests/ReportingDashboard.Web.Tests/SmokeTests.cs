using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportingDashboard.Web.Tests;

public class SmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_Renders_Placeholders_And_Is_Static_Ssr()
    {
        var client = _factory.CreateClient();

        var resp = await client.GetAsync("/");
        resp.IsSuccessStatusCode.Should().BeTrue();
        var body = await resp.Content.ReadAsStringAsync();

        body.Should().Contain("Timeline placeholder");
        body.Should().Contain("Heatmap placeholder");
        body.Should().NotContain("blazor.server.js");
        body.Should().Contain("1920px");
    }

    [Fact]
    public async Task Healthz_Returns_Ok()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/healthz");
        resp.IsSuccessStatusCode.Should().BeTrue();
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Be("ok");
    }

    [Fact]
    public async Task DataJson_Served_As_Json()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/data.json");
        resp.IsSuccessStatusCode.Should().BeTrue();
        resp.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}