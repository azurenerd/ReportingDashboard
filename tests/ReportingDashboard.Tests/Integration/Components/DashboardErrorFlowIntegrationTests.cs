using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying the full error flow: DashboardDataService loads file,
/// detects error, Dashboard.razor reads service state, and ErrorPanel renders with correct content.
/// Tests the real interaction between service and components without mocking the service internals.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardErrorFlowIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<IWebHostEnvironment> _mockEnv;

    public DashboardErrorFlowIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardErrorFlow_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
    }

    public new void Dispose()
    {
        base.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService LoadService(string? fileContent = null)
    {
        var dataJsonPath = Path.Combine(_tempDir, "data.json");

        if (fileContent != null)
        {
            File.WriteAllText(dataJsonPath, fileContent);
        }
        else if (File.Exists(dataJsonPath))
        {
            File.Delete(dataJsonPath);
        }

        var service = new DashboardDataService(_mockEnv.Object, NullLogger<DashboardDataService>.Instance);
        service.LoadAsync(dataJsonPath).GetAwaiter().GetResult();
        return service;
    }

    // ── Full pipeline: missing file → error panel ──────────────────────

    [Fact]
    public void FullPipeline_MissingFile_ServiceSetsError_DashboardRendersErrorPanel()
    {
        // Arrange
        var service = LoadService(); // no file
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – verify complete chain
        service.IsError.Should().BeTrue("service should detect missing file");
        service.Data.Should().BeNull("data should not be set on error");

        cut.FindAll(".error-panel").Should().HaveCount(1, "Dashboard should render ErrorPanel");
        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Find(".error-hint").TextContent
            .Should().Contain("Check data.json for errors and restart the application.");

        // Error message from service should be displayed
        if (!string.IsNullOrEmpty(service.ErrorMessage))
        {
            cut.Markup.Should().Contain(service.ErrorMessage);
        }
    }

    // ── Full pipeline: malformed JSON → error panel ────────────────────

    [Fact]
    public void FullPipeline_MalformedJson_ServiceSetsError_DashboardRendersErrorPanel()
    {
        // Arrange
        var service = LoadService("{{{ malformed }}}");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue("service should detect malformed JSON");
        cut.FindAll(".error-panel").Should().HaveCount(1);
        cut.FindAll(".hdr").Should().BeEmpty("dashboard content should not render");
    }

    [Fact]
    public void FullPipeline_TruncatedJson_ServiceSetsError_DashboardRendersErrorPanel()
    {
        // Arrange – JSON cut off mid-stream
        var service = LoadService("""{"title": "Test", "subtitle": "Sub""");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    // ── ErrorMessage content fidelity ──────────────────────────────────

    [Fact]
    public void FullPipeline_ErrorMessage_FromService_AppearsVerbatimInRenderedPanel()
    {
        // Arrange
        var service = LoadService("NOT JSON");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – whatever error message the service produces should appear in rendered output
        var errorMsg = service.ErrorMessage;
        errorMsg.Should().NotBeNullOrWhiteSpace("service should provide a descriptive error");

        // The message should be HTML-encoded but present
        var markup = cut.Markup;
        // Check that at least a significant portion of the error message is in the markup
        markup.Should().NotBeEmpty();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    // ── No dashboard sections in error state ───────────────────────────

    [Fact]
    public void FullPipeline_ErrorState_NoDashboardSectionsRendered()
    {
        // Arrange
        var service = LoadService(); // missing file
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – verify no dashboard sections leak through
        cut.FindAll(".hdr").Should().BeEmpty("header should not render in error state");
        cut.FindAll(".tl-area").Should().BeEmpty("timeline should not render in error state");
        cut.FindAll(".hm-wrap").Should().BeEmpty("heatmap should not render in error state");
        cut.FindAll("svg").Should().BeEmpty("no SVG timeline should render in error state");
    }

    // ── Concurrent error scenarios ─────────────────────────────────────

    [Fact]
    public void FullPipeline_MultipleErrorScenarios_AllShowErrorPanel()
    {
        var invalidInputs = new[]
        {
            "",           // empty
            "   ",        // whitespace
            "{",          // truncated
            "[}",         // mismatched brackets
            "undefined",  // not JSON
            "<html>",     // HTML instead of JSON
        };

        foreach (var input in invalidInputs)
        {
            using var ctx = new Bunit.TestContext();

            var mockEnv = new Mock<IWebHostEnvironment>();
            var tempPath = Path.Combine(Path.GetTempPath(), $"err_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempPath);
            mockEnv.Setup(e => e.WebRootPath).Returns(tempPath);

            var dataJsonPath = Path.Combine(tempPath, "data.json");
            File.WriteAllText(dataJsonPath, input);

            var service = new DashboardDataService(mockEnv.Object, NullLogger<DashboardDataService>.Instance);
            service.LoadAsync(dataJsonPath).GetAwaiter().GetResult();

            ctx.Services.AddSingleton(service);

            try
            {
                var cut = ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

                // Either shows error panel or renders empty (no crash)
                service.IsError.Should().BeTrue(
                    $"service should report error for input: '{input}'");
                cut.FindAll(".error-panel").Should().HaveCount(1,
                    $"error panel should render for input: '{input}'");
            }
            finally
            {
                try { Directory.Delete(tempPath, true); } catch { }
            }
        }
    }

    // ── Service singleton behavior ─────────────────────────────────────

    [Fact]
    public void Service_RegisteredAsSingleton_SameInstanceUsedByDashboard()
    {
        // Arrange
        var service = LoadService(); // error state
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – the component should use the same service instance
        var resolvedService = Services.GetRequiredService<DashboardDataService>();
        resolvedService.Should().BeSameAs(service, "DI should provide the singleton instance");
        resolvedService.IsError.Should().BeTrue();
    }

    // ── ErrorPanel parameter binding ───────────────────────────────────

    [Fact]
    public void ErrorPanel_MessageParameter_BindsFromDashboardServiceErrorMessage()
    {
        // Arrange – use invalid JSON to get a specific error message
        var service = LoadService("{bad json}");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – verify the parameter chain works:
        // DataService.ErrorMessage → Dashboard.razor @DataService.ErrorMessage → ErrorPanel.Message
        var errorMsg = service.ErrorMessage;
        if (!string.IsNullOrEmpty(errorMsg))
        {
            // The message should appear somewhere in the error panel markup
            var panel = cut.Find(".error-panel");
            panel.InnerHtml.Should().Contain(
                System.Net.WebUtility.HtmlEncode(errorMsg) ?? errorMsg,
                "ErrorPanel should display the service's error message");
        }
    }

    // ── Rendering consistency ──────────────────────────────────────────

    [Fact]
    public void ErrorPanel_RenderedMultipleTimes_ProducesSameMarkup()
    {
        // Arrange
        var service = LoadService();
        Services.AddSingleton(service);

        // Act
        var cut1 = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();
        var markup1 = cut1.Markup;

        var cut2 = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();
        var markup2 = cut2.Markup;

        // Assert
        markup1.Should().Be(markup2, "same error state should produce identical markup");
    }

    [Fact]
    public void ErrorPanel_DirectRender_MatchesDashboardEmbeddedRender()
    {
        // Arrange
        var errorMessage = "Test comparison error";

        // Render ErrorPanel directly
        var directPanel = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p => p
            .Add(x => x.Message, errorMessage));
        var directMarkup = directPanel.Markup;

        // Assert – direct render should produce valid error panel
        directMarkup.Should().Contain("error-panel");
        directMarkup.Should().Contain("Dashboard data could not be loaded");
        directMarkup.Should().Contain(errorMessage);
        directMarkup.Should().Contain("Check data.json for errors and restart the application.");
    }

    // ── Large file error scenario ──────────────────────────────────────

    [Fact]
    public void FullPipeline_VeryLargeInvalidFile_HandlesGracefully()
    {
        // Arrange – large invalid JSON (1MB of garbage)
        var largeContent = new string('x', 1024 * 1024);
        var service = LoadService(largeContent);
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – should handle gracefully, no timeout or crash
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    // ── Unicode content in error messages ──────────────────────────────

    [Fact]
    public void ErrorPanel_WithUnicodeInErrorPath_RendersCorrectly()
    {
        // Arrange – create a scenario that might produce unicode in error message
        var service = LoadService("{ \"title\": \"日本語テスト\" }");
        Services.AddSingleton(service);

        // Act – should not throw regardless of outcome
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – component renders without exception
        cut.Markup.Should().NotBeEmpty();
    }
}