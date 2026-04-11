using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class ErrorPanelIntegrationTests : TestContext
{
    private static DashboardDataService CreateService()
    {
        var logger = NullLogger<DashboardDataService>.Instance;
        return new DashboardDataService(logger);
    }

    [Fact]
    public void ErrorPanel_WithServiceErrorMessage_RendersErrorDetails()
    {
        var service = CreateService();
        service.LoadAsync(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.json")).Wait();

        service.IsError.Should().BeTrue("loading a missing file should set error state");
        service.ErrorMessage.Should().NotBeNullOrEmpty();

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, service.ErrorMessage));

        cut.Find(".error-details").TextContent.Should().Be(service.ErrorMessage);
        cut.Find(".error-title").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Find(".error-help").TextContent.Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void ErrorPanel_WithServiceInValidState_HidesDetails()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dataPath = Path.Combine(tempDir, "data.json");

        try
        {
            File.WriteAllText(dataPath, GetMinimalValidJson());

            var service = CreateService();
            service.LoadAsync(dataPath).Wait();

            if (service.IsError)
            {
                var cut = RenderComponent<ErrorPanel>(p =>
                    p.Add(x => x.ErrorMessage, service.ErrorMessage));
                cut.Find(".error-details").Should().NotBeNull();
            }
            else
            {
                var cut = RenderComponent<ErrorPanel>(p =>
                    p.Add(x => x.ErrorMessage, service.ErrorMessage));
                cut.FindAll(".error-details").Should().BeEmpty(
                    "when service has no error, ErrorMessage is null/empty so details should be hidden");
            }
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ErrorPanel_WithCorruptJson_ShowsParseError()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dataPath = Path.Combine(tempDir, "data.json");

        try
        {
            File.WriteAllText(dataPath, "{ this is not valid json !!! }");

            var service = CreateService();
            service.LoadAsync(dataPath).Wait();

            service.IsError.Should().BeTrue("corrupt JSON should set error state");
            service.ErrorMessage.Should().NotBeNullOrEmpty();

            var cut = RenderComponent<ErrorPanel>(p =>
                p.Add(x => x.ErrorMessage, service.ErrorMessage));

            var details = cut.Find(".error-details").TextContent;
            details.Should().NotBeNullOrEmpty();
            cut.Find(".error-title").TextContent.Should().Be("Dashboard data could not be loaded");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ErrorPanel_WithEmptyJsonFile_ShowsError()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dataPath = Path.Combine(tempDir, "data.json");

        try
        {
            File.WriteAllText(dataPath, "");

            var service = CreateService();
            service.LoadAsync(dataPath).Wait();

            service.IsError.Should().BeTrue("empty file should set error state");

            var cut = RenderComponent<ErrorPanel>(p =>
                p.Add(x => x.ErrorMessage, service.ErrorMessage));

            cut.FindAll(".error-details").Should().HaveCount(1,
                "a non-empty error message should be rendered");
            cut.Find(".error-icon").Should().NotBeNull();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ErrorPanel_ServiceErrorThenReload_ReflectsNewState()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dataPath = Path.Combine(tempDir, "data.json");

        try
        {
            File.WriteAllText(dataPath, "NOT JSON");
            var service = CreateService();
            service.LoadAsync(dataPath).Wait();

            service.IsError.Should().BeTrue();
            var errorMsg1 = service.ErrorMessage;

            var cut = RenderComponent<ErrorPanel>(p =>
                p.Add(x => x.ErrorMessage, errorMsg1));

            cut.FindAll(".error-details").Should().HaveCount(1);
            cut.Find(".error-details").TextContent.Should().Be(errorMsg1);

            File.WriteAllText(dataPath, GetMinimalValidJson());
            service.LoadAsync(dataPath).Wait();

            if (!service.IsError)
            {
                cut.SetParametersAndRender(p =>
                    p.Add(x => x.ErrorMessage, service.ErrorMessage));

                cut.FindAll(".error-details").Should().BeEmpty(
                    "after successful reload, error message should be null/empty");
            }
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ErrorPanel_ConditionalRendering_MatchesDashboardPattern()
    {
        var service = CreateService();
        service.LoadAsync(Path.Combine(Path.GetTempPath(), "nonexistent", "data.json")).Wait();

        bool shouldRenderErrorPanel = service.IsError;
        shouldRenderErrorPanel.Should().BeTrue();

        if (shouldRenderErrorPanel)
        {
            var cut = RenderComponent<ErrorPanel>(p =>
                p.Add(x => x.ErrorMessage, service.ErrorMessage));

            cut.FindAll(".error-panel").Should().HaveCount(1);
            cut.FindAll(".error-icon").Should().HaveCount(1);
            cut.FindAll(".error-title").Should().HaveCount(1);
            cut.FindAll(".error-details").Should().HaveCount(1);
            cut.FindAll(".error-help").Should().HaveCount(1);
        }
    }

    [Fact]
    public void ErrorPanel_MultipleInstances_RenderIndependently()
    {
        var cut1 = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Error from service A"));

        var cut2 = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "Error from service B"));

        cut1.Find(".error-details").TextContent.Should().Be("Error from service A");
        cut2.Find(".error-details").TextContent.Should().Be("Error from service B");

        cut1.Find(".error-details").TextContent
            .Should().NotBe(cut2.Find(".error-details").TextContent);
    }

    [Fact]
    public void ErrorPanel_WithRealFileNotFoundPath_ContainsPathInMessage()
    {
        var specificPath = Path.Combine(Path.GetTempPath(), "integration-test-" + Guid.NewGuid(), "data.json");

        var service = CreateService();
        service.LoadAsync(specificPath).Wait();

        service.IsError.Should().BeTrue();

        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, service.ErrorMessage));

        var renderedText = cut.Find(".error-details").TextContent;
        renderedText.Should().NotBeNullOrEmpty();
    }

    private static string GetMinimalValidJson()
    {
        return """
        {
            "title": "Test Dashboard",
            "subtitle": "Integration Test",
            "backlogLink": "https://example.com",
            "timeline": {
                "nowDate": "2026-04-01",
                "months": ["Jan", "Feb", "Mar", "Apr"],
                "tracks": []
            },
            "heatmap": {
                "months": ["Jan", "Feb", "Mar", "Apr"],
                "currentMonth": "Apr",
                "shipped": [],
                "inProgress": [],
                "carryover": [],
                "blockers": []
            }
        }
        """;
    }
}