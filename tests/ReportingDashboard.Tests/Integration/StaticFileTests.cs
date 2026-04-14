using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class StaticFileTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public StaticFileTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CssEndpoint_DoesNotReturn500()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/css/dashboard.css");

        ((int)response.StatusCode).Should().BeLessThan(500);
    }

    [Fact]
    public async Task NonExistentStaticFile_Returns404()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/css/nonexistent.css");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}