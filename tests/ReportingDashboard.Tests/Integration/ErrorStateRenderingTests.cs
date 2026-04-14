using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class ErrorStateRenderingTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public ErrorStateRenderingTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MissingDataJson_ReturnsOK_WithoutCrash()
    {
        using var client = _fixture.CreateClientWithMissingData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task MalformedJson_ReturnsOK_WithoutCrash()
    {
        using var client = _fixture.CreateClientWithMalformedData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ValidData_ReturnsOK()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}