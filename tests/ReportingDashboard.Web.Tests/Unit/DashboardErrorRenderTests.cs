using System;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Components.Pages;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardErrorRenderTests : TestContext
{
    public DashboardErrorRenderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private sealed class StubService : IDashboardDataService
    {
        private readonly DashboardLoadResult _result;
        public StubService(DashboardLoadResult r) { _result = r; }
        public event EventHandler? DataChanged { add { } remove { } }
        public DashboardLoadResult GetCurrent() => _result;
    }

    private sealed class ThrowingService : IDashboardDataService
    {
        public event EventHandler? DataChanged { add { } remove { } }
        public DashboardLoadResult GetCurrent() => throw new InvalidOperationException("boom");
    }

    [Theory]
    [InlineData("NotFound")]
    [InlineData("ParseError")]
    [InlineData("ValidationError")]
    public void Dashboard_OnError_RendersBannerAndDegradedPlaceholders(string kind)
    {
        var err = new DashboardLoadError("wwwroot/data.json", "some problem", null, null, kind);
        Services.AddSingleton<IDashboardDataService>(
            new StubService(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow)));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().HaveCount(1);
        cut.FindAll(".hdr").Should().NotBeEmpty();
        cut.FindAll(".tl-area svg").Should().NotBeEmpty();
        cut.FindAll(".hm-grid").Should().NotBeEmpty();
        cut.Markup.Should().NotContain("blazor.server.js");
    }

    [Fact]
    public void Dashboard_WhenServiceThrows_CatchesAndRendersErrorBanner()
    {
        Services.AddSingleton<IDashboardDataService>(new ThrowingService());

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().HaveCount(1);
        cut.Markup.Should().Contain("Failed to load data.json").And.Contain("boom");
    }

    [Fact]
    public void Dashboard_WhenServiceReturnsNull_RendersErrorBanner()
    {
        Services.AddSingleton<IDashboardDataService>(new StubService(null!));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().HaveCount(1);
        cut.Markup.Should().Contain("DashboardDataService returned null");
    }

    [Fact]
    public void Dashboard_ParseErrorWithLineAndColumn_AppearsInBannerText()
    {
        var err = new DashboardLoadError("data.json", "Unexpected token", 42, 3, "ParseError");
        Services.AddSingleton<IDashboardDataService>(
            new StubService(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow)));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("line 42").And.Contain("column 3");
    }
}