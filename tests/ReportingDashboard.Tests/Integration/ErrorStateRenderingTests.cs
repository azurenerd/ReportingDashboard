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
    public async Task MissingDataJson_ShowsErrorBanner_WithoutCrash()
    {
        using var client = _fixture.CreateClientWithMissingData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error-banner");
        content.Should().Contain("Data Error");
    }

    [Fact]
    public async Task MalformedJson_ShowsErrorBanner_WithParseDetails()
    {
        using var client = _fixture.CreateClientWithMalformedData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error-banner");
        content.Should().Contain("Invalid JSON");
    }

    [Fact]
    public async Task ValidData_DoesNotShowErrorBanner()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().NotContain("error-banner");
    }
}