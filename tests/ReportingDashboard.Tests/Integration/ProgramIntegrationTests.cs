using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class ProgramIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProgramIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_Returns200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Root_ReturnsHtmlContentType()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task Get_Root_ContainsDocTypeHtml()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("<!DOCTYPE html", Exactly.Once());
    }

    [Fact]
    public async Task Get_Root_ContainsBlazorErrorUiHiddenStyle()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("blazor-error-ui");
        content.Should().Contain("display: none !important");
    }

    [Fact]
    public async Task Get_Root_HasNoCdnLinks()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().NotContain("cdn.jsdelivr.net");
        content.Should().NotContain("unpkg.com");
        content.Should().NotContain("cdnjs.cloudflare.com");
        content.Should().NotContain("ajax.googleapis.com");
    }
}