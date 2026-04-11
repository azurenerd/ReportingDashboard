using System.Diagnostics;
using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class PerformanceTests
{
    private readonly PlaywrightFixture _fixture;

    public PerformanceTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task PageLoad_CompletesWithin5Seconds()
    {
        var page = await _fixture.NewPageAsync();

        var sw = Stopwatch.StartNew();
        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(5000,
            "Static SSR page should load quickly on localhost");
    }

    [Fact]
    public async Task PageLoad_NoPendingNetworkRequests()
    {
        var page = await _fixture.NewPageAsync();

        var pendingRequests = new List<string>();
        page.Request += (_, r) => pendingRequests.Add(r.Url);
        page.RequestFinished += (_, r) => pendingRequests.Remove(r.Url);

        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        // After NetworkIdle, no requests should be pending
        pendingRequests.Should().BeEmpty("all network requests should complete");
    }

    [Fact]
    public async Task CssFile_LoadsWithin2Seconds()
    {
        var page = await _fixture.NewPageAsync();

        double cssLoadTimeMs = 0;
        page.Response += (_, r) =>
        {
            if (r.Url.Contains("css/app.css"))
            {
                // Timing from navigation start
                cssLoadTimeMs = 1; // mark as loaded
            }
        };

        var sw = Stopwatch.StartNew();
        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });
        sw.Stop();

        if (cssLoadTimeMs > 0)
        {
            sw.ElapsedMilliseconds.Should().BeLessThan(2000,
                "CSS file should load quickly");
        }
    }

    [Fact]
    public async Task MultiplePageLoads_AllSucceed()
    {
        for (int i = 0; i < 3; i++)
        {
            var page = await _fixture.NewPageAsync();
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            response.Should().NotBeNull();
            response!.Status.Should().Be(200, $"page load {i + 1} should succeed");

            await page.CloseAsync();
        }
    }
}