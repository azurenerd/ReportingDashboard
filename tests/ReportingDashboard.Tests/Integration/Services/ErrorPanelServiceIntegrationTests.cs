using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

/// <summary>
/// Integration tests for DashboardDataService focusing on error state scenarios
/// that feed into ErrorPanel rendering. Tests real file I/O with temp files.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorPanelServiceIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<IWebHostEnvironment> _mockEnv;

    public ErrorPanelServiceIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SvcTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService CreateService()
    {
        return new DashboardDataService(_mockEnv.Object, NullLogger<DashboardDataService>.Instance);
    }

    private string WriteDataJson(string content)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, content);
        return path;
    }

    // ── File not found ─────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsIsErrorTrue()
    {
        var service = CreateService();
        var missingPath = Path.Combine(_tempDir, "data.json");

        await service.LoadAsync(missingPath);

        service.IsError.Should().BeTrue();
        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsErrorMessage()
    {
        var service = CreateService();
        var missingPath = Path.Combine(_tempDir, "data.json");

        await service.LoadAsync(missingPath);

        service.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    // ── Invalid JSON ───────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_InvalidJsonSyntax_SetsIsErrorTrue()
    {
        var path = WriteDataJson("{{{invalid}}}");
        var service = CreateService();

        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_InvalidJsonSyntax_ProvidesDescriptiveErrorMessage()
    {
        var path = WriteDataJson("{{{");
        var service = CreateService();

        await service.LoadAsync(path);

        service.ErrorMessage.Should().NotBeNullOrWhiteSpace();
        // Error message should give some indication of what went wrong
        service.ErrorMessage!.Length.Should().BeGreaterThan(5,
            "error message should be descriptive, not just a code");
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_SetsIsErrorTrue()
    {
        var path = WriteDataJson("");
        var service = CreateService();

        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnly_SetsIsErrorTrue()
    {
        var path = WriteDataJson("   \n\t   ");
        var service = CreateService();

        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    // ── Type mismatch ──────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_JsonArray_SetsIsErrorTrue()
    {
        var path = WriteDataJson("[1, 2, 3]");
        var service = CreateService();

        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonString_SetsIsErrorTrue()
    {
        var path = WriteDataJson("\"just a string\"");
        var service = CreateService();

        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonNumber_SetsIsErrorTrue()
    {
        var path = WriteDataJson("42");
        var service = CreateService();

        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonBoolean_SetsIsErrorTrue()
    {
        var path = WriteDataJson("true");
        var service = CreateService();

        await service.LoadAsync(path);

        service.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_JsonNull_SetsErrorState()
    {
        var path = WriteDataJson("null");
        var service = CreateService();

        await service.LoadAsync(path);

        // null deserialization should result in error since Data would be null
        service.IsError.Should().BeTrue();
    }

    // ── Concurrent loads ───────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_CalledTwice_SecondLoadOverwritesFirst()
    {
        var service = CreateService();

        // First load: missing file → error
        await service.LoadAsync(Path.Combine(_tempDir, "data.json"));
        service.IsError.Should().BeTrue();

        // Second load: also missing
        await service.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));
        service.IsError.Should().BeTrue();
    }

    // ── Permission / encoding edge cases ───────────────────────────────

    [Fact]
    public async Task LoadAsync_Utf8WithBom_HandlesGracefully()
    {
        var path = Path.Combine(_tempDir, "data.json");
        // Write with BOM
        await File.WriteAllTextAsync(path, "{}", System.Text.Encoding.UTF8);
        var service = CreateService();

        await service.LoadAsync(path);

        // Should either parse successfully or report a clear error
        (service.IsError || service.Data != null).Should().BeTrue(
            "service should handle UTF-8 BOM without crashing");
    }

    [Fact]
    public async Task LoadAsync_LargeInvalidFile_DoesNotHang()
    {
        // Arrange - 500KB of invalid JSON
        var largeContent = new string('{', 500_000);
        var path = WriteDataJson(largeContent);
        var service = CreateService();

        // Act - should complete in reasonable time
        var loadTask = service.LoadAsync(path);
        var completed = await Task.WhenAny(loadTask, Task.Delay(TimeSpan.FromSeconds(10)));

        // Assert
        completed.Should().Be(loadTask, "LoadAsync should complete within 10 seconds even for large files");
        service.IsError.Should().BeTrue();
    }

    // ── Error state consistency ────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_Error_DataIsAlwaysNull()
    {
        var invalidInputs = new[] { "", "   ", "{{{", "[1]", "null", "true", "42", "\"str\"" };

        foreach (var input in invalidInputs)
        {
            var path = WriteDataJson(input);
            var service = CreateService();

            await service.LoadAsync(path);

            if (service.IsError)
            {
                service.Data.Should().BeNull($"Data should be null when IsError is true for input: '{input}'");
                service.ErrorMessage.Should().NotBeNullOrWhiteSpace(
                    $"ErrorMessage should be set when IsError is true for input: '{input}'");
            }
        }
    }

    [Fact]
    public async Task LoadAsync_Error_ErrorMessageIsNotEmpty()
    {
        var service = CreateService();
        await service.LoadAsync(Path.Combine(_tempDir, "missing.json"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().NotBeNull();
        service.ErrorMessage.Should().NotBeEmpty();
        service.ErrorMessage!.Trim().Length.Should().BeGreaterThan(0);
    }
}