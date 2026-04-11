using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Shared fixture for integration tests. Creates <see cref="WebApplicationFactory{T}"/>-backed
/// <see cref="HttpClient"/> instances with controllable data.json content.
/// Implements <see cref="IAsyncLifetime"/> to clean up all temp directories on disposal.
/// </summary>
public class WebAppFixture : IAsyncLifetime
{
    private readonly List<string> _tempDirectories = [];
    private readonly List<WebApplicationFactory<Program>> _factories = [];
    private readonly object _lock = new();

    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        // Dispose all factories first so file handles are released
        lock (_lock)
        {
            foreach (var factory in _factories)
            {
                try { factory.Dispose(); } catch { /* best-effort */ }
            }
            _factories.Clear();
        }

        // Delete all temp directories
        lock (_lock)
        {
            foreach (var dir in _tempDirectories)
            {
                try
                {
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, recursive: true);
                }
                catch
                {
                    // Best-effort cleanup; CI runners may hold locks briefly
                }
            }
            _tempDirectories.Clear();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> backed by a test server whose
    /// data.json content is controlled by <paramref name="dataJson"/>.
    /// Pass <c>null</c> to simulate a missing data file.
    /// The temp directory is tracked and cleaned up in <see cref="DisposeAsync"/>.
    /// </summary>
    public HttpClient CreateInitializedClient(string? dataJson = null)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"DashboardIntTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        lock (_lock)
        {
            _tempDirectories.Add(tempDir);
        }

        var dataDir = Path.Combine(tempDir, "data");
        Directory.CreateDirectory(dataDir);

        if (dataJson is not null)
        {
            File.WriteAllText(Path.Combine(dataDir, "data.json"), dataJson);
        }

        // Ensure wwwroot/css exists for static file middleware
        var cssDir = Path.Combine(tempDir, "wwwroot", "css");
        Directory.CreateDirectory(cssDir);

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseContentRoot(tempDir);
                builder.UseSetting("Dashboard:DataFilePath", "data/data.json");
                builder.UseEnvironment("Development");
            });

        lock (_lock)
        {
            _factories.Add(factory);
        }

        return factory.CreateClient();
    }

    /// <summary>
    /// Creates a client pre-loaded with a valid sample data.json.
    /// </summary>
    public HttpClient CreateClientWithValidData()
    {
        var sampleData = new
        {
            project = new { name = "Test Project", lead = "Alice", status = "On Track", lastUpdated = "2026-04-01", summary = "Integration test data" },
            milestones = new[] { new { title = "M1", targetDate = "2026-05-01", status = "Completed" } },
            shipped = new[] { new { title = "Feature A", description = "Done", category = "Core", percentComplete = 100 } },
            inProgress = new[] { new { title = "Feature B", percentComplete = 50 } },
            carriedOver = new[] { new { title = "Feature C", carryOverReason = "Delayed" } },
            currentMonth = new { month = "April", totalItems = 10, completedItems = 7, carriedItems = 2, overallHealth = "On Track" }
        };

        return CreateInitializedClient(JsonSerializer.Serialize(sampleData, CamelCaseOptions));
    }

    /// <summary>
    /// Creates a client with no data.json to simulate missing-file scenario.
    /// </summary>
    public HttpClient CreateClientWithMissingData()
    {
        return CreateInitializedClient(dataJson: null);
    }

    /// <summary>
    /// Creates a client with malformed JSON in data.json.
    /// </summary>
    public HttpClient CreateClientWithMalformedData()
    {
        return CreateInitializedClient("{ this is not valid json !!! }");
    }
}