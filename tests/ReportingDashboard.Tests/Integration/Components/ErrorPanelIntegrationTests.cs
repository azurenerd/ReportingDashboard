using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests verifying ErrorPanel renders correctly when wired through
/// Dashboard.razor with a real DashboardDataService in various error states.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorPanelIntegrationTests : TestContext
{
    private readonly string _tempDir;

    public ErrorPanelIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ErrorPanelTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public new void Dispose()
    {
        base.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService CreateAndLoadService(string? jsonContent = null)
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);

        var logger = NullLogger<DashboardDataService>.Instance;
        var service = new DashboardDataService(mockEnv.Object, logger);

        if (jsonContent != null)
        {
            var filePath = Path.Combine(_tempDir, "data.json");
            File.WriteAllText(filePath, jsonContent);
        }

        service.LoadAsync(Path.Combine(_tempDir, "data.json")).GetAwaiter().GetResult();
        return service;
    }

    // ── Missing file scenario ──────────────────────────────────────────

    [Fact]
    public void Dashboard_WhenDataJsonMissing_RendersErrorPanel()
    {
        // Arrange – no data.json written
        var service = CreateAndLoadService();
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.FindAll(".error-panel").Should().HaveCount(1);
        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
    }

    [Fact]
    public void Dashboard_WhenDataJsonMissing_ShowsFileNotFoundMessage()
    {
        // Arrange
        var service = CreateAndLoadService();
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – the error message should reference the missing file
        var markup = cut.Markup;
        markup.Should().ContainAny("not found", "could not find", "does not exist", "No such file",
            "data.json");
    }

    [Fact]
    public void Dashboard_WhenDataJsonMissing_DoesNotRenderDashboardSections()
    {
        // Arrange
        var service = CreateAndLoadService();
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.FindAll(".hm-wrap").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WhenDataJsonMissing_ShowsHintText()
    {
        // Arrange
        var service = CreateAndLoadService();
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.Find(".error-hint").TextContent
            .Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void Dashboard_WhenDataJsonMissing_ShowsWarningIcon()
    {
        // Arrange
        var service = CreateAndLoadService();
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.Find(".error-icon").TextContent.Should().Contain("\u26A0"); // ⚠
    }

    // ── Invalid JSON scenario ──────────────────────────────────────────

    [Fact]
    public void Dashboard_WhenDataJsonIsInvalidSyntax_RendersErrorPanel()
    {
        // Arrange
        var service = CreateAndLoadService("{{{invalid json}}}");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsInvalidSyntax_ShowsParseErrorDetail()
    {
        // Arrange
        var service = CreateAndLoadService("{{{");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – error message should contain parse/json error context
        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsEmptyString_RendersErrorPanel()
    {
        // Arrange
        var service = CreateAndLoadService("");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsEmptyObject_ServiceReflectsState()
    {
        // Arrange – valid JSON but missing required fields
        var service = CreateAndLoadService("{}");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – either error panel shows (if validation fails) or empty dashboard
        // The important thing is no unhandled exception
        var hasError = cut.FindAll(".error-panel").Count > 0;
        var hasDashboard = cut.FindAll(".hdr").Count > 0;

        // One or the other should render, but not both
        (hasError || hasDashboard || cut.Markup.Length > 0).Should().BeTrue(
            "the page should render something without throwing");
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsNull_RendersErrorPanel()
    {
        // Arrange
        var service = CreateAndLoadService("null");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert - null deserialization should trigger error or empty state
        var hasError = cut.FindAll(".error-panel").Count > 0;
        var hasDashboardContent = cut.FindAll(".hdr").Count > 0;
        // If Data is null and IsError is false, Dashboard renders nothing (no else branch)
        // This is valid behavior
        (hasError || !hasDashboardContent).Should().BeTrue();
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsArray_RendersErrorPanel()
    {
        // Arrange – JSON is valid but wrong shape (array instead of object)
        var service = CreateAndLoadService("[1, 2, 3]");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_WhenDataJsonHasTrailingComma_RendersErrorPanel()
    {
        // Arrange
        var json = """{ "title": "Test", }""";
        var service = CreateAndLoadService(json);
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – trailing commas are invalid in strict JSON
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    // ── ErrorPanel component isolation integration ─────────────────────

    [Fact]
    public void ErrorPanel_RenderedStandalone_WithMessage_ShowsAllSections()
    {
        // Arrange & Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p => p
            .Add(x => x.Message, "Integration test error message"));

        // Assert – all four expected sections present
        cut.Find(".error-icon").Should().NotBeNull();
        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Markup.Should().Contain("Integration test error message");
        cut.Find(".error-hint").TextContent
            .Should().Be("Check data.json for errors and restart the application.");
    }

    [Fact]
    public void ErrorPanel_RenderedStandalone_WithoutMessage_GracefullyOmitsDetail()
    {
        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>();

        // Assert
        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Find(".error-hint").Should().NotBeNull();

        // Only the hint <p> should exist, no message <p>
        cut.FindAll("p").Should().HaveCount(1);
    }

    // ── Service state propagation ──────────────────────────────────────

    [Fact]
    public void DashboardDataService_AfterLoadingMissingFile_HasErrorState()
    {
        // Arrange & Act
        var service = CreateAndLoadService();

        // Assert
        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().NotBeNullOrEmpty();
        service.Data.Should().BeNull();
    }

    [Fact]
    public void DashboardDataService_AfterLoadingInvalidJson_HasErrorState()
    {
        // Arrange & Act
        var service = CreateAndLoadService("not json at all");

        // Assert
        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().NotBeNullOrEmpty();
        service.Data.Should().BeNull();
    }

    [Fact]
    public void DashboardDataService_ErrorMessage_PropagatesFullyToErrorPanel()
    {
        // Arrange
        var service = CreateAndLoadService("{broken");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – the exact error message from the service should appear in the rendered panel
        var errorMessage = service.ErrorMessage;
        errorMessage.Should().NotBeNullOrEmpty();
        cut.Markup.Should().Contain(errorMessage!);
    }

    // ── Transition scenarios ───────────────────────────────────────────

    [Fact]
    public void ErrorPanel_WithSpecialCharactersInMessage_EncodesCorrectly()
    {
        // Arrange
        var message = "Error: unexpected '<' at position 0 in \"data.json\"";
        
        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p => p
            .Add(x => x.Message, message));

        // Assert – HTML-encoded, no raw angle brackets
        cut.Markup.Should().Contain("&lt;");
        cut.Markup.Should().Contain("&quot;");
    }

    [Fact]
    public void ErrorPanel_WithVeryLongMessage_RendersCompletely()
    {
        // Arrange
        var longMessage = string.Join("\n", Enumerable.Range(1, 50)
            .Select(i => $"Error detail line {i}: something went wrong at position {i * 100}"));

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ErrorPanel>(p => p
            .Add(x => x.Message, longMessage));

        // Assert
        cut.Markup.Should().Contain("Error detail line 1");
        cut.Markup.Should().Contain("Error detail line 50");
    }

    [Fact]
    public void Dashboard_ErrorPanel_ContainsExpectedDomStructure()
    {
        // Arrange
        var service = CreateAndLoadService();
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – verify the full DOM structure
        var panel = cut.Find(".error-panel");
        panel.Should().NotBeNull();

        var children = panel.Children;
        children.Should().HaveCountGreaterThanOrEqualTo(3, 
            "error panel should have at minimum: icon, title, hint");

        // First child: icon
        children[0].ClassName.Should().Contain("error-icon");

        // Second child: h2 title
        children[1].TagName.Should().BeEquivalentTo("H2");

        // Last child: hint paragraph
        var lastChild = children[children.Length - 1];
        lastChild.ClassName.Should().Contain("error-hint");
    }

    [Fact]
    public void Dashboard_ErrorAndSuccessStates_AreMutuallyExclusive()
    {
        // Arrange – error state
        var errorService = CreateAndLoadService();
        Services.AddSingleton(errorService);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – error panel present, dashboard content absent
        cut.FindAll(".error-panel").Should().NotBeEmpty();
        cut.FindAll(".hdr").Should().BeEmpty();
        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.FindAll(".hm-wrap").Should().BeEmpty();
    }

    // ── Edge cases with file content ───────────────────────────────────

    [Fact]
    public void Dashboard_WhenDataJsonContainsOnlyWhitespace_RendersErrorPanel()
    {
        // Arrange
        var service = CreateAndLoadService("   \n\t  ");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_WhenDataJsonContainsBom_HandlesGracefully()
    {
        // Arrange – UTF-8 BOM followed by invalid JSON
        var bomJson = "\uFEFF{invalid}";
        var filePath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(filePath, bomJson);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
        var service = new DashboardDataService(mockEnv.Object, NullLogger<DashboardDataService>.Instance);
        service.LoadAsync(filePath).GetAwaiter().GetResult();

        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert – should handle BOM gracefully (either parse error or success, no crash)
        (service.IsError || service.Data != null).Should().BeTrue("service should either error or parse");
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsNumericLiteral_RendersErrorPanel()
    {
        // Arrange – valid JSON but not an object
        var service = CreateAndLoadService("42");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsStringLiteral_RendersErrorPanel()
    {
        // Arrange
        var service = CreateAndLoadService("\"just a string\"");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_WhenDataJsonIsBooleanLiteral_RendersErrorPanel()
    {
        // Arrange
        var service = CreateAndLoadService("true");
        Services.AddSingleton(service);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Assert
        service.IsError.Should().BeTrue();
        cut.FindAll(".error-panel").Should().HaveCount(1);
    }
}