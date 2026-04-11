using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class StaticFileTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public StaticFileTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CssFile_IsServedAsStaticFile()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/css/dashboard.css");

        response.IsSuccessStatusCode.Should().BeTrue();
        var contentType = response.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("text/css");
    }

    [Fact]
    public async Task DataJson_IsServedAsStaticFile()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/data.json");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}