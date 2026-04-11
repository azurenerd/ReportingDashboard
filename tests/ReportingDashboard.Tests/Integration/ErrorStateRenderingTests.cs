using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class ErrorStateRenderingTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public ErrorStateRenderingTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task MissingData_StillServesPage()
    {
        var client = _fixture.CreateClientWithMissingData();

        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("<!DOCTYPE html");
    }

    [Fact]
    public async Task MalformedData_StillServesPage()
    {
        var client = _fixture.CreateClientWithMalformedData();

        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task ValidData_ServesPage()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("<!DOCTYPE html");
    }
}